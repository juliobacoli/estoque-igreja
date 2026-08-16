using System.Globalization;
using System.Text;

namespace EstoqueIgreja.Domain.Entities;

public class Item
{
    private Item() { }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string NomeNormalizado { get; private set; } = null!;
    public string Unidade { get; private set; } = null!;
    public int EstoqueAtual { get; private set; }
    public DateTime CriadoEm { get; private set; }

    /// <summary>
    /// Item inativo some das telas de operação, mas continua existindo para o
    /// Histórico poder exibir o nome dele nos registros antigos.
    /// </summary>
    public bool Ativo { get; private set; }

    public static Item Criar(string nome, string unidade)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            NomeNormalizado = Normalizar(nome),
            Unidade = unidade.Trim(),
            EstoqueAtual = 0,
            Ativo = true,
            CriadoEm = DateTime.UtcNow
        };
    }

    public void Inativar()
    {
        Ativo = false;
    }

    /// <summary>
    /// Reaproveita um item inativo quando o mesmo nome é cadastrado de novo. O
    /// estoque volta zerado e a unidade informada agora substitui a anterior —
    /// quem está recadastrando sabe melhor que o registro antigo.
    /// </summary>
    public void Reativar(string nome, string unidade)
    {
        Nome = nome.Trim();
        NomeNormalizado = Normalizar(nome);
        Unidade = unidade.Trim();
        EstoqueAtual = 0;
        Ativo = true;
    }

    public void AtualizarQuantidade(int novaQuantidade)
    {
        if (novaQuantidade < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(novaQuantidade), "A quantidade não pode ser negativa.");
        }

        EstoqueAtual = novaQuantidade;
    }

    /// <summary>
    /// Remove acentos e passa para minúsculas, para que "Papel Higiênico" e
    /// "papel higienico" colidam no índice único.
    /// </summary>
    public static string Normalizar(string texto)
    {
        var semAcento = texto
            .Trim()
            .Normalize(NormalizationForm.FormD)
            .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            .ToArray();

        return new string(semAcento)
            .Normalize(NormalizationForm.FormC)
            .ToLowerInvariant();
    }
}
