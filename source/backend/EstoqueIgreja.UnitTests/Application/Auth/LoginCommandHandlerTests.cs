using EstoqueIgreja.Application.Auth.Commands.Login;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;
using Moq;

namespace EstoqueIgreja.UnitTests.Application.Auth;

public class LoginCommandHandlerTests
{
    private readonly AppDbContext _db = BancoEmMemoria.Criar();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Usuario _admin = Usuario.Criar("admin", "hash-admin", PerfilUsuario.Admin, Modulo.Obreiros);

    public LoginCommandHandlerTests()
    {
        _db.Usuarios.Add(_admin);
        _db.SaveChanges();
    }

    private LoginCommandHandler CriarHandler() => new(_db, _hasher.Object);

    [Fact]
    public async Task CredenciaisCorretas_RetornaUsuarioPerfilEModulos()
    {
        _hasher.Setup(h => h.Verificar("senha", "hash-admin")).Returns(true);

        var resultado = await CriarHandler().Handle(new LoginCommand("admin", "senha"), CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(_admin.Id, resultado.UsuarioId);
        Assert.Equal("Admin", resultado.Perfil);
        Assert.Equal("admin", resultado.Login);
        Assert.Equal(["Obreiros"], resultado.Modulos);
    }

    [Fact]
    public async Task LoginInexistente_RetornaNull()
    {
        var resultado = await CriarHandler().Handle(new LoginCommand("fulano", "senha"), CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task SenhaErrada_RetornaNull()
    {
        _hasher.Setup(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var resultado = await CriarHandler().Handle(new LoginCommand("admin", "errada"), CancellationToken.None);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task LoginComEspacosEMaiusculas_EncontraUsuario()
    {
        _hasher.Setup(h => h.Verificar("senha", "hash-admin")).Returns(true);

        var resultado = await CriarHandler().Handle(new LoginCommand("  ADMIN ", "senha"), CancellationToken.None);

        Assert.NotNull(resultado);
        Assert.Equal(_admin.Id, resultado.UsuarioId);
    }

    [Fact]
    public async Task LoginInexistente_NaoVerificaSenha()
    {
        await CriarHandler().Handle(new LoginCommand("fulano", "senha"), CancellationToken.None);

        _hasher.Verify(h => h.Verificar(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}
