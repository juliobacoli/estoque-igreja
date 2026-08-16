using MediatR;

namespace EstoqueIgreja.Application.Itens.Commands.RemoverItem;

public record RemoverItemCommand(Guid Id) : IRequest<ItemRemovidoResult>;

/// <summary>
/// <c>Removido</c> diz se o item saiu do banco de vez (nunca foi contado) ou se
/// apenas ficou inativo, preservando o histórico.
/// </summary>
public record ItemRemovidoResult(bool Removido);
