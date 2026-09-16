using System.Security.Claims;
using EstoqueIgreja.Api.Services;
using Microsoft.AspNetCore.Http;

namespace EstoqueIgreja.UnitTests.Api;

public class UsuarioAtualTests
{
    private static UsuarioAtual Criar(HttpContext? contexto) =>
        new(new HttpContextAccessor { HttpContext = contexto });

    private static DefaultHttpContext ContextoComClaim(string? valor)
    {
        var claims = valor is null ? [] : new[] { new Claim(ClaimTypes.NameIdentifier, valor) };

        return new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Cookies")) };
    }

    [Fact]
    public void ClaimComGuidValido_RetornaId()
    {
        var id = Guid.NewGuid();

        Assert.Equal(id, Criar(ContextoComClaim(id.ToString())).Id);
    }

    [Fact]
    public void SemClaim_LancaExcecao()
    {
        Assert.Throws<InvalidOperationException>(() => Criar(ContextoComClaim(null)).Id);
    }

    [Fact]
    public void ClaimComGuidInvalido_LancaExcecao()
    {
        Assert.Throws<InvalidOperationException>(() => Criar(ContextoComClaim("nao-e-guid")).Id);
    }

    [Fact]
    public void SemHttpContext_LancaExcecao()
    {
        Assert.Throws<InvalidOperationException>(() => Criar(null).Id);
    }
}
