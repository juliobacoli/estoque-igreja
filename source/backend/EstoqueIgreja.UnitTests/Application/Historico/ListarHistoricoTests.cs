using EstoqueIgreja.Application.Historico.Queries.ListarHistorico;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;
using FluentValidation.TestHelper;

namespace EstoqueIgreja.UnitTests.Application.Historico;

public class ListarHistoricoQueryValidatorTests
{
    private readonly ListarHistoricoQueryValidator _validator = new();

    [Fact]
    public void PaginaMenorQueUm_RetornaErro()
    {
        var resultado = _validator.TestValidate(new ListarHistoricoQuery(null, 0, 20));

        resultado.ShouldHaveValidationErrorFor(x => x.Pagina);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void TamanhoPaginaForaDoLimite_RetornaErro(int tamanho)
    {
        var resultado = _validator.TestValidate(new ListarHistoricoQuery(null, 1, tamanho));

        resultado.ShouldHaveValidationErrorFor(x => x.TamanhoPagina);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void TamanhoPaginaNosLimites_EhValido(int tamanho)
    {
        var resultado = _validator.TestValidate(new ListarHistoricoQuery(null, 1, tamanho));

        resultado.ShouldNotHaveAnyValidationErrors();
    }
}

public class ListarHistoricoQueryHandlerTests
{
    private static readonly DateTime Base = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly AppDbContext _db = BancoEmMemoria.Criar();
    private readonly Usuario _admin = Usuario.Criar("admin", "hash", PerfilUsuario.Admin);
    private readonly Item _sabao = Item.Criar("Sabão", "unidade");
    private readonly Item _esponja = Item.Criar("Esponja", "unidade");

    public ListarHistoricoQueryHandlerTests()
    {
        _db.Usuarios.Add(_admin);
        _db.Itens.AddRange(_sabao, _esponja);
        _db.SaveChanges();
    }

    private ListarHistoricoQueryHandler CriarHandler() => new(_db);

    private AtualizacaoEstoque Registrar(Item item, DateTime data, int quantidadeNova = 1, Guid? id = null)
    {
        var registro = AtualizacaoEstoque.Criar(item.Id, 0, quantidadeNova, _admin.Id);
        Reflexao.Definir(registro, nameof(AtualizacaoEstoque.Data), data);

        if (id is not null)
            Reflexao.Definir(registro, nameof(AtualizacaoEstoque.Id), id.Value);

        _db.AtualizacoesEstoque.Add(registro);
        return registro;
    }

    [Fact]
    public async Task SemFiltro_RetornaRegistrosDeTodosOsItens()
    {
        Registrar(_sabao, Base);
        Registrar(_esponja, Base.AddMinutes(1));
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new ListarHistoricoQuery(null, 1, 20), CancellationToken.None);

        Assert.Equal(2, resultado.Registros.Count);
    }

    [Fact]
    public async Task ComItemId_RetornaApenasRegistrosDoItem()
    {
        Registrar(_sabao, Base);
        Registrar(_esponja, Base.AddMinutes(1));
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new ListarHistoricoQuery(_sabao.Id, 1, 20), CancellationToken.None);

        var registro = Assert.Single(resultado.Registros);
        Assert.Equal("Sabão", registro.ItemNome);
    }

    [Fact]
    public async Task OrdenaPorDataDecrescenteEDepoisPorIdDecrescente()
    {
        Registrar(_sabao, Base, quantidadeNova: 1);
        Registrar(_sabao, Base.AddMinutes(5), quantidadeNova: 2, id: Guid.Parse("00000000-0000-0000-0000-000000000001"));
        Registrar(_sabao, Base.AddMinutes(5), quantidadeNova: 3, id: Guid.Parse("00000000-0000-0000-0000-000000000002"));
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new ListarHistoricoQuery(null, 1, 20), CancellationToken.None);

        Assert.Equal([3, 2, 1], resultado.Registros.Select(r => r.QuantidadeNova));
    }

    [Fact]
    public async Task MaisRegistrosQueAPagina_IndicaQueHaMais()
    {
        for (var i = 0; i < 3; i++)
        {
            Registrar(_sabao, Base.AddMinutes(i));
        }
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new ListarHistoricoQuery(null, 1, 2), CancellationToken.None);

        Assert.True(resultado.TemMaisPaginas);
        Assert.Equal(2, resultado.Registros.Count);
    }

    [Fact]
    public async Task RegistrosIguaisAoTamanhoDaPagina_NaoIndicaQueHaMais()
    {
        Registrar(_sabao, Base);
        Registrar(_sabao, Base.AddMinutes(1));
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new ListarHistoricoQuery(null, 1, 2), CancellationToken.None);

        Assert.False(resultado.TemMaisPaginas);
        Assert.Equal(2, resultado.Registros.Count);
    }

    [Fact]
    public async Task SegundaPagina_PulaRegistrosDaPrimeira()
    {
        for (var i = 1; i <= 3; i++)
        {
            Registrar(_sabao, Base.AddMinutes(i), quantidadeNova: i);
        }
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new ListarHistoricoQuery(null, 2, 2), CancellationToken.None);

        var registro = Assert.Single(resultado.Registros);
        Assert.Equal(1, registro.QuantidadeNova);
        Assert.False(resultado.TemMaisPaginas);
    }

    [Fact]
    public async Task PreencheNomeDoItemInativoEPerfilDoUsuario()
    {
        _sabao.Inativar();
        Registrar(_sabao, Base);
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new ListarHistoricoQuery(null, 1, 20), CancellationToken.None);

        var registro = Assert.Single(resultado.Registros);
        Assert.Equal("Sabão", registro.ItemNome);
        Assert.Equal("Admin", registro.Perfil);
        Assert.Equal(Base, registro.Data);
    }
}
