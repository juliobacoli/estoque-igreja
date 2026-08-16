using FluentValidation;

namespace EstoqueIgreja.Application.Itens.Commands.RemoverItem;

public class RemoverItemCommandValidator : AbstractValidator<RemoverItemCommand>
{
    public RemoverItemCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
