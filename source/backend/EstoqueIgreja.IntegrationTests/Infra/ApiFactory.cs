using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace EstoqueIgreja.IntegrationTests.Infra;

/// <summary>
/// Sobe a API real contra um PostgreSQL em container, compartilhado por todos os testes.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ConteudoIndex = "<!doctype html><title>SPA de teste</title>";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();
    private readonly string _webRoot = Path.Combine(Path.GetTempPath(), $"estoque-webroot-{Guid.NewGuid():N}");

    public string ConnectionString => _postgres.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        // O wwwroot só existe na imagem Docker; aqui um index.html falso cobre o fallback do SPA.
        Directory.CreateDirectory(_webRoot);
        await File.WriteAllTextAsync(Path.Combine(_webRoot, "index.html"), ConteudoIndex);

        // Variáveis de ambiente, e não UseSetting: o Program.cs lê a connection string
        // antes do Build, quando as configurações da factory ainda não foram aplicadas.
        Environment.SetEnvironmentVariable("ConnectionStrings__DefaultConnection", ConnectionString);
        Environment.SetEnvironmentVariable("ASPNETCORE_WEBROOT", _webRoot);
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();

        Directory.Delete(_webRoot, recursive: true);
    }
}

[CollectionDefinition(Nome)]
public class ApiCollection : ICollectionFixture<ApiFactory>
{
    public const string Nome = "Api";
}
