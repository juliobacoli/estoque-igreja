using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;
using Moq;

namespace EstoqueIgreja.UnitTests.Application.Estoque;

public class ExportarEstoquePdfQueryHandlerTests
{
    private readonly AppDbContext _db = BancoEmMemoria.Criar();
    private readonly Mock<IGeradorPdf> _gerador = new();
    private readonly byte[] _pdf = [1, 2, 3];

    private IReadOnlyList<ItemDoRelatorio> _itensEnviados = [];

    public ExportarEstoquePdfQueryHandlerTests()
    {
        _gerador
            .Setup(g => g.GerarRelatorioEstoque(It.IsAny<IReadOnlyList<ItemDoRelatorio>>(), It.IsAny<DateTime>()))
            .Callback<IReadOnlyList<ItemDoRelatorio>, DateTime>((itens, _) => _itensEnviados = itens)
            .Returns(_pdf);
    }

    private Task<byte[]> Executar() =>
        new ExportarEstoquePdfQueryHandler(_db, _gerador.Object).Handle(new ExportarEstoquePdfQuery(), CancellationToken.None);

    [Fact]
    public async Task EnviaApenasItensAtivos()
    {
        var inativo = Item.Criar("Vassoura", "unidade");
        inativo.Inativar();
        _db.Itens.AddRange(Item.Criar("Sabão", "unidade"), inativo);
        await _db.SaveChangesAsync();

        await Executar();

        var item = Assert.Single(_itensEnviados);
        Assert.Equal("Sabão", item.Nome);
    }

    [Fact]
    public async Task EnviaItensOrdenadosPeloNomeSemAcento()
    {
        _db.Itens.AddRange(
            Item.Criar("Sabão", "unidade"),
            Item.Criar("Álcool em gel", "unidade"),
            Item.Criar("Esponja", "unidade"));
        await _db.SaveChangesAsync();

        await Executar();

        Assert.Equal(["Álcool em gel", "Esponja", "Sabão"], _itensEnviados.Select(i => i.Nome));
    }

    [Fact]
    public async Task RetornaBytesDoGerador()
    {
        var resultado = await Executar();

        Assert.Same(_pdf, resultado);
    }
}
