namespace EstoqueIgreja.Domain.Entities;

public class ModeloCestaItem
{
    private ModeloCestaItem() { }

    public Guid Id { get; private set; }
    public Guid ModeloCestaId { get; private set; }
    public Guid ItemSocialId { get; private set; }

    /// <summary>Quanto do item vai em cada cesta, na unidade do item.</summary>
    public int Quantidade { get; private set; }

    public ItemSocial ItemSocial { get; private set; } = null!;

    internal static ModeloCestaItem Criar(Guid modeloCestaId, Guid itemSocialId, int quantidade)
    {
        return new ModeloCestaItem
        {
            Id = Guid.NewGuid(),
            ModeloCestaId = modeloCestaId,
            ItemSocialId = itemSocialId,
            Quantidade = quantidade
        };
    }
}
