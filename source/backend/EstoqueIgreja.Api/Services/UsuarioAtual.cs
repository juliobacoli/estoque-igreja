using System.Security.Claims;
using EstoqueIgreja.Application.Common.Interfaces;

namespace EstoqueIgreja.Api.Services;

public class UsuarioAtual(IHttpContextAccessor acessor) : IUsuarioAtual
{
    private readonly IHttpContextAccessor _acessor = acessor;

    public Guid Id
    {
        get
        {
            var valor = _acessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(valor, out var id)
                ? id
                : throw new InvalidOperationException("Requisição sem usuário autenticado.");
        }
    }
}
