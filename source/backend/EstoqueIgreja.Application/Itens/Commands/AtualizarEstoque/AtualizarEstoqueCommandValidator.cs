using FluentValidation;

namespace EstoqueIgreja.Application.Itens.Commands.AtualizarEstoque;

public class AtualizarEstoqueCommandValidator : AbstractValidator<AtualizarEstoqueCommand>
{
    public AtualizarEstoqueCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();

        RuleFor(x => x.NovaQuantidade)
            .GreaterThanOrEqualTo(0).WithMessage("A quantidade não pode ser negativa.");
    }
}
