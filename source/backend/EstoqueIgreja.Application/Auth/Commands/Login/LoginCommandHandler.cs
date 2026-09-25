using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Auth.Commands.Login;

public class LoginCommandHandler(IAppDbContext db, IPasswordHasher hasher) : IRequestHandler<LoginCommand, LoginResult?>
{
    private readonly IAppDbContext _db = db;
    private readonly IPasswordHasher _hasher = hasher;

    public async Task<LoginResult?> Handle(LoginCommand request, CancellationToken ct)
    {
        var login = request.Login.Trim().ToLowerInvariant();

        var usuario = await _db.Usuarios
            .FirstOrDefaultAsync(u => u.Login == login, ct);

        // Retorna null tanto para login inexistente quanto para senha errada — o
        // controller devolve a mesma mensagem nos dois casos, sem revelar qual
        // dos campos está incorreto.
        if (usuario is null || !_hasher.Verificar(request.Senha, usuario.SenhaHash))
            return null;

        return new LoginResult(
            usuario.Id,
            usuario.Login,
            usuario.Perfil.ToString(),
            [.. usuario.Modulos().Select(m => m.ToString())]);
    }
}
