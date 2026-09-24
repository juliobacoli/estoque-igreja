using FluentValidation;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Commands.CriarItemSocial;

public class CriarItemSocialCommandValidator : AbstractValidator<CriarItemSocialCommand>
{
    public CriarItemSocialCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome do item.")
            .MaximumLength(120);

        RuleFor(x => x.Unidade)
            .NotEmpty().WithMessage("Informe a unidade.")
            .MaximumLength(40);
    }
}
