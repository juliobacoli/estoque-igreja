using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Commands.RemoverItemSocial;

public record RemoverItemSocialCommand(Guid Id) : IRequest<ItemSocialRemovidoResult>;

/// <summary>
/// <c>Removido</c> diz se o item saiu do banco de vez (nunca foi movimentado) ou
/// se apenas ficou inativo, preservando o histórico.
/// </summary>
public record ItemSocialRemovidoResult(bool Removido);
