using MediatR;

namespace EstoqueIgreja.Application.Itens.Commands.AtualizarEstoque;

public record AtualizarEstoqueCommand(Guid ItemId, int NovaQuantidade)
    : IRequest<EstoqueAtualizadoResult>;

public record EstoqueAtualizadoResult(Guid ItemId, int QuantidadeAnterior, int QuantidadeNova);
