using EstoqueIgreja.Api.Contracts.Requests;
using EstoqueIgreja.Api.Services;
using EstoqueIgreja.Application.AcaoSocial.Itens.Commands.CriarItemSocial;
using EstoqueIgreja.Application.AcaoSocial.Itens.Commands.RemoverItemSocial;
using EstoqueIgreja.Application.AcaoSocial.Itens.Queries.ListarItensSociais;
using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.AjustarEstoqueSocial;
using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.RegistrarEntrada;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Authorize(Policy = Politicas.AcaoSocial)]
[Route("api/acao-social")]
public class AcaoSocialController(ISender mediator) : ControllerBase
{
    private readonly ISender _mediator = mediator;

    [HttpGet("itens")]
    public async Task<IActionResult> ListarItens(CancellationToken ct)
        => Ok(await _mediator.Send(new ListarItensSociaisQuery(), ct));

    [Authorize(Roles = "Admin")]
    [HttpPost("itens")]
    public async Task<IActionResult> CriarItem([FromBody] CriarItemSocialCommand command, CancellationToken ct)
    {
        var item = await _mediator.Send(command, ct);

        return StatusCode(StatusCodes.Status201Created, item);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("itens/{id:guid}")]
    public async Task<IActionResult> RemoverItem(Guid id, CancellationToken ct)
        => Ok(await _mediator.Send(new RemoverItemSocialCommand(id), ct));

    [HttpPost("itens/{id:guid}/entradas")]
    public async Task<IActionResult> RegistrarEntrada(
        Guid id,
        [FromBody] RegistrarEntradaRequest request,
        CancellationToken ct)
        => Ok(await _mediator.Send(new RegistrarEntradaCommand(id, request.Quantidade), ct));

    [HttpPost("itens/{id:guid}/ajustes")]
    public async Task<IActionResult> Ajustar(
        Guid id,
        [FromBody] AjustarEstoqueSocialRequest request,
        CancellationToken ct)
        => Ok(await _mediator.Send(new AjustarEstoqueSocialCommand(id, request.NovaQuantidade, request.Motivo), ct));
}
