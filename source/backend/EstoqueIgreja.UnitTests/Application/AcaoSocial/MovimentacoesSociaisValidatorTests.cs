using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.AjustarEstoqueSocial;
using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.RegistrarEntrada;
using FluentValidation.TestHelper;

namespace EstoqueIgreja.UnitTests.Application.AcaoSocial;

// Os handlers usam FOR UPDATE (PostgreSQL) e ficam para os testes de integração.
public class MovimentacoesSociaisValidatorTests
{
    private readonly RegistrarEntradaCommandValidator _entrada = new();
    private readonly AjustarEstoqueSocialCommandValidator _ajuste = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Entrada_QuantidadeNaoPositiva_RetornaErro(int quantidade)
    {
        var resultado = _entrada.TestValidate(new RegistrarEntradaCommand(Guid.NewGuid(), quantidade, null));

        resultado.ShouldHaveValidationErrorFor(x => x.Quantidade)
            .WithErrorMessage("A quantidade precisa ser maior que zero.");
    }

    [Fact]
    public void Entrada_SemDoador_EhValida()
    {
        var resultado = _entrada.TestValidate(new RegistrarEntradaCommand(Guid.NewGuid(), 1, null));

        resultado.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Entrada_DoadorLongo_RetornaErro()
    {
        var resultado = _entrada.TestValidate(new RegistrarEntradaCommand(Guid.NewGuid(), 1, new string('a', 121)));

        resultado.ShouldHaveValidationErrorFor(x => x.Doador);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Ajuste_SemMotivo_RetornaErro(string motivo)
    {
        var resultado = _ajuste.TestValidate(new AjustarEstoqueSocialCommand(Guid.NewGuid(), 1, motivo));

        resultado.ShouldHaveValidationErrorFor(x => x.Motivo)
            .WithErrorMessage("Informe o motivo do ajuste.");
    }

    [Fact]
    public void Ajuste_QuantidadeNegativa_RetornaErro()
    {
        var resultado = _ajuste.TestValidate(new AjustarEstoqueSocialCommand(Guid.NewGuid(), -1, "Perda"));

        resultado.ShouldHaveValidationErrorFor(x => x.NovaQuantidade)
            .WithErrorMessage("A quantidade não pode ser negativa.");
    }

    [Fact]
    public void Ajuste_ParaZeroComMotivo_EhValido()
    {
        var resultado = _ajuste.TestValidate(new AjustarEstoqueSocialCommand(Guid.NewGuid(), 0, "Acabou"));

        resultado.ShouldNotHaveAnyValidationErrors();
    }
}
