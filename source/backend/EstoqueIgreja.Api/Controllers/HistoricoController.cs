using EstoqueIgreja.Application.Historico.Queries.ListarHistorico;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Route("api/historico")]
public class HistoricoController : ControllerBase
{
    private readonly ISender _mediator;

    public HistoricoController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? itemId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken ct = default)
    {
        return Ok(await _mediator.Send(new ListarHistoricoQuery(itemId, pagina, tamanhoPagina), ct));
    }
}
