using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class BancoTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task Migrations_CriamTabelasEIndices()
    {
        var pendentes = await NoBanco(db => db.Database.GetPendingMigrationsAsync());
        Assert.Empty(pendentes);

        await using var conexao = await AbrirConexao();
        await using var comando = new NpgsqlCommand("SELECT indexname FROM pg_indexes WHERE schemaname = 'public'", conexao);
        await using var leitor = await comando.ExecuteReaderAsync();

        var indices = new List<string>();
        while (await leitor.ReadAsync())
            indices.Add(leitor.GetString(0));

        Assert.Contains("IX_Itens_NomeNormalizado", indices);
        Assert.Contains("IX_Usuarios_Login", indices);
        Assert.Contains("IX_AtualizacoesEstoque_ItemId_Data", indices);
        Assert.Contains("IX_AtualizacoesEstoque_Data", indices);
        Assert.Contains("IX_AtualizacoesEstoque_UsuarioId", indices);
    }

    [Fact]
    public async Task MigrationAdicionaItemAtivo_MarcaItensExistentesComoAtivos()
    {
        // Banco separado: a migration é aplicada em duas etapas, com um item gravado no meio.
        var nomeBanco = $"migracao_{Guid.NewGuid():N}";

        await using (var conexao = await AbrirConexao())
        await using (var criar = new NpgsqlCommand($"CREATE DATABASE \"{nomeBanco}\"", conexao))
            await criar.ExecuteNonQueryAsync();

        var connectionString = new NpgsqlConnectionStringBuilder(Factory.ConnectionString) { Database = nomeBanco }.ConnectionString;
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options);

        await db.GetService<IMigrator>().MigrateAsync("20260814143630_Inicial");

        var itemId = Guid.NewGuid();
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO \"Itens\" (\"Id\", \"Nome\", \"NomeNormalizado\", \"Unidade\", \"EstoqueAtual\", \"CriadoEm\") VALUES ({itemId}, 'Sabão', 'sabao', 'unidade', 3, now())");

        await db.Database.MigrateAsync();

        var ativo = await db.Database
            .SqlQuery<bool>($"SELECT \"Ativo\" AS \"Value\" FROM \"Itens\" WHERE \"Id\" = {itemId}")
            .SingleAsync();

        Assert.True(ativo);
    }

    [Fact]
    public async Task IndiceUnico_RecusaItensComMesmoNomeNormalizado()
    {
        await NoBanco(async db =>
        {
            db.Itens.Add(Item.Criar("Sabão", "unidade"));
            await db.SaveChangesAsync();
        });

        await AssertViolacao(PostgresErrorCodes.UniqueViolation, () => NoBanco(async db =>
        {
            db.Itens.Add(Item.Criar("SABAO", "pacote"));
            await db.SaveChangesAsync();
        }));
    }

    [Fact]
    public async Task IndiceUnico_RecusaUsuariosComMesmoLogin()
    {
        await AssertViolacao(PostgresErrorCodes.UniqueViolation, () => NoBanco(async db =>
        {
            db.Usuarios.Add(Usuario.Criar("admin", "outro-hash", PerfilUsuario.Admin));
            await db.SaveChangesAsync();
        }));
    }

    [Fact]
    public async Task ChaveEstrangeira_ImpedeExcluirItemComHistorico()
    {
        var item = await CriarItemComHistorico();

        await AssertViolacao(PostgresErrorCodes.ForeignKeyViolation, () => NoBanco(db =>
            db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Itens\" WHERE \"Id\" = {item.Id}")));
    }

    [Fact]
    public async Task ChaveEstrangeira_ImpedeExcluirUsuarioComHistorico()
    {
        await CriarItemComHistorico();

        await AssertViolacao(PostgresErrorCodes.ForeignKeyViolation, () => NoBanco(db =>
            db.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM \"Usuarios\" WHERE \"Id\" = {Admin.Id}")));
    }

    [Fact]
    public async Task Perfil_EhGravadoComoTextoELidoDeVolta()
    {
        var gravado = await NoBanco(db => db.Database
            .SqlQuery<string>($"SELECT \"Perfil\" AS \"Value\" FROM \"Usuarios\" WHERE \"Id\" = {Admin.Id}")
            .SingleAsync());

        var lido = await NoBanco(db => db.Usuarios.AsNoTracking().SingleAsync(u => u.Id == Admin.Id));

        Assert.Equal("Admin", gravado);
        Assert.Equal(PerfilUsuario.Admin, lido.Perfil);
    }

    private async Task<Item> CriarItemComHistorico()
    {
        var item = Item.Criar("Sabão", "unidade");

        await NoBanco(async db =>
        {
            db.Itens.Add(item);
            db.AtualizacoesEstoque.Add(AtualizacaoEstoque.Criar(item.Id, 0, 1, Admin.Id));
            await db.SaveChangesAsync();
        });

        return item;
    }
}
