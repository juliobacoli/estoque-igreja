using EstoqueIgreja.Application.Common.Interfaces;

namespace EstoqueIgreja.Infrastructure.Services;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha);

    public bool Verificar(string senha, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(senha, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // Hash gravado em formato inválido — trata como credencial incorreta
            // em vez de derrubar a requisição com 500.
            return false;
        }
    }
}
