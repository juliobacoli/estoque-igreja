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
        // Item removido não entra no relatório (Capítulo 4, item 4.3), e a ordenação
        // usa o nome sem acento: por code point, "Álcool" viria depois de "Sabão".
        var itens = await _db.Itens
            .AsNoTracking()
            .Where(i => i.Ativo)
            .OrderBy(i => i.NomeNormalizado)
            .Select(i => new ItemDoRelatorio(i.Nome, i.Unidade, i.EstoqueAtual))
            .ToListAsync(ct);

        return _geradorPdf.GerarRelatorioEstoque(itens, DateTime.UtcNow);
    }
}
