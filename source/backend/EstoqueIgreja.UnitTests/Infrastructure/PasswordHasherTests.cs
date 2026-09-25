using EstoqueIgreja.Infrastructure.Services;

namespace EstoqueIgreja.UnitTests.Infrastructure;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Verificar_MesmaSenhaDoHash_RetornaTrue()
    {
        var hash = _hasher.Hash("senha-forte");

        Assert.True(_hasher.Verificar("senha-forte", hash));
    }

    [Fact]
    public void Verificar_SenhaDiferente_RetornaFalse()
    {
        var hash = _hasher.Hash("senha-forte");

        Assert.False(_hasher.Verificar("outra-senha", hash));
    }

    [Fact]
    public void Verificar_HashInvalido_RetornaFalseSemLancar()
        => Assert.False(_hasher.Verificar("senha", "isto-nao-e-um-hash"));

    [Fact]
    public void Hash_MesmaSenha_GeraHashesDiferentes() => Assert.NotEqual(_hasher.Hash("senha"), _hasher.Hash("senha"));
}
