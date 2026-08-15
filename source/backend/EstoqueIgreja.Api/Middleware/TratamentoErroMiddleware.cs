using EstoqueIgreja.Application.Common.Exceptions;
using FluentValidation;

namespace EstoqueIgreja.Api.Middleware;

/// <summary>
/// Traduz exceções em respostas no formato { "error": "..." }, que é o contrato
/// usado por toda a API (Capítulo 3, item 3.5).
/// </summary>
public class TratamentoErroMiddleware
{
    private readonly RequestDelegate _proximo;
    private readonly ILogger<TratamentoErroMiddleware> _logger;

    public TratamentoErroMiddleware(RequestDelegate proximo, ILogger<TratamentoErroMiddleware> logger)
    {
        _proximo = proximo;
        _logger = logger;
    }

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
            _logger.LogError(ex, "Erro não tratado em {Caminho}", contexto.Request.Path);

            // Mensagem genérica: detalhe do erro fica só no log do servidor.
            await Responder(contexto, 500, new { error = "Erro interno" });
        }
    }

    private static async Task Responder(HttpContext contexto, int status, object corpo)
    {
        if (contexto.Response.HasStarted)
        {
            return;
        }

        contexto.Response.Clear();
        contexto.Response.StatusCode = status;
        contexto.Response.ContentType = "application/json";

        await contexto.Response.WriteAsJsonAsync(corpo);
    }
}
