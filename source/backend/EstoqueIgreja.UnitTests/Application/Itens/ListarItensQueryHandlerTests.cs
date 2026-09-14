using EstoqueIgreja.Application.Itens.Queries.ListarItens;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;

namespace EstoqueIgreja.UnitTests.Application.Itens;

public class ListarItensQueryHandlerTests
{
    private readonly AppDbContext _db = BancoEmMemoria.Criar();

    private ListarItensQueryHandler CriarHandler() => new(_db);

    private async Task AdicionarAtivoEInativo()
    {
        var inativo = Item.Criar("Vassoura", "unidade");
        inativo.Inativar();

        _db.Itens.AddRange(Item.Criar("Sabão", "unidade"), inativo);
        await _db.SaveChangesAsync();
    }

    [Fact]
    public async Task PorPadrao_RetornaApenasAtivos()
    {
        await AdicionarAtivoEInativo();

        var itens = await CriarHandler().Handle(new ListarItensQuery(), CancellationToken.None);

        var item = Assert.Single(itens);
        Assert.Equal("Sabão", item.Nome);
    }

    [Fact]
    public async Task IncluirInativos_RetornaTodos()
    {
        await AdicionarAtivoEInativo();

        var itens = await CriarHandler().Handle(new ListarItensQuery(IncluirInativos: true), CancellationToken.None);

        Assert.Equal(2, itens.Count);
    }

    [Fact]
    public async Task OrdenaPeloNomeSemAcento()
    {
        _db.Itens.AddRange(
            Item.Criar("Sabão", "unidade"),
            Item.Criar("Álcool em gel", "unidade"),
            Item.Criar("Esponja", "unidade"));
        await _db.SaveChangesAsync();

        var itens = await CriarHandler().Handle(new ListarItensQuery(), CancellationToken.None);

        Assert.Equal(["Álcool em gel", "Esponja", "Sabão"], itens.Select(i => i.Nome));
    }

    [Fact]
    public async Task SemItens_RetornaListaVazia()
    {
        var itens = await CriarHandler().Handle(new ListarItensQuery(), CancellationToken.None);

        Assert.Empty(itens);
    }
}
