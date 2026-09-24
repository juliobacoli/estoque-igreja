using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.AspNetCore.Mvc.Testing;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class AuthTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task Login_CredenciaisValidas_RetornaPerfilECookieSeguro()
    {
        // Sem controle de cookies no cliente, para o Set-Cookie chegar intacto na resposta.
        var cliente = Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

        var resposta = await cliente.PostAsJsonAsync("/auth/login", new { login = "admin", senha = SenhaAdmin });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("Admin", (await LerJson(resposta)).GetProperty("perfil").GetString());

        var cookie = Assert.Single(resposta.Headers.GetValues("Set-Cookie"));
        Assert.StartsWith("estoque.auth.v2=", cookie);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=lax", cookie, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("fulano", SenhaAdmin)]
    [InlineData("admin", "senha-errada")]
    public async Task Login_CredenciaisInvalidas_Retorna401ComMesmaMensagem(string login, string senha)
    {
        var resposta = await ClienteAnonimo().PostAsJsonAsync("/auth/login", new { login, senha });

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Equal("Login ou senha inválidos", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Login_CamposVazios_Retorna400NoFormatoDoMiddleware()
    {
        var resposta = await ClienteAnonimo().PostAsJsonAsync("/auth/login", new { login = "", senha = "" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpo = await LerJson(resposta);
        Assert.Equal("Informe o login.", corpo.GetProperty("error").GetString());
        Assert.True(corpo.GetProperty("errors").TryGetProperty("Login", out _));
        Assert.True(corpo.GetProperty("errors").TryGetProperty("Senha", out _));
    }

    [Fact]
    public async Task Login_ComMaiusculasEEspacos_Autentica()
    {
        var resposta = await ClienteAnonimo().PostAsJsonAsync("/auth/login", new { login = "  ADMIN ", senha = SenhaAdmin });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task Me_ComCookie_RetornaPerfilLoginEModulos()
    {
        var cliente = await ClienteAdmin();

        var resposta = await cliente.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await LerJson(resposta);
        Assert.Equal("Admin", corpo.GetProperty("perfil").GetString());
        Assert.Equal("admin", corpo.GetProperty("login").GetString());
        Assert.Equal(["Obreiros"], corpo.GetProperty("modulos").EnumerateArray().Select(m => m.GetString()));
    }

    [Fact]
    public async Task Login_UsuarioSemModulo_Retorna403SemCookie()
    {
        var cliente = Factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = false });

        var resposta = await cliente.PostAsJsonAsync("/auth/login", new { login = "semacesso", senha = SenhaAdmin });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Equal("Usuário sem acesso", (await LerJson(resposta)).GetProperty("error").GetString());
        Assert.False(resposta.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task Login_UsuarioDaAcaoSocial_RetornaSoOModuloDele()
    {
        var resposta = await ClienteAnonimo().PostAsJsonAsync("/auth/login", new { login = "social", senha = SenhaAdmin });

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await LerJson(resposta);
        Assert.Equal("social", corpo.GetProperty("login").GetString());
        Assert.Equal(["AcaoSocial"], corpo.GetProperty("modulos").EnumerateArray().Select(m => m.GetString()));
    }

    [Theory]
    [InlineData("GET", "/api/itens")]
    [InlineData("POST", "/api/itens")]
    [InlineData("PUT", "/api/itens/00000000-0000-0000-0000-000000000001/estoque")]
    [InlineData("DELETE", "/api/itens/00000000-0000-0000-0000-000000000001")]
    [InlineData("GET", "/api/historico")]
    [InlineData("GET", "/api/estoque/exportar-pdf")]
    [InlineData("GET", "/api/estoque/ultima-atualizacao")]
    public async Task RotasDosObreiros_UsuarioSemOModulo_Retornam403SemRedirect(string metodo, string rota)
    {
        var cliente = await ClienteSocial();

        var resposta = await cliente.SendAsync(new HttpRequestMessage(new HttpMethod(metodo), rota)
        {
            Content = JsonContent.Create(new { nome = "Sabão", unidade = "unidade", novaQuantidade = 1 })
        });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
        Assert.Null(resposta.Headers.Location);
    }

    [Fact]
    public async Task Me_SemCookie_Retorna401SemRedirect()
    {
        var resposta = await ClienteAnonimo().GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
        Assert.Null(resposta.Headers.Location);
    }

    [Fact]
    public async Task Logout_InvalidaSessao()
    {
        var cliente = await ClienteAdmin();

        var logout = await cliente.PostAsync("/auth/logout", null);
        var me = await cliente.GetAsync("/auth/me");

        Assert.Equal(HttpStatusCode.OK, logout.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, me.StatusCode);
    }

    [Theory]
    [InlineData("/api/itens")]
    [InlineData("/api/historico")]
    public async Task RotasDaApi_SemCookie_Retornam401(string rota)
    {
        var resposta = await ClienteAnonimo().GetAsync(rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact]
    public async Task Admin_PodeCadastrarERemover()
    {
        var admin = await ClienteAdmin();

        var item = await CriarItem(admin, "Sabão");
        var remocao = await admin.DeleteAsync($"/api/itens/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, remocao.StatusCode);
    }

    [Fact]
    public async Task Health_SemCookie_Retorna200()
    {
        var resposta = await ClienteAnonimo().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact]
    public async Task HealthDb_ComBancoNoAr_RetornaConectado()
    {
        var resposta = await ClienteAnonimo().GetAsync("/health/db");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("connected", (await LerJson(resposta)).GetProperty("database").GetString());
    }

    [Fact]
    public async Task RotaDoSpa_SemCookie_ServeIndexHtml()
    {
        var resposta = await ClienteAnonimo().GetAsync("/dashboard");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("text/html", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal(ApiFactory.ConteudoIndex, await resposta.Content.ReadAsStringAsync());
    }
}
