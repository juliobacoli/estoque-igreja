using EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.DefinirModeloCesta;
using EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.MontarCestas;
using FluentValidation.TestHelper;

namespace EstoqueIgreja.UnitTests.Application.AcaoSocial;

public class CestaValidatorTests
{
    private readonly DefinirModeloCestaCommandValidator _modelo = new();
    private readonly MontarCestasCommandValidator _montar = new();

    [Fact]
    public void Modelo_SemItens_RetornaErro()
    {
        _modelo.TestValidate(new DefinirModeloCestaCommand([]))
            .ShouldHaveValidationErrorFor(x => x.Itens)
            .WithErrorMessage("Escolha pelo menos um item para a cesta.");
    }

    [Fact]
    public void Modelo_QuantidadeZero_RetornaErro()
    {
        var resultado = _modelo.TestValidate(new DefinirModeloCestaCommand([new ItemDoModelo(Guid.NewGuid(), 0)]));

        Assert.Contains(resultado.Errors, e => e.ErrorMessage == "A quantidade de cada item precisa ser maior que zero.");
    }

    [Fact]
    public void Modelo_ItemRepetido_RetornaErro()
    {
        var id = Guid.NewGuid();

        _modelo.TestValidate(new DefinirModeloCestaCommand([new ItemDoModelo(id, 1), new ItemDoModelo(id, 2)]))
            .ShouldHaveValidationErrorFor(x => x.Itens)
            .WithErrorMessage("Um item aparece mais de uma vez na cesta.");
    }

    [Fact]
    public void Modelo_Valido_NaoTemErro()
    {
        _modelo.TestValidate(new DefinirModeloCestaCommand([new ItemDoModelo(Guid.NewGuid(), 2)]))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Montar_QuantidadeNaoPositiva_RetornaErro(int quantidade)
    {
        _montar.TestValidate(new MontarCestasCommand(quantidade))
            .ShouldHaveValidationErrorFor(x => x.Quantidade)
            .WithErrorMessage("Informe quantas cestas vai montar.");
    }
}
