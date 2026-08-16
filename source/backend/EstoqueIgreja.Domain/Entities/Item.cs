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

    public static Item Criar(string nome, string unidade)
    {
        return new Item
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            NomeNormalizado = Normalizar(nome),
            Unidade = unidade.Trim(),
            EstoqueAtual = 0,
            CriadoEm = DateTime.UtcNow
        };
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
