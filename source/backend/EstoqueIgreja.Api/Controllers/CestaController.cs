using EstoqueIgreja.Api.Services;
using EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.DefinirModeloCesta;
using EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.MontarCestas;
using EstoqueIgreja.Application.AcaoSocial.Cesta.Queries.ObterCesta;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Authorize(Policy = Politicas.AcaoSocial)]
[Route("api/acao-social/cesta")]
public class CestaController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpGet]
    public async Task<IActionResult> Obter(CancellationToken ct) => Ok(await _mediator.Send(new ObterCestaQuery(), ct));

    [Authorize(Roles = "Admin")]
    [HttpPut("modelo")]
    public async Task<IActionResult> DefinirModelo([FromBody] DefinirModeloCestaCommand command, CancellationToken ct)
    {
        await _mediator.Send(command, ct);

        return NoContent();
    }

    [HttpPost("montagens")]
    public async Task<IActionResult> Montar([FromBody] MontarCestasCommand command, CancellationToken ct)
        => Ok(await _mediator.Send(command, ct));
}
