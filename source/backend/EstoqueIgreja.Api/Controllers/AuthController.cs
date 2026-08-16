using System.Security.Claims;
using EstoqueIgreja.Application.Auth.Commands.Login;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken ct)
    {
        var resultado = await _mediator.Send(command, ct);

        // Mesma resposta para login inexistente e senha errada — não revela qual
        // dos dois campos está incorreto.
        if (resultado is null)
        {
            return Unauthorized(new { error = "Login ou senha inválidos" });
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, resultado.UsuarioId.ToString()),
            new(ClaimTypes.Role, resultado.Perfil)
        };

        var identidade = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidade),
            new AuthenticationProperties { IsPersistent = true });

        return Ok(new { perfil = resultado.Perfil });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Ok();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new { perfil = User.FindFirstValue(ClaimTypes.Role) });
    }
}
