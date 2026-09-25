using EstoqueIgreja.Domain.Enums;

namespace EstoqueIgreja.Domain.Entities;

/// <summary>
/// Toda mudança no estoque da Ação Social. Guarda as quantidades de antes e de
/// depois para o histórico não depender do estoque atual.
/// </summary>
public class MovimentacaoSocial
{
    private MovimentacaoSocial() { }

    public Guid Id { get; private set; }
    public Guid ItemSocialId { get; private set; }
    public TipoMovimentacaoSocial Tipo { get; private set; }
    public int QuantidadeAnterior { get; private set; }
    public int QuantidadeNova { get; private set; }

    /// <summary>Por que o estoque foi corrigido, só nos ajustes. Obrigatório neles.</summary>
    public string? Motivo { get; private set; }

    /// <summary>Montagem que consumiu o item, só nas movimentações de montagem.</summary>
    public Guid? MontagemCestaId { get; private set; }

    public Guid UsuarioId { get; private set; }
    public DateTime Data { get; private set; }

    public ItemSocial ItemSocial { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    public static MovimentacaoSocial Entrada(
        Guid itemSocialId, int quantidadeAnterior, int quantidadeNova, Guid usuarioId)
        => Criar(itemSocialId, TipoMovimentacaoSocial.Entrada, quantidadeAnterior, quantidadeNova, usuarioId);

    public static MovimentacaoSocial Ajuste(
        Guid itemSocialId, int quantidadeAnterior, int quantidadeNova, string motivo, Guid usuarioId)
    {
        return Criar(itemSocialId, TipoMovimentacaoSocial.Ajuste, quantidadeAnterior, quantidadeNova, usuarioId,
            motivo: motivo.Trim());
    }

    public static MovimentacaoSocial Montagem(
        Guid itemSocialId, int quantidadeAnterior, int quantidadeNova, Guid montagemCestaId, Guid usuarioId)
    {
        var movimentacao = Criar(itemSocialId, TipoMovimentacaoSocial.Montagem, quantidadeAnterior, quantidadeNova, usuarioId);
        movimentacao.MontagemCestaId = montagemCestaId;
        return movimentacao;
    }

    private static MovimentacaoSocial Criar(
        Guid itemSocialId,
        TipoMovimentacaoSocial tipo,
        int quantidadeAnterior,
        int quantidadeNova,
        Guid usuarioId,
        string? motivo = null)
    {
        return new MovimentacaoSocial
        {
            Id = Guid.NewGuid(),
            ItemSocialId = itemSocialId,
            Tipo = tipo,
            QuantidadeAnterior = quantidadeAnterior,
            QuantidadeNova = quantidadeNova,
            Motivo = motivo,
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow
        };
    }
}
