using EstoqueIgreja.Api.Services;
using EstoqueIgreja.Application.Historico.Queries.ListarHistorico;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Authorize(Policy = Politicas.Obreiros)]
[Route("api/historico")]
public class HistoricoController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Listar(
        [FromQuery] Guid? itemId,
        [FromQuery] int pagina = 1,
        [FromQuery] int tamanhoPagina = 20,
        CancellationToken ct = default)
        => Ok(await _mediator.Send(new ListarHistoricoQuery(itemId, pagina, tamanhoPagina), ct));
}
