using EstoqueIgreja.Application.Estoque.Queries.ObterUltimaAtualizacao;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;

namespace EstoqueIgreja.UnitTests.Application.Estoque;

public class ObterUltimaAtualizacaoQueryHandlerTests
{
    private readonly AppDbContext _db = BancoEmMemoria.Criar();

    private Task<UltimaAtualizacaoResult> Executar() =>
        new ObterUltimaAtualizacaoQueryHandler(_db).Handle(new ObterUltimaAtualizacaoQuery(), CancellationToken.None);

    [Fact]
    public async Task SemContagens_RetornaDataNula()
    {
        var resultado = await Executar();

        Assert.Null(resultado.Data);
    }

    [Fact]
    public async Task ComContagens_RetornaDataMaisRecente()
    {
        var item = Item.Criar("Sabão", "unidade");
        var usuario = Usuario.Criar("admin", "hash", PerfilUsuario.Admin);
        var maisRecente = new DateTime(2026, 9, 10, 15, 0, 0, DateTimeKind.Utc);

        var antiga = AtualizacaoEstoque.Criar(item.Id, 0, 1, usuario.Id);
        var recente = AtualizacaoEstoque.Criar(item.Id, 1, 2, usuario.Id);
        Reflexao.Definir(antiga, nameof(AtualizacaoEstoque.Data), maisRecente.AddDays(-3));
        Reflexao.Definir(recente, nameof(AtualizacaoEstoque.Data), maisRecente);

        _db.Itens.Add(item);
        _db.Usuarios.Add(usuario);
        _db.AtualizacoesEstoque.AddRange(recente, antiga);
        await _db.SaveChangesAsync();

        var resultado = await Executar();

        Assert.Equal(maisRecente, resultado.Data);
    }
}
