namespace EstoqueIgreja.Domain.Entities;

/// <summary>
/// Composição da cesta básica e quantas cestas já estão montadas. Existe um só,
/// com Id fixo: duas criações simultâneas batem na chave primária em vez de
/// gerar dois modelos.
/// </summary>
public class ModeloCesta
{
    public static readonly Guid IdUnico = new("00000000-0000-0000-0000-00000000ce57");

    private readonly List<ModeloCestaItem> _itens = [];

    private ModeloCesta() { }

    public Guid Id { get; private set; }
    public int CestasProntas { get; private set; }
    public IReadOnlyList<ModeloCestaItem> Itens => _itens;

    public static ModeloCesta Criar() => new() { Id = IdUnico, CestasProntas = 0 };

    /// <summary>Substitui a composição inteira. Mudar o modelo não altera montagens antigas.</summary>
    public void DefinirItens(IEnumerable<(Guid ItemSocialId, int Quantidade)> itens)
    {
        var lista = itens.ToList();

        if (lista.Count == 0)
            throw new ArgumentException("A cesta precisa de pelo menos um item.", nameof(itens));

        if (lista.Any(i => i.Quantidade <= 0))
            throw new ArgumentOutOfRangeException(nameof(itens), "A quantidade de cada item precisa ser maior que zero.");

        if (lista.Select(i => i.ItemSocialId).Distinct().Count() != lista.Count)
            throw new ArgumentException("Um item aparece mais de uma vez na cesta.", nameof(itens));

        _itens.Clear();
        _itens.AddRange(lista.Select(i => ModeloCestaItem.Criar(Id, i.ItemSocialId, i.Quantidade)));
    }

    public void AdicionarCestasProntas(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade precisa ser maior que zero.");

        CestasProntas += quantidade;
    }
}
