using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Queries.ListarItensSociais;

public class ListarItensSociaisQueryHandler(IAppDbContext db)
        : IRequestHandler<ListarItensSociaisQuery, IReadOnlyList<ItemSocialListado>>
{
    private readonly IAppDbContext _db = db;

    public async Task<IReadOnlyList<ItemSocialListado>> Handle(ListarItensSociaisQuery request, CancellationToken ct)
    {
        return await _db.ItensSociais
            .AsNoTracking()
            .Where(i => i.Ativo)
            .OrderBy(i => i.NomeNormalizado)
            .Select(i => new ItemSocialListado(i.Id, i.Nome, i.Unidade, i.EstoqueAtual))
            .ToListAsync(ct);
    }
}
