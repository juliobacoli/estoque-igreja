using EstoqueIgreja.Api.Contracts.Requests;
using EstoqueIgreja.Application.Itens.Commands.AtualizarEstoque;
using EstoqueIgreja.Application.Itens.Commands.CriarItem;
using EstoqueIgreja.Application.Itens.Queries.ListarItens;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Route("api/itens")]
public class ItensController : ControllerBase
{
    private readonly ISender _mediator;

    public ItensController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> Listar(CancellationToken ct)
    {
        return Ok(await _mediator.Send(new ListarItensQuery(), ct));
    }

    /// <summary>
    /// Restrito a Admin no servidor — esconder a opção no menu do Angular é
    /// cosmético, a barreira real é esta.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] CriarItemCommand command, CancellationToken ct)
    {
        var item = await _mediator.Send(command, ct);

        return StatusCode(StatusCodes.Status201Created, item);
    }

    [HttpPut("{id:guid}/estoque")]
    public async Task<IActionResult> AtualizarEstoque(
        Guid id,
        [FromBody] AtualizarEstoqueRequest request,
        CancellationToken ct)
    {
        var resultado = await _mediator.Send(
            new AtualizarEstoqueCommand(id, request.NovaQuantidade), ct);

        return Ok(resultado);
    }
}
