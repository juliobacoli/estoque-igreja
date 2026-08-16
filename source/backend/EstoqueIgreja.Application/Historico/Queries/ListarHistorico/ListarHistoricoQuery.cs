using MediatR;

namespace EstoqueIgreja.Application.Historico.Queries.ListarHistorico;

public record ListarHistoricoQuery(Guid? ItemId, int Pagina, int TamanhoPagina)
    : IRequest<HistoricoPaginado>;

public record HistoricoPaginado(IReadOnlyList<RegistroHistorico> Registros, bool TemMaisPaginas);

public record RegistroHistorico(
    string ItemNome,
    int QuantidadeAnterior,
    int QuantidadeNova,
    string Perfil,
    DateTime Data);
