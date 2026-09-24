using EstoqueIgreja.Domain.Enums;

namespace EstoqueIgreja.Domain.Entities;

public class Usuario
{
    private Usuario() { }

    public Guid Id { get; private set; }
    public string Login { get; private set; } = null!;
    public string SenhaHash { get; private set; } = null!;
    public PerfilUsuario Perfil { get; private set; }
    public DateTime CriadoEm { get; private set; }
    public bool AcessoObreiros { get; private set; }
    public bool AcessoAcaoSocial { get; private set; }

    public static Usuario Criar(string login, string senhaHash, PerfilUsuario perfil, params Modulo[] modulos)
    {
        return new Usuario
        {
            Id = Guid.NewGuid(),
            Login = login,
            SenhaHash = senhaHash,
            Perfil = perfil,
            CriadoEm = DateTime.UtcNow,
            AcessoObreiros = modulos.Contains(Modulo.Obreiros),
            AcessoAcaoSocial = modulos.Contains(Modulo.AcaoSocial)
        };
    }

    public IReadOnlyList<Modulo> Modulos()
    {
        var modulos = new List<Modulo>();

        if (AcessoObreiros)
            modulos.Add(Modulo.Obreiros);

        if (AcessoAcaoSocial)
            modulos.Add(Modulo.AcaoSocial);

        return modulos;
    }
}
