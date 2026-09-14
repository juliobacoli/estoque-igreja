using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Itens.Commands.CriarItem;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.UnitTests.Helpers;
using FluentValidation.TestHelper;

namespace EstoqueIgreja.UnitTests.Application.Itens;

public class CriarItemCommandValidatorTests
{
    private readonly CriarItemCommandValidator _validator = new();

    [Fact]
    public void NomeVazio_RetornaErro()
    {
        var resultado = _validator.TestValidate(new CriarItemCommand("", "rolo"));

        resultado.ShouldHaveValidationErrorFor(x => x.Nome).WithErrorMessage("Informe o nome do item.");
    }

    [Fact]
    public void NomeAcimaDe120Caracteres_RetornaErro()
    {
        var resultado = _validator.TestValidate(new CriarItemCommand(new string('a', 121), "rolo"));

        resultado.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void NomeCom120Caracteres_EhValido()
    {
        var resultado = _validator.TestValidate(new CriarItemCommand(new string('a', 120), "rolo"));

        resultado.ShouldNotHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void UnidadeVazia_RetornaErro()
    {
        var resultado = _validator.TestValidate(new CriarItemCommand("Sabão", ""));

        resultado.ShouldHaveValidationErrorFor(x => x.Unidade).WithErrorMessage("Informe a unidade.");
    }

    [Fact]
    public void UnidadeAcimaDe40Caracteres_RetornaErro()
    {
        var resultado = _validator.TestValidate(new CriarItemCommand("Sabão", new string('a', 41)));

        resultado.ShouldHaveValidationErrorFor(x => x.Unidade);
    }

    [Fact]
    public void UnidadeCom40Caracteres_EhValida()
    {
        var resultado = _validator.TestValidate(new CriarItemCommand("Sabão", new string('a', 40)));

        resultado.ShouldNotHaveValidationErrorFor(x => x.Unidade);
    }
}

public class CriarItemCommandHandlerTests
{
    private readonly AppDbContext _db = BancoEmMemoria.Criar();

    private CriarItemCommandHandler CriarHandler() => new(_db);

    [Fact]
    public async Task NomeNovo_CriaItemComEstoqueZerado()
    {
        var resultado = await CriarHandler().Handle(new CriarItemCommand("Sabão", "unidade"), CancellationToken.None);

        var salvo = Assert.Single(_db.Itens);
        Assert.Equal(salvo.Id, resultado.Id);
        Assert.Equal("Sabão", resultado.Nome);
        Assert.Equal("unidade", resultado.Unidade);
        Assert.Equal(0, resultado.EstoqueAtual);
    }

    [Theory]
    [InlineData("Sabão")]
    [InlineData("SABAO")]
    [InlineData("  sabão ")]
    public async Task NomeDeItemAtivo_LancaConflito(string nome)
    {
        _db.Itens.Add(Item.Criar("Sabão", "unidade"));
        await _db.SaveChangesAsync();

        var excecao = await Assert.ThrowsAsync<ConflitoException>(
            () => CriarHandler().Handle(new CriarItemCommand(nome, "unidade"), CancellationToken.None));

        Assert.Equal("Já existe um item com esse nome", excecao.Message);
    }

    [Fact]
    public async Task NomeDeItemInativo_ReativaMesmoRegistro()
    {
        var inativo = Item.Criar("Sabão", "pacote");
        inativo.AtualizarQuantidade(8);
        inativo.Inativar();
        _db.Itens.Add(inativo);
        await _db.SaveChangesAsync();

        var resultado = await CriarHandler().Handle(new CriarItemCommand("Sabão", "unidade"), CancellationToken.None);

        var salvo = Assert.Single(_db.Itens);
        Assert.Equal(inativo.Id, resultado.Id);
        Assert.True(salvo.Ativo);
        Assert.Equal(0, resultado.EstoqueAtual);
        Assert.Equal("unidade", resultado.Unidade);
    }
}
