using FluentValidation;

namespace EstoqueIgreja.Application.Historico.Queries.ListarHistorico;

public class ListarHistoricoQueryValidator : AbstractValidator<ListarHistoricoQuery>
{
    public ListarHistoricoQueryValidator()
    {
        RuleFor(x => x.Pagina).GreaterThanOrEqualTo(1);

        RuleFor(x => x.TamanhoPagina).InclusiveBetween(1, 100);
    }
}
