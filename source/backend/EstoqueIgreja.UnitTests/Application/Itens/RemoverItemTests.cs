using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Itens.Commands.RemoverItem;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;
using FluentValidation.TestHelper;

namespace EstoqueIgreja.UnitTests.Application.Itens;

public class RemoverItemCommandValidatorTests
{
    [Fact]
    public void IdVazio_RetornaErro()
    {
        var resultado = new RemoverItemCommandValidator().TestValidate(new RemoverItemCommand(Guid.Empty));

        resultado.ShouldHaveValidationErrorFor(x => x.Id);
    }
}

public class RemoverItemCommandHandlerTests
{
    private readonly AppDbContext _db = BancoEmMemoria.Criar();

    private RemoverItemCommandHandler CriarHandler() => new(_db);

    [Fact]
    public async Task ItemNuncaContado_ExcluiDoBanco()
    {
        var item = Item.Criar("Sabão", "unidade");
        _db.Itens.Add(item);
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new RemoverItemCommand(item.Id), CancellationToken.None);

        Assert.True(resultado.Removido);
        Assert.Empty(_db.Itens);
    }

    [Fact]
    public async Task ItemComHistorico_ApenasInativa()
    {
        var item = Item.Criar("Sabão", "unidade");
        var usuario = Usuario.Criar("admin", "hash", PerfilUsuario.Admin);
        _db.Itens.Add(item);
        _db.Usuarios.Add(usuario);
        _db.AtualizacoesEstoque.Add(AtualizacaoEstoque.Criar(item.Id, 0, 5, usuario.Id));
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new RemoverItemCommand(item.Id), CancellationToken.None);

        Assert.False(resultado.Removido);
        var salvo = Assert.Single(_db.Itens);
        Assert.False(salvo.Ativo);
    }

    [Fact]
    public async Task ItemInexistente_LancaNaoEncontrado()
    {
        await Assert.ThrowsAsync<NaoEncontradoException>(
            () => CriarHandler().Handle(new RemoverItemCommand(Guid.NewGuid()), CancellationToken.None));
    }

    [Fact]
    public async Task ItemJaInativo_LancaNaoEncontrado()
    {
        var item = Item.Criar("Sabão", "unidade");
        item.Inativar();
        _db.Itens.Add(item);
        await _db.SaveChangesAsync();

        await Assert.ThrowsAsync<NaoEncontradoException>(
            () => CriarHandler().Handle(new RemoverItemCommand(item.Id), CancellationToken.None));
    }
}
