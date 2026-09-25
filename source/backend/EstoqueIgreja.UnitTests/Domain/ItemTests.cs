using EstoqueIgreja.Domain.Entities;

namespace EstoqueIgreja.UnitTests.Domain;

public class ItemTests
{
    [Fact]
    public void Criar_RemoveEspacosDoNomeEDaUnidade()
    {
        var item = Item.Criar("  Papel Toalha  ", "  rolo ");

        Assert.Equal("Papel Toalha", item.Nome);
        Assert.Equal("rolo", item.Unidade);
    }

    [Fact]
    public void Criar_IniciaAtivoComEstoqueZerado()
    {
        var item = Item.Criar("Sabão", "unidade");

        Assert.Equal(0, item.EstoqueAtual);
        Assert.True(item.Ativo);
        Assert.NotEqual(Guid.Empty, item.Id);
        Assert.Equal(DateTimeKind.Utc, item.CriadoEm.Kind);
    }

    [Fact]
    public void Criar_PreencheNomeNormalizado()
    {
        var item = Item.Criar("Papel Higiênico", "rolo");

        Assert.Equal("papel higienico", item.NomeNormalizado);
    }

    [Theory]
    [InlineData("Papel Higiênico", "papel higienico")]
    [InlineData("  ÁLCOOL EM GEL ", "alcool em gel")]
    [InlineData("Sabão", "sabao")]
    [InlineData("Detergente 500ml - neutro", "detergente 500ml - neutro")]
    public void Normalizar_RemoveAcentosEspacosDasPontasEMaiusculas(string entrada, string esperado)
        => Assert.Equal(esperado, Item.Normalizar(entrada));

    [Fact]
    public void Normalizar_NomesComESemAcentoColidem()
        => Assert.Equal(Item.Normalizar("Álcool"), Item.Normalizar("alcool"));

    [Theory]
    [InlineData(1)]
    [InlineData(250)]
    public void AtualizarQuantidade_ValorPositivo_AtualizaEstoque(int quantidade)
    {
        var item = Item.Criar("Sabão", "unidade");

        item.AtualizarQuantidade(quantidade);

        Assert.Equal(quantidade, item.EstoqueAtual);
    }

    [Fact]
    public void AtualizarQuantidade_Zero_EhAceito()
    {
        var item = Item.Criar("Sabão", "unidade");
        item.AtualizarQuantidade(5);

        item.AtualizarQuantidade(0);

        Assert.Equal(0, item.EstoqueAtual);
    }

    [Fact]
    public void AtualizarQuantidade_Negativa_LancaExcecaoSemAlterarEstoque()
    {
        var item = Item.Criar("Sabão", "unidade");
        item.AtualizarQuantidade(3);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.AtualizarQuantidade(-1));
        Assert.Equal(3, item.EstoqueAtual);
    }

    [Fact]
    public void Inativar_DesativaItem()
    {
        var item = Item.Criar("Sabão", "unidade");

        item.Inativar();

        Assert.False(item.Ativo);
    }

    [Fact]
    public void Reativar_AtivaZeraEstoqueESubstituiNomeEUnidade()
    {
        var item = Item.Criar("sabao", "pacote");
        item.AtualizarQuantidade(7);
        item.Inativar();

        item.Reativar("  Sabão  ", " unidade ");

        Assert.True(item.Ativo);
        Assert.Equal(0, item.EstoqueAtual);
        Assert.Equal("Sabão", item.Nome);
        Assert.Equal("unidade", item.Unidade);
    }

    [Fact]
    public void Reativar_RecalculaNomeNormalizado()
    {
        var item = Item.Criar("Sabao", "pacote");
        item.Inativar();

        item.Reativar("Sabão Líquido", "unidade");

        Assert.Equal("sabao liquido", item.NomeNormalizado);
    }

    [Fact]
    public void Reativar_MantemMesmoId()
    {
        var item = Item.Criar("Sabão", "pacote");
        var idOriginal = item.Id;
        item.Inativar();

        item.Reativar("Sabão", "unidade");

        Assert.Equal(idOriginal, item.Id);
    }
}
