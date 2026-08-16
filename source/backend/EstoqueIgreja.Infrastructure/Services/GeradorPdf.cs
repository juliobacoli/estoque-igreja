using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace EstoqueIgreja.Infrastructure.Services;

/// <summary>
/// Relatório de estoque com a identidade visual da ICPA (Capítulo 2, item 2.9).
/// </summary>
public class GeradorPdf : IGeradorPdf
{
    private const string RoxoEscuro = "#2D1B3D";
    private const string Dourado = "#D4A574";
    private const string CinzaClaro = "#F5F5F7";

    // O container roda em UTC. Sem converter, o relatório sairia três horas
    // adiantado para quem o lê no Brasil.
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
                pagina.Margin(0);
                pagina.DefaultTextStyle(estilo => estilo.FontSize(11));

                pagina.Header().Background(RoxoEscuro).Padding(30).Column(coluna =>
                {
                    coluna.Item().Text("Estoque — Igreja ICPA")
                        .FontSize(20).FontColor(Colors.White).Bold();

                    coluna.Item().Text("Relatório de itens em estoque")
                        .FontSize(12).FontColor(Dourado);

                    coluna.Item().PaddingTop(10).Width(48).Height(3).Background(Dourado);
                });

                pagina.Content().Padding(30).Column(coluna =>
                {
                    coluna.Item().Row(linha =>
                    {
                        linha.RelativeItem()
                            .Text($"Gerado em {geradoEmLocal:dd/MM/yyyy 'às' HH:mm}")
                            .FontSize(12);

                        linha.RelativeItem().AlignRight()
                            .Text($"{itens.Count} {(itens.Count == 1 ? "item cadastrado" : "itens cadastrados")}")
                            .FontSize(12);
                    });

                    coluna.Item().PaddingTop(16).Table(tabela =>
                    {
                        tabela.ColumnsDefinition(colunas =>
                        {
                            colunas.RelativeColumn(3);
                            colunas.RelativeColumn(2);
                            colunas.RelativeColumn(1);
                        });

                        tabela.Header(cabecalho =>
                        {
                            cabecalho.Cell().Background(RoxoEscuro).Padding(8)
                                .Text("Item").FontColor(Colors.White).Bold();

                            cabecalho.Cell().Background(RoxoEscuro).Padding(8)
                                .Text("Unidade").FontColor(Colors.White).Bold();

                            cabecalho.Cell().Background(RoxoEscuro).Padding(8).AlignRight()
                                .Text("Qtd. atual").FontColor(Colors.White).Bold();
                        });

                        foreach (var (item, indice) in itens.Select((i, idx) => (i, idx)))
                        {
                            // Tipo explícito: string e Color convertem entre si, e o
                            // compilador não consegue inferir num ternário misto.
                            Color fundo = indice % 2 == 0 ? CinzaClaro : Colors.White;

                            // ShowEntire impede que um nome longo seja partido na quebra
                            // de página, o que deixaria metade do texto órfão na página
                            // seguinte, sem unidade nem quantidade ao lado.
                            tabela.Cell().ShowEntire().Background(fundo).Padding(8).Text(item.Nome);
                            tabela.Cell().ShowEntire().Background(fundo).Padding(8).Text(item.Unidade);
                            tabela.Cell().ShowEntire().Background(fundo).Padding(8).AlignRight()
                                .Text(item.EstoqueAtual.ToString()).Bold();
                        }
                    });
                });

                pagina.Footer().Background(CinzaClaro).Padding(10).Row(linha =>
                {
                    linha.RelativeItem()
                        .Text("Gerado automaticamente pelo sistema de estoque")
                        .FontSize(10).FontColor(Colors.Grey.Medium);

                    linha.RelativeItem().AlignRight().Text(texto =>
                    {
                        texto.DefaultTextStyle(estilo => estilo.FontSize(10).FontColor(Colors.Grey.Medium));
                        texto.Span("Página ");
                        texto.CurrentPageNumber();
                        texto.Span(" de ");
                        texto.TotalPages();
                    });
                });
            });
        });

        return documento.GeneratePdf();
    }
}
