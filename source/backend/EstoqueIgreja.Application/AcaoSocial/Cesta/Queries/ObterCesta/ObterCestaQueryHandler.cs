using EstoqueIgreja.Application.AcaoSocial.Cesta.Common;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.AcaoSocial.Cesta.Queries.ObterCesta;

public class ObterCestaQueryHandler : IRequestHandler<ObterCestaQuery, CestaResumo>
{
    private readonly IAppDbContext _db;

    public ObterCestaQueryHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<CestaResumo> Handle(ObterCestaQuery request, CancellationToken ct)
    {
        var cestasProntas = await _db.ModelosCesta
            .AsNoTracking()
            .Where(m => m.Id == ModeloCesta.IdUnico)
            .Select(m => m.CestasProntas)
            .FirstOrDefaultAsync(ct);

        var itens = await _db.ModeloCestaItens
            .AsNoTracking()
            .Where(i => i.ModeloCestaId == ModeloCesta.IdUnico)
            .OrderBy(i => i.ItemSocial.NomeNormalizado)
            .Select(i => new ItemDaCesta(
                i.ItemSocialId, i.ItemSocial.Nome, i.ItemSocial.Unidade, i.Quantidade, i.ItemSocial.EstoqueAtual))
            .ToListAsync(ct);

        var paraCalculo = itens
            .Select(i => new ItemParaCalculo(i.Nome, i.Unidade, i.QuantidadePorCesta, i.EstoqueAtual))
            .ToList();

        var podeMontar = CapacidadeDaCesta.PodeMontar(paraCalculo);

        return new CestaResumo(
            cestasProntas, podeMontar, itens, CapacidadeDaCesta.Faltas(paraCalculo, podeMontar + 1));
    }
}
