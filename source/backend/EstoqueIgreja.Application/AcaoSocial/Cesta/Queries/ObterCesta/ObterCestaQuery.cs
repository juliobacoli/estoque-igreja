using EstoqueIgreja.Application.AcaoSocial.Cesta.Common;
using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Queries.ObterCesta;

public record ObterCestaQuery : IRequest<CestaResumo>;

/// <summary>
/// <c>FaltasParaProxima</c> diz o que falta para montar uma cesta além das que
/// o estoque já permite; serve para a equipe saber o que pedir de doação.
/// </summary>
public record CestaResumo(
    int CestasProntas,
    int PodeMontar,
    IReadOnlyList<ItemDaCesta> Itens,
    IReadOnlyList<FaltaParaCesta> FaltasParaProxima);

public record ItemDaCesta(Guid ItemId, string Nome, string Unidade, int QuantidadePorCesta, int EstoqueAtual);
