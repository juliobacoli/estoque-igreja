using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Historico.Queries.ListarHistorico;

public class ListarHistoricoQueryHandler : IRequestHandler<ListarHistoricoQuery, HistoricoPaginado>
{
    private readonly IAppDbContext _db;

    public ListarHistoricoQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<HistoricoPaginado> Handle(ListarHistoricoQuery request, CancellationToken ct)
    {
        var consulta = _db.AtualizacoesEstoque.AsNoTracking();

        if (request.ItemId is not null)
        {
            consulta = consulta.Where(a => a.ItemId == request.ItemId);
        }

        // Busca um registro além do tamanho da página: se ele vier, é porque ainda
        // há mais para carregar.
        var registros = await consulta
            .OrderByDescending(a => a.Data)
            .ThenByDescending(a => a.Id)
            .Skip((request.Pagina - 1) * request.TamanhoPagina)
            .Take(request.TamanhoPagina + 1)
            .Select(a => new RegistroHistorico(
                a.Item.Nome,
                a.QuantidadeAnterior,
                a.QuantidadeNova,
                a.Usuario.Perfil.ToString(),
                a.Data))
            .ToListAsync(ct);

        var temMais = registros.Count > request.TamanhoPagina;

        if (temMais)
        {
            registros.RemoveAt(registros.Count - 1);
        }

        return new HistoricoPaginado(registros, temMais);
    }
}
