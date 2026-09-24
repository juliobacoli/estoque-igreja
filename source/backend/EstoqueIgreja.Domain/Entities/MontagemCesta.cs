namespace EstoqueIgreja.Domain.Entities;

/// <summary>
/// Registro de uma montagem. O que cada item perdeu fica nas
/// <see cref="MovimentacaoSocial"/> ligadas a ela, com as quantidades daquele
/// momento: mudar o modelo depois não reescreve o histórico.
/// </summary>
public class MontagemCesta
{
    private MontagemCesta() { }

    public Guid Id { get; private set; }
    public int Quantidade { get; private set; }
    public int CestasAnteriores { get; private set; }
    public int CestasNovas { get; private set; }
    public Guid UsuarioId { get; private set; }
    public DateTime Data { get; private set; }

    public Usuario Usuario { get; private set; } = null!;

    public static MontagemCesta Criar(int quantidade, int cestasAnteriores, Guid usuarioId)
    {
        return new MontagemCesta
        {
            Id = Guid.NewGuid(),
            Quantidade = quantidade,
            CestasAnteriores = cestasAnteriores,
            CestasNovas = cestasAnteriores + quantidade,
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow
        };
    }
}
