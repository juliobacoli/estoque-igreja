using FluentValidation;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Commands.RemoverItemSocial;

public class RemoverItemSocialCommandValidator : AbstractValidator<RemoverItemSocialCommand>
{
    public RemoverItemSocialCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
