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

    /// <summary>Quem doou, só nas entradas. Opcional.</summary>
    public string? Doador { get; private set; }

    /// <summary>Por que o estoque foi corrigido, só nos ajustes. Obrigatório neles.</summary>
    public string? Motivo { get; private set; }

    public Guid UsuarioId { get; private set; }
    public DateTime Data { get; private set; }

    public ItemSocial ItemSocial { get; private set; } = null!;
    public Usuario Usuario { get; private set; } = null!;

    public static MovimentacaoSocial Entrada(
        Guid itemSocialId, int quantidadeAnterior, int quantidadeNova, string? doador, Guid usuarioId)
    {
        return Criar(itemSocialId, TipoMovimentacaoSocial.Entrada, quantidadeAnterior, quantidadeNova, usuarioId,
            doador: string.IsNullOrWhiteSpace(doador) ? null : doador.Trim());
    }

    public static MovimentacaoSocial Ajuste(
        Guid itemSocialId, int quantidadeAnterior, int quantidadeNova, string motivo, Guid usuarioId)
    {
        return Criar(itemSocialId, TipoMovimentacaoSocial.Ajuste, quantidadeAnterior, quantidadeNova, usuarioId,
            motivo: motivo.Trim());
    }

    private static MovimentacaoSocial Criar(
        Guid itemSocialId,
        TipoMovimentacaoSocial tipo,
        int quantidadeAnterior,
        int quantidadeNova,
        Guid usuarioId,
        string? doador = null,
        string? motivo = null)
    {
        return new MovimentacaoSocial
        {
            Id = Guid.NewGuid(),
            ItemSocialId = itemSocialId,
            Tipo = tipo,
            QuantidadeAnterior = quantidadeAnterior,
            QuantidadeNova = quantidadeNova,
            Doador = doador,
            Motivo = motivo,
            UsuarioId = usuarioId,
            Data = DateTime.UtcNow
        };
    }
}
