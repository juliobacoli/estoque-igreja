using System.Net;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.Extensions.DependencyInjection;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class TratamentoErroTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    private const string DetalheInterno = "Host=db-interno;Password=senha-secreta";

    [Fact]
    public async Task ExcecaoNaoTratada_Retorna500SemDetalhes()
    {
        await using var comFalha = FactoryCom(servicos => servicos.AddSingleton<IGeradorPdf, GeradorQueFalha>());

        var resposta = await (await ClienteAdmin(comFalha)).GetAsync("/api/estoque/exportar-pdf");

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);

        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Equal("Erro interno", (await LerJson(resposta)).GetProperty("error").GetString());
        Assert.DoesNotContain("senha-secreta", corpo);
        Assert.DoesNotContain("   at ", corpo);
    }

    private sealed class GeradorQueFalha : IGeradorPdf
    {
        public byte[] GerarRelatorioEstoque(IReadOnlyList<ItemDoRelatorio> itens, DateTime geradoEmUtc) =>
            throw new InvalidOperationException(DetalheInterno);
    }
}
