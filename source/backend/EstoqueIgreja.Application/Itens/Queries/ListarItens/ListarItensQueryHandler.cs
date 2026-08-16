using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Itens.Queries.ListarItens;

public class ListarItensQueryHandler : IRequestHandler<ListarItensQuery, IReadOnlyList<ItemListado>>
{
    private readonly IAppDbContext _db;

    public ListarItensQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ItemListado>> Handle(ListarItensQuery request, CancellationToken ct)
    {
        var consulta = _db.Itens.AsNoTracking();

        if (!request.IncluirInativos)
        {
            consulta = consulta.Where(i => i.Ativo);
        }

        return await consulta
            .OrderBy(i => i.Nome)
            .Select(i => new ItemListado(i.Id, i.Nome, i.Unidade, i.EstoqueAtual))
            .ToListAsync(ct);
    }
}
