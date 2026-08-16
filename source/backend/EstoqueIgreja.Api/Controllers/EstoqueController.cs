using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Route("api/estoque")]
public class EstoqueController : ControllerBase
{
    private readonly ISender _mediator;

    public EstoqueController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("exportar-pdf")]
    public async Task<IActionResult> ExportarPdf(CancellationToken ct)
    {
        var pdf = await _mediator.Send(new ExportarEstoquePdfQuery(), ct);

        return File(pdf, "application/pdf", "estoque.pdf");
    }
}
