using FluentValidation;

namespace EstoqueIgreja.Application.Itens.Commands.CriarItem;

public class CriarItemCommandValidator : AbstractValidator<CriarItemCommand>
{
    public CriarItemCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Informe o nome do item.")
            .MaximumLength(120);

        RuleFor(x => x.Unidade)
            .NotEmpty().WithMessage("Informe a unidade.")
            .MaximumLength(40);
    }
}
