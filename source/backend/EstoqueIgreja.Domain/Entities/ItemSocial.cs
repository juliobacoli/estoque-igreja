namespace EstoqueIgreja.Domain.Entities;

/// <summary>
/// Item do estoque da Ação Social (arroz, feijão...). Tabela própria, separada
/// dos itens de limpeza dos Obreiros.
/// </summary>
public class ItemSocial
{
    private ItemSocial() { }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = null!;
    public string NomeNormalizado { get; private set; } = null!;
    public string Unidade { get; private set; } = null!;
    public int EstoqueAtual { get; private set; }
    public bool Ativo { get; private set; }
    public DateTime CriadoEm { get; private set; }

    public static ItemSocial Criar(string nome, string unidade)
    {
        return new ItemSocial
        {
            Id = Guid.NewGuid(),
            Nome = nome.Trim(),
            NomeNormalizado = Item.Normalizar(nome),
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
    /// Mesmo comportamento do <see cref="Item.Reativar"/>: recadastrar um nome
    /// removido volta com estoque zerado e a unidade nova.
    /// </summary>
    public void Reativar(string nome, string unidade)
    {
        Nome = nome.Trim();
        NomeNormalizado = Item.Normalizar(nome);
        Unidade = unidade.Trim();
        EstoqueAtual = 0;
        Ativo = true;
    }

    public void Adicionar(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade precisa ser maior que zero.");

        EstoqueAtual += quantidade;
    }

    public void Ajustar(int novaQuantidade)
    {
        if (novaQuantidade < 0)
            throw new ArgumentOutOfRangeException(nameof(novaQuantidade), "A quantidade não pode ser negativa.");

        EstoqueAtual = novaQuantidade;
    }
}
