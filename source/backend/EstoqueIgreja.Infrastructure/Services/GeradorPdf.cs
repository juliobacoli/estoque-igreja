using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EstoqueIgreja.Infrastructure.Services;

/// <summary>
/// Layout mínimo — a diagramação final do relatório é pendência assumida
/// (Capítulo 2, item 2.8).
/// </summary>
public class GeradorPdf : IGeradorPdf
{
    private static readonly TimeZoneInfo FusoBrasilia =
        TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");

    public byte[] GerarRelatorioEstoque(IReadOnlyList<ItemDoRelatorio> itens, DateTime geradoEmUtc)
    {
        var geradoEmLocal = TimeZoneInfo.ConvertTimeFromUtc(geradoEmUtc, FusoBrasilia);

        var documento = Document.Create(container =>
        {
            container.Page(pagina =>
            {
                pagina.Size(PageSizes.A4);
                pagina.Margin(2, Unit.Centimetre);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(11));

                pagina.Header().Column(coluna =>
                {
                    coluna.Item().Text("Estoque — ICPA").FontSize(18).Bold();
                    coluna.Item().Text($"Gerado em {geradoEmLocal:dd/MM/yyyy HH:mm}").FontSize(9);
                });

                pagina.Content().PaddingVertical(15).Table(tabela =>
                {
                    tabela.ColumnsDefinition(colunas =>
                    {
                        colunas.RelativeColumn(4);
                        colunas.RelativeColumn(2);
                        colunas.RelativeColumn(2);
                    });

                    tabela.Header(cabecalho =>
                    {
                        cabecalho.Cell().Text("Item").Bold();
                        cabecalho.Cell().Text("Unidade").Bold();
                        cabecalho.Cell().AlignRight().Text("Quantidade").Bold();
                    });

                    foreach (var item in itens)
                    {
                        tabela.Cell().PaddingVertical(3).Text(item.Nome);
                        tabela.Cell().PaddingVertical(3).Text(item.Unidade);
                        tabela.Cell().PaddingVertical(3).AlignRight().Text(item.EstoqueAtual.ToString());
                    }
                });

                pagina.Footer().AlignCenter().Text(texto =>
                {
                    texto.CurrentPageNumber();
                    texto.Span(" / ");
                    texto.TotalPages();
                });
            });
        });

        return documento.GeneratePdf();
    }
}
