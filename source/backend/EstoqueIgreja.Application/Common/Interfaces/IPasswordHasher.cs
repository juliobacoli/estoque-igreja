namespace EstoqueIgreja.Application.Common.Interfaces;

public interface IPasswordHasher
{
    string Hash(string senha);

    bool Verificar(string senha, string hash);
}
