using System.Text;
using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using EstoqueIgreja.Infrastructure.Services;
using QuestPDF.Infrastructure;

namespace EstoqueIgreja.UnitTests.Infrastructure;

public class GeradorPdfTests
{
    public GeradorPdfTests()
    {
        // Em produção a licença é definida no AddInfrastructure.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    public void GerarRelatorioEstoque_RetornaPdfValido(int quantidadeDeItens)
    {
        var itens = Enumerable.Range(1, quantidadeDeItens)
            .Select(i => new ItemDoRelatorio($"Item {i}", "unidade", i))
            .ToList();

        var bytes = new GeradorPdf().GerarRelatorioEstoque(itens, DateTime.UtcNow);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }
}
