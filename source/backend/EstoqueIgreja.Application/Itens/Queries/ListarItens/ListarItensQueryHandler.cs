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
        return await _db.Itens
            .AsNoTracking()
            // Mesma ordenação do relatório: pelo nome sem acento, senão itens
            // como "Álcool em gel" caem no fim da lista.
            .OrderBy(i => i.NomeNormalizado)
            .Select(i => new ItemListado(i.Id, i.Nome, i.Unidade, i.EstoqueAtual))
            .ToListAsync(ct);
    }
}
