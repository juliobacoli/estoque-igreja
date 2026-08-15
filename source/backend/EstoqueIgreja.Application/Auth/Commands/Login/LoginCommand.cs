using MediatR;

namespace EstoqueIgreja.Application.Auth.Commands.Login;

/// <summary>
/// Retorna null quando as credenciais não conferem — o controller traduz isso
/// para 401 com mensagem genérica.
/// </summary>
public record LoginCommand(string Login, string Senha) : IRequest<LoginResult?>;

public record LoginResult(Guid UsuarioId, string Perfil);
