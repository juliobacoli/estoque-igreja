using EstoqueIgreja.Application.AcaoSocial.Cesta.Common;
using EstoqueIgreja.Domain.Entities;

namespace EstoqueIgreja.UnitTests.Domain;

public class ModeloCestaTests
{
    [Fact]
    public void Criar_UsaOIdUnicoESemCestas()
    {
        var modelo = ModeloCesta.Criar();

        Assert.Equal(ModeloCesta.IdUnico, modelo.Id);
        Assert.Equal(0, modelo.CestasProntas);
        Assert.Empty(modelo.Itens);
    }

    [Fact]
    public void DefinirItens_SubstituiAComposicao()
    {
        var modelo = ModeloCesta.Criar();
        var arroz = Guid.NewGuid();
        var feijao = Guid.NewGuid();

        modelo.DefinirItens([(arroz, 5)]);
        modelo.DefinirItens([(arroz, 2), (feijao, 1)]);

        Assert.Equal([(arroz, 2), (feijao, 1)], modelo.Itens.Select(i => (i.ItemSocialId, i.Quantidade)));
        Assert.All(modelo.Itens, i => Assert.Equal(ModeloCesta.IdUnico, i.ModeloCestaId));
    }

    [Fact]
    public void DefinirItens_Vazio_Lanca()
    {
        Assert.Throws<ArgumentException>(() => ModeloCesta.Criar().DefinirItens([]));
    }

    [Fact]
    public void DefinirItens_QuantidadeZero_Lanca()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ModeloCesta.Criar().DefinirItens([(Guid.NewGuid(), 0)]));
    }

    [Fact]
    public void DefinirItens_ItemRepetido_Lanca()
    {
        var id = Guid.NewGuid();

        Assert.Throws<ArgumentException>(() => ModeloCesta.Criar().DefinirItens([(id, 1), (id, 2)]));
    }

    [Fact]
    public void AdicionarCestasProntas_Soma()
    {
        var modelo = ModeloCesta.Criar();

        modelo.AdicionarCestasProntas(3);
        modelo.AdicionarCestasProntas(2);

        Assert.Equal(5, modelo.CestasProntas);
        Assert.Throws<ArgumentOutOfRangeException>(() => modelo.AdicionarCestasProntas(0));
    }

    [Fact]
    public void Retirar_TiraDoEstoqueENaoDeixaFicarNegativo()
    {
        var item = ItemSocial.Criar("Arroz", "kg");
        item.Adicionar(10);

        item.Retirar(4);

        Assert.Equal(6, item.EstoqueAtual);
        Assert.Throws<InvalidOperationException>(() => item.Retirar(7));
        Assert.Throws<ArgumentOutOfRangeException>(() => item.Retirar(0));
        Assert.Equal(6, item.EstoqueAtual);
    }

    [Fact]
    public void MontagemCesta_CalculaCestasNovas()
    {
        var montagem = MontagemCesta.Criar(3, 2, Guid.NewGuid());

        Assert.Equal((3, 2, 5), (montagem.Quantidade, montagem.CestasAnteriores, montagem.CestasNovas));
    }
}

public class CapacidadeDaCestaTests
{
    private static readonly ItemParaCalculo Arroz = new("Arroz", "kg", 5, 12);
    private static readonly ItemParaCalculo Oleo = new("Óleo", "garrafa", 1, 7);

    [Fact]
    public void PodeMontar_OItemQueAcabaPrimeiroManda()
    {
        Assert.Equal(2, CapacidadeDaCesta.PodeMontar([Arroz, Oleo]));
        Assert.Equal(7, CapacidadeDaCesta.PodeMontar([Oleo]));
    }

    [Fact]
    public void PodeMontar_SemItens_EhZero()
    {
        Assert.Equal(0, CapacidadeDaCesta.PodeMontar([]));
    }

    [Fact]
    public void Faltas_ListaSoOsItensQueNaoChegam()
    {
        var faltas = CapacidadeDaCesta.Faltas([Arroz, Oleo], 3);

        Assert.Equal([new FaltaParaCesta("Arroz", "kg", 12, 15)], faltas);
    }
}
