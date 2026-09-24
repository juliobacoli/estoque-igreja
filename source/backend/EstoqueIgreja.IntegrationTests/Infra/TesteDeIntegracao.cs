using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EstoqueIgreja.Application.Itens.Commands.CriarItem;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace EstoqueIgreja.IntegrationTests.Infra;

/// <summary>
/// Cada teste começa com o banco limpo e com as contas fixas: admin (Obreiros), social (Ação Social) e
/// semacesso (nenhum módulo). As três usam a mesma senha.
/// </summary>
public abstract class TesteDeIntegracao : IAsyncLifetime
{
    protected const string SenhaAdmin = "senha-admin";

    // BCrypt é lento de propósito; o hash é gerado uma vez só para a execução inteira.
    private static readonly Lazy<string> HashAdmin = new(() => new PasswordHasher().Hash(SenhaAdmin));

    protected TesteDeIntegracao(ApiFactory factory)
    {
        Factory = factory;
    }

    protected ApiFactory Factory { get; }
    protected Usuario Admin { get; private set; } = null!;
    protected Usuario Social { get; private set; } = null!;
    protected Usuario SemAcesso { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Admin = Usuario.Criar("admin", HashAdmin.Value, PerfilUsuario.Admin, Modulo.Obreiros);
        Social = Usuario.Criar("social", HashAdmin.Value, PerfilUsuario.Admin, Modulo.AcaoSocial);
        SemAcesso = Usuario.Criar("semacesso", HashAdmin.Value, PerfilUsuario.Admin);

        await NoBanco(async db =>
        {
            await db.Database.ExecuteSqlRawAsync(
                "TRUNCATE TABLE \"ModeloCestaItens\", \"ModelosCesta\", \"MovimentacoesSociais\", \"MontagensCesta\", \"ItensSociais\", \"AtualizacoesEstoque\", \"Itens\", \"Usuarios\" CASCADE");

            db.Usuarios.AddRange(Admin, Social, SemAcesso);
            await db.SaveChangesAsync();
        });
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // ---- Banco ----

    protected async Task NoBanco(Func<AppDbContext, Task> acao)
    {
        using var escopo = Factory.Services.CreateScope();
        await acao(escopo.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    protected async Task<T> NoBanco<T>(Func<AppDbContext, Task<T>> consulta)
    {
        using var escopo = Factory.Services.CreateScope();
        return await consulta(escopo.ServiceProvider.GetRequiredService<AppDbContext>());
    }

    protected async Task<NpgsqlConnection> AbrirConexao()
    {
        var conexao = new NpgsqlConnection(Factory.ConnectionString);
        await conexao.OpenAsync();
        return conexao;
    }

    protected static async Task Executar(NpgsqlConnection conexao, string sql, Guid id)
    {
        await using var comando = new NpgsqlCommand(sql, conexao);
        comando.Parameters.AddWithValue("id", id);
        await comando.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Espera até alguma sessão do banco ficar parada aguardando um lock — sinal de que a
    /// requisição chegou no ponto de conflito com a transação aberta pelo teste.
    /// </summary>
    protected async Task AguardarBloqueio()
    {
        await using var conexao = await AbrirConexao();
        var limite = DateTime.UtcNow.AddSeconds(15);

        while (DateTime.UtcNow < limite)
        {
            await using var comando = new NpgsqlCommand("SELECT count(*) FROM pg_locks WHERE NOT granted", conexao);

            if ((long)(await comando.ExecuteScalarAsync())! > 0)
                return;

            await Task.Delay(50);
        }

        throw new TimeoutException("A requisição não chegou a esperar pelo lock.");
    }

    protected static async Task AssertViolacao(string sqlState, Func<Task> acao)
    {
        var excecao = await Assert.ThrowsAnyAsync<Exception>(acao);
        var postgres = excecao as PostgresException ?? excecao.InnerException as PostgresException;

        Assert.NotNull(postgres);
        Assert.Equal(sqlState, postgres.SqlState);
    }

    // ---- HTTP ----

    protected WebApplicationFactory<Program> FactoryCom(Action<IServiceCollection> servicos) =>
        Factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(servicos));

    protected HttpClient ClienteAnonimo(WebApplicationFactory<Program>? factory = null) =>
        (factory ?? Factory).CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

    protected Task<HttpClient> ClienteAdmin(WebApplicationFactory<Program>? factory = null) =>
        Logar(factory, "admin", SenhaAdmin);

    protected Task<HttpClient> ClienteSocial(WebApplicationFactory<Program>? factory = null) =>
        Logar(factory, "social", SenhaAdmin);

    private async Task<HttpClient> Logar(WebApplicationFactory<Program>? factory, string login, string senha)
    {
        var cliente = ClienteAnonimo(factory);
        var resposta = await cliente.PostAsJsonAsync("/auth/login", new { login, senha });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        return cliente;
    }

    protected static async Task<ItemCriadoResult> CriarItem(HttpClient admin, string nome, string unidade = "unidade")
    {
        var resposta = await admin.PostAsJsonAsync("/api/itens", new { nome, unidade });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<ItemCriadoResult>())!;
    }

    protected static Task<HttpResponseMessage> AtualizarEstoque(HttpClient cliente, Guid itemId, int quantidade) =>
        cliente.PutAsJsonAsync($"/api/itens/{itemId}/estoque", new { novaQuantidade = quantidade });

    protected static async Task<JsonElement> LerJson(HttpResponseMessage resposta) =>
        await resposta.Content.ReadFromJsonAsync<JsonElement>();
}
