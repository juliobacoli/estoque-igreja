using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Common;
using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.AjustarEstoqueSocial;

/// <summary>
/// Correção do estoque (perda, vencido, contagem errada): informa a quantidade
/// que existe de fato, com o motivo.
/// </summary>
public record AjustarEstoqueSocialCommand(Guid ItemId, int NovaQuantidade, string Motivo)
    : IRequest<EstoqueSocialAlteradoResult>;
