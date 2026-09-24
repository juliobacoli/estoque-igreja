using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;

namespace EstoqueIgreja.UnitTests.Domain;

public class AtualizacaoEstoqueEUsuarioTests
{
    [Fact]
    public void AtualizacaoEstoque_Criar_PreencheTodosOsCampos()
    {
        var itemId = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        var atualizacao = AtualizacaoEstoque.Criar(itemId, 4, 9, usuarioId);

        Assert.NotEqual(Guid.Empty, atualizacao.Id);
        Assert.Equal(itemId, atualizacao.ItemId);
        Assert.Equal(4, atualizacao.QuantidadeAnterior);
        Assert.Equal(9, atualizacao.QuantidadeNova);
        Assert.Equal(usuarioId, atualizacao.UsuarioId);
        Assert.Equal(DateTimeKind.Utc, atualizacao.Data.Kind);
    }

    [Fact]
    public void AtualizacaoEstoque_Criar_GeraIdsDiferentes()
    {
        var primeira = AtualizacaoEstoque.Criar(Guid.NewGuid(), 0, 1, Guid.NewGuid());
        var segunda = AtualizacaoEstoque.Criar(Guid.NewGuid(), 0, 1, Guid.NewGuid());

        Assert.NotEqual(primeira.Id, segunda.Id);
    }

    [Fact]
    public void Usuario_Criar_PreencheTodosOsCampos()
    {
        var usuario = Usuario.Criar("admin", "hash", PerfilUsuario.Admin, Modulo.Obreiros);

        Assert.NotEqual(Guid.Empty, usuario.Id);
        Assert.Equal("admin", usuario.Login);
        Assert.Equal("hash", usuario.SenhaHash);
        Assert.Equal(PerfilUsuario.Admin, usuario.Perfil);
        Assert.True(usuario.AcessoObreiros);
        Assert.False(usuario.AcessoAcaoSocial);
        Assert.Equal(DateTimeKind.Utc, usuario.CriadoEm.Kind);
    }

    [Fact]
    public void Usuario_Modulos_ListaOsModulosLiberados()
    {
        Assert.Empty(Usuario.Criar("a", "hash", PerfilUsuario.Admin).Modulos());
        Assert.Equal([Modulo.AcaoSocial], Usuario.Criar("b", "hash", PerfilUsuario.Admin, Modulo.AcaoSocial).Modulos());
        Assert.Equal(
            [Modulo.Obreiros, Modulo.AcaoSocial],
            Usuario.Criar("c", "hash", PerfilUsuario.Admin, Modulo.AcaoSocial, Modulo.Obreiros).Modulos());
    }
}
