using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;

public class ExportarEstoquePdfQueryHandler : IRequestHandler<ExportarEstoquePdfQuery, byte[]>
{
    private readonly IAppDbContext _db;
    private readonly IGeradorPdf _geradorPdf;

    public ExportarEstoquePdfQueryHandler(IAppDbContext db, IGeradorPdf geradorPdf)
    {
        _db = db;
        _geradorPdf = geradorPdf;
    }

    public async Task<byte[]> Handle(ExportarEstoquePdfQuery request, CancellationToken ct)
    {
        var itens = await _db.Itens
            .AsNoTracking()
            .OrderBy(i => i.Nome)
            .Select(i => new ItemDoRelatorio(i.Nome, i.Unidade, i.EstoqueAtual))
            .ToListAsync(ct);

        return _geradorPdf.GerarRelatorioEstoque(itens, DateTime.UtcNow);
    }
}
