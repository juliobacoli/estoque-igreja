namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Common;

public record ItemParaCalculo(string Nome, string Unidade, int QuantidadePorCesta, int EstoqueAtual);

public record FaltaParaCesta(string Nome, string Unidade, int Tem, int Precisa);

public static class CapacidadeDaCesta
{
    /// <summary>Quantas cestas o estoque atual monta: o item que acaba primeiro manda.</summary>
    public static int PodeMontar(IReadOnlyCollection<ItemParaCalculo> itens)
    {
        return itens.Count == 0 ? 0 : itens.Min(i => i.EstoqueAtual / i.QuantidadePorCesta);
    }

    /// <summary>O que falta, item a item, para montar <paramref name="cestas"/> cestas.</summary>
    public static IReadOnlyList<FaltaParaCesta> Faltas(IEnumerable<ItemParaCalculo> itens, int cestas)
    {
        return itens
            .Select(i => new FaltaParaCesta(i.Nome, i.Unidade, i.EstoqueAtual, i.QuantidadePorCesta * cestas))
            .Where(f => f.Tem < f.Precisa)
            .ToList();
    }
}
