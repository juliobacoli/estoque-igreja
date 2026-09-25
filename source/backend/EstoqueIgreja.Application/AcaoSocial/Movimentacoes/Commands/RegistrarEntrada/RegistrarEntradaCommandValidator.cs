using FluentValidation;

namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.RegistrarEntrada;

public class RegistrarEntradaCommandValidator : AbstractValidator<RegistrarEntradaCommand>
{
    public RegistrarEntradaCommandValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();

        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("A quantidade precisa ser maior que zero.");
    }
}
