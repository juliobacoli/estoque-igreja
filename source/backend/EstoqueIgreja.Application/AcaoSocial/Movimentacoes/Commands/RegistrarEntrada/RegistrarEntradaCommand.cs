using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Common;
using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.RegistrarEntrada;

/// <summary>Doação recebida: soma a quantidade ao estoque.</summary>
public record RegistrarEntradaCommand(Guid ItemId, int Quantidade)
    : IRequest<EstoqueSocialAlteradoResult>;
