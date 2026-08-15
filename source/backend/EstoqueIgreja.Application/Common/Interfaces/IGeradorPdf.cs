using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;

namespace EstoqueIgreja.Application.Common.Interfaces;

public interface IGeradorPdf
{
    byte[] GerarRelatorioEstoque(IReadOnlyList<ItemDoRelatorio> itens, DateTime geradoEmUtc);
}
