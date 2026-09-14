using EstoqueIgreja.Application.Auth.Commands.Login;
using FluentValidation.TestHelper;

namespace EstoqueIgreja.UnitTests.Application.Auth;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LoginVazio_RetornaErro(string login)
    {
        var resultado = _validator.TestValidate(new LoginCommand(login, "senha"));

        resultado.ShouldHaveValidationErrorFor(x => x.Login).WithErrorMessage("Informe o login.");
    }

    [Fact]
    public void SenhaVazia_RetornaErro()
    {
        var resultado = _validator.TestValidate(new LoginCommand("admin", ""));

        resultado.ShouldHaveValidationErrorFor(x => x.Senha).WithErrorMessage("Informe a senha.");
    }

    [Fact]
    public void LoginESenhaPreenchidos_EhValido()
    {
        var resultado = _validator.TestValidate(new LoginCommand("admin", "senha"));

        resultado.ShouldNotHaveAnyValidationErrors();
    }
}
