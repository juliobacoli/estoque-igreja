using MediatR;

namespace EstoqueIgreja.Application.Itens.Queries.ListarItens;

/// <summary>
/// Por padrão devolve só itens ativos. O filtro do Histórico precisa dos inativos
/// também, senão o histórico de um item removido fica inalcançável.
/// </summary>
public record ListarItensQuery(bool IncluirInativos = false) : IRequest<IReadOnlyList<ItemListado>>;

public record ItemListado(Guid Id, string Nome, string Unidade, int EstoqueAtual);
