using System.Text.Json;
using EstoqueIgreja.Api.Middleware;
using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.UnitTests.Helpers;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EstoqueIgreja.UnitTests.Api;

public class TratamentoErroMiddlewareTests
{
    private sealed class RespostaJaIniciada : HttpResponseFeature
    {
        public override bool HasStarted => true;
    }

    private static async Task<(DefaultHttpContext Contexto, JsonElement? Corpo)> Executar(
        RequestDelegate proximo,
        ILogger<TratamentoErroMiddleware>? logger = null,
        Action<DefaultHttpContext>? preparar = null)
    {
        var contexto = new DefaultHttpContext();
        contexto.Response.Body = new MemoryStream();
        preparar?.Invoke(contexto);

        var middleware = new TratamentoErroMiddleware(proximo, logger ?? NullLogger<TratamentoErroMiddleware>.Instance);
        await middleware.InvokeAsync(contexto);

        var corpo = contexto.Response.Body;
        if (corpo.Length == 0)
            return (contexto, null);

        corpo.Position = 0;
        using var documento = await JsonDocument.ParseAsync(corpo);
        return (contexto, documento.RootElement.Clone());
    }

    [Fact]
    public async Task ValidationException_Retorna400ComPrimeiraMensagemEErrosAgrupados()
    {
        var falhas = new[]
        {
            new ValidationFailure("Nome", "Informe o nome do item."),
            new ValidationFailure("Nome", "Nome muito longo."),
            new ValidationFailure("Unidade", "Informe a unidade.")
        };

        var (contexto, corpo) = await Executar(_ => throw new ValidationException(falhas));

        Assert.Equal(400, contexto.Response.StatusCode);
        Assert.Equal("Informe o nome do item.", corpo!.Value.GetProperty("error").GetString());

        var erros = corpo.Value.GetProperty("errors");
        Assert.Equal(2, erros.GetProperty("Nome").GetArrayLength());
        Assert.Equal(1, erros.GetProperty("Unidade").GetArrayLength());
    }

    [Fact]
    public async Task ConflitoException_Retorna400ComMensagem()
    {
        var (contexto, corpo) = await Executar(_ => throw new ConflitoException("Já existe um item com esse nome"));

        Assert.Equal(400, contexto.Response.StatusCode);
        Assert.Equal("Já existe um item com esse nome", corpo!.Value.GetProperty("error").GetString());
    }

    [Fact]
    public async Task NaoEncontradoException_Retorna404ComMensagem()
    {
        var (contexto, corpo) = await Executar(_ => throw new NaoEncontradoException("Item não encontrado"));

        Assert.Equal(404, contexto.Response.StatusCode);
        Assert.Equal("Item não encontrado", corpo!.Value.GetProperty("error").GetString());
    }

    [Fact]
    public async Task ExcecaoGenerica_Retorna500ERegistraLog()
    {
        var logger = new Mock<ILogger<TratamentoErroMiddleware>>();

        var (contexto, corpo) = await Executar(_ => throw new InvalidOperationException("detalhe interno"), logger.Object);

        Assert.Equal(500, contexto.Response.StatusCode);
        Assert.Equal("Erro interno", corpo!.Value.GetProperty("error").GetString());
        logger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ExcecaoGenerica_CaminhoNaoQuebraLinhaNoLog()
    {
        var logger = new LoggerQueCaptura<TratamentoErroMiddleware>();

        await Executar(
            _ => throw new InvalidOperationException("falha"),
            logger,
            // Pelo feature, o caminho chega bruto, como numa requisição real.
            contexto => contexto.Features.Get<IHttpRequestFeature>()!.Path = "/api/itens\nFAKE: linha forjada");

        // O log fica numa linha só: a tentativa de forjar outra linha não passa.
        var mensagem = Assert.Single(logger.Mensagens);
        Assert.DoesNotContain("\n", mensagem);
        Assert.DoesNotContain("\r", mensagem);
        Assert.Contains("/api/itens", mensagem);
    }

    [Fact]
    public async Task RespostaJaIniciada_NaoEscreveNada()
    {
        var (contexto, corpo) = await Executar(
            _ => throw new NaoEncontradoException("Item não encontrado"),
            preparar: c => c.Features.Set<IHttpResponseFeature>(new RespostaJaIniciada()));

        Assert.Equal(200, contexto.Response.StatusCode);
        Assert.Null(corpo);
    }

    [Fact]
    public async Task SemExcecao_ApenasRepassa()
    {
        var chamado = false;

        var (contexto, corpo) = await Executar(c =>
        {
            chamado = true;
            c.Response.StatusCode = 204;
            return Task.CompletedTask;
        });

        Assert.True(chamado);
        Assert.Equal(204, contexto.Response.StatusCode);
        Assert.Null(corpo);
    }
}
