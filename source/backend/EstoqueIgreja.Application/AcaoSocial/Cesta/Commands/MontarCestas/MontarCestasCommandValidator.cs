using FluentValidation;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.MontarCestas;

public class MontarCestasCommandValidator : AbstractValidator<MontarCestasCommand>
{
    public MontarCestasCommandValidator()
    {
        RuleFor(x => x.Quantidade)
            .GreaterThan(0).WithMessage("Informe quantas cestas vai montar.");
    }
}
