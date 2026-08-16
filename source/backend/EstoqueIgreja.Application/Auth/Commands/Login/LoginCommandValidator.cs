using FluentValidation;

namespace EstoqueIgreja.Application.Auth.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Login).NotEmpty().WithMessage("Informe o login.");
        RuleFor(x => x.Senha).NotEmpty().WithMessage("Informe a senha.");
    }
}
