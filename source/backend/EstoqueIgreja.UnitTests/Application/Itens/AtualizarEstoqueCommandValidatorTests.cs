using EstoqueIgreja.Application.Itens.Commands.AtualizarEstoque;
using FluentValidation.TestHelper;

namespace EstoqueIgreja.UnitTests.Application.Itens;

// O handler usa SQL específico do PostgreSQL (FOR UPDATE) e fica para os testes de integração.
public class AtualizarEstoqueCommandValidatorTests
{
    private readonly AtualizarEstoqueCommandValidator _validator = new();

    [Fact]
    public void ItemIdVazio_RetornaErro()
    {
        var resultado = _validator.TestValidate(new AtualizarEstoqueCommand(Guid.Empty, 1));

        resultado.ShouldHaveValidationErrorFor(x => x.ItemId);
    }

    [Fact]
    public void QuantidadeNegativa_RetornaErro()
    {
        var resultado = _validator.TestValidate(new AtualizarEstoqueCommand(Guid.NewGuid(), -1));

        resultado.ShouldHaveValidationErrorFor(x => x.NovaQuantidade)
            .WithErrorMessage("A quantidade não pode ser negativa.");
    }

    [Fact]
    public void QuantidadeZero_EhValida()
    {
        var resultado = _validator.TestValidate(new AtualizarEstoqueCommand(Guid.NewGuid(), 0));

        resultado.ShouldNotHaveAnyValidationErrors();
    }
}
