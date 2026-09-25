using EstoqueIgreja.Api.Services;
using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using EstoqueIgreja.Application.Estoque.Queries.ObterUltimaAtualizacao;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Authorize(Policy = Politicas.Obreiros)]
[Route("api/estoque")]
public class EstoqueController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpGet("exportar-pdf")]
    public async Task<IActionResult> ExportarPdf(CancellationToken ct)
    {
        var pdf = await _mediator.Send(new ExportarEstoquePdfQuery(), ct);

        return File(pdf, "application/pdf", "estoque.pdf");
    }

    [HttpGet("ultima-atualizacao")]
    public async Task<IActionResult> UltimaAtualizacao(CancellationToken ct)
        => Ok(await _mediator.Send(new ObterUltimaAtualizacaoQuery(), ct));
}
