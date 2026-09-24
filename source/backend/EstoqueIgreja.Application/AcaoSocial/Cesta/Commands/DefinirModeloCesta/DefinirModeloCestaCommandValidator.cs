using FluentValidation;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.DefinirModeloCesta;

public class DefinirModeloCestaCommandValidator : AbstractValidator<DefinirModeloCestaCommand>
{
    public DefinirModeloCestaCommandValidator()
    {
        RuleFor(x => x.Itens)
            .NotEmpty().WithMessage("Escolha pelo menos um item para a cesta.")
            .Must(itens => itens is null || itens.Select(i => i.ItemId).Distinct().Count() == itens.Count)
            .WithMessage("Um item aparece mais de uma vez na cesta.");

        RuleForEach(x => x.Itens).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemId).NotEmpty();
            item.RuleFor(i => i.Quantidade)
                .GreaterThan(0).WithMessage("A quantidade de cada item precisa ser maior que zero.");
        });
    }
}
