namespace EstoqueIgreja.Domain.Entities;

public class AtualizacaoEstoque
{
    private AtualizacaoEstoque() { }

    public Guid Id { get; private set; }
    public Guid ItemId { get; private set; }
    public int QuantidadeAnterior { get; private set; }
    public int QuantidadeNova { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTime Data { get; private set; }

    public Item Item { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    public static AtualizacaoEstoque Criar(
        Guid itemId,
        int quantidadeAnterior,
        int quantidadeNova,
        Guid usuarioId)
    {
        return new AtualizacaoEstoque
        {
            Id = Guid.NewGuid(),
            ItemId = itemId,
            QuantidadeAnterior = quantidadeAnterior,
            QuantidadeNova = quantidadeNova,
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow
        };
    }
}
