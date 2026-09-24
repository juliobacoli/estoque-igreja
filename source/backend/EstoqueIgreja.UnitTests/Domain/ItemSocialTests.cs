using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;

namespace EstoqueIgreja.UnitTests.Domain;

public class ItemSocialTests
{
    [Fact]
    public void Criar_IniciaAtivoComEstoqueZeradoENomeNormalizado()
    {
        var item = ItemSocial.Criar("  Feijão Preto ", " kg ");

        Assert.Equal("Feijão Preto", item.Nome);
        Assert.Equal("feijao preto", item.NomeNormalizado);
        Assert.Equal("kg", item.Unidade);
        Assert.Equal(0, item.EstoqueAtual);
        Assert.True(item.Ativo);
    }

    [Fact]
    public void Adicionar_SomaAoEstoque()
    {
        var item = ItemSocial.Criar("Arroz", "kg");

        item.Adicionar(5);
        item.Adicionar(3);

        Assert.Equal(8, item.EstoqueAtual);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Adicionar_QuantidadeNaoPositiva_Lanca(int quantidade)
    {
        var item = ItemSocial.Criar("Arroz", "kg");

        Assert.Throws<ArgumentOutOfRangeException>(() => item.Adicionar(quantidade));
        Assert.Equal(0, item.EstoqueAtual);
    }

    [Fact]
    public void Ajustar_DefineAQuantidade()
    {
        var item = ItemSocial.Criar("Arroz", "kg");
        item.Adicionar(10);

        item.Ajustar(4);

        Assert.Equal(4, item.EstoqueAtual);
    }

    [Fact]
    public void Ajustar_Negativo_Lanca()
    {
        var item = ItemSocial.Criar("Arroz", "kg");

        Assert.Throws<ArgumentOutOfRangeException>(() => item.Ajustar(-1));
    }

    [Fact]
    public void Reativar_ZeraEstoqueETrocaUnidade()
    {
        var item = ItemSocial.Criar("Arroz", "kg");
        item.Adicionar(10);
        item.Inativar();

        item.Reativar("arroz", "pacote");

        Assert.True(item.Ativo);
        Assert.Equal(0, item.EstoqueAtual);
        Assert.Equal("pacote", item.Unidade);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData(" Mercado Bom ", "Mercado Bom")]
    public void Entrada_GuardaDoadorSemEspacosOuNulo(string? doador, string? esperado)
    {
        var movimentacao = MovimentacaoSocial.Entrada(Guid.NewGuid(), 0, 5, doador, Guid.NewGuid());

        Assert.Equal(TipoMovimentacaoSocial.Entrada, movimentacao.Tipo);
        Assert.Equal(esperado, movimentacao.Doador);
        Assert.Null(movimentacao.Motivo);
    }

    [Fact]
    public void Ajuste_GuardaMotivoEQuantidades()
    {
        var itemId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var movimentacao = MovimentacaoSocial.Ajuste(itemId, 10, 7, " Venceu ", usuarioId);

        Assert.Equal(TipoMovimentacaoSocial.Ajuste, movimentacao.Tipo);
        Assert.Equal("Venceu", movimentacao.Motivo);
        Assert.Null(movimentacao.Doador);
        Assert.Equal(10, movimentacao.QuantidadeAnterior);
        Assert.Equal(7, movimentacao.QuantidadeNova);
        Assert.Equal(itemId, movimentacao.ItemSocialId);
        Assert.Equal(usuarioId, movimentacao.UsuarioId);
        Assert.Equal(DateTimeKind.Utc, movimentacao.Data.Kind);
    }
}
