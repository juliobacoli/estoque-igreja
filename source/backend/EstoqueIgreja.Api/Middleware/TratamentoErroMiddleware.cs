using EstoqueIgreja.Application.Common.Exceptions;
using FluentValidation;

namespace EstoqueIgreja.Api.Middleware;

public class TratamentoErroMiddleware(RequestDelegate proximo, ILogger<TratamentoErroMiddleware> logger)
{
    private readonly RequestDelegate _proximo = proximo;
    private readonly ILogger<TratamentoErroMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _proximo(contexto);
        }
        catch (ValidationException ex)
        {
            var erros = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await Responder(contexto, 400, new
            {
                error = ex.Errors.FirstOrDefault()?.ErrorMessage ?? "Requisição inválida",
                errors = erros
            });
        }
        catch (ConflitoException ex)
        {
            await Responder(contexto, 400, new { error = ex.Message });
        }
        catch (NaoEncontradoException ex)
        {
            await Responder(contexto, 404, new { error = ex.Message });
        }
        catch (Exception ex)
        {
            // O caminho vem do usuário: sem tirar as quebras de linha, dá para forjar
            // linhas inteiras no log e atrapalhar a investigação de um incidente.
            var caminho = contexto.Request.Path.ToString()
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            _logger.LogError(ex, "Erro não tratado em {Caminho}", caminho);

            await Responder(contexto, 500, new { error = "Erro interno" });
        }
    }

    private static async Task Responder(HttpContext contexto, int status, object corpo)
    {
        if (contexto.Response.HasStarted)
            return;

        contexto.Response.Clear();
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = "application/json";

        await contexto.Response.WriteAsJsonAsync(corpo);
    }
}
