using MediatR;

namespace EstoqueIgreja.Application.Itens.Queries.ListarItens;

public record ListarItensQuery : IRequest<IReadOnlyList<ItemListado>>;

public record ItemListado(Guid Id, string Nome, string Unidade, int EstoqueAtual);
