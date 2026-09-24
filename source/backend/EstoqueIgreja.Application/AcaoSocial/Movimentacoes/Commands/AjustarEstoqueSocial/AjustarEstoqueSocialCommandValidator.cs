using FluentValidation;

namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.AjustarEstoqueSocial;

public class AjustarEstoqueSocialCommandValidator : AbstractValidator<AjustarEstoqueSocialCommand>
{
    public AjustarEstoqueSocialCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();

        RuleFor(x => x.NovaQuantidade)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade não pode ser negativa.");

        RuleFor(x => x.Motivo)
            .NotEmpty().WithMessage("Informe o motivo do ajuste.")
            .MaximumLength(200).WithMessage("O motivo pode ter até 200 caracteres.");
    }
}
