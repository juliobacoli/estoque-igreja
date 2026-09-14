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

    public static TheoryData<ItemDoRelatorio[]> Listas => new()
    {
        Array.Empty<ItemDoRelatorio>(),
        new[] { new ItemDoRelatorio("Sabão", "unidade", 3), new ItemDoRelatorio("Esponja", "pacote", 0) }
    };

    [Theory]
    [MemberData(nameof(Listas))]
    public void GerarRelatorioEstoque_RetornaPdfValido(ItemDoRelatorio[] itens)
    {
        var bytes = new GeradorPdf().GerarRelatorioEstoque(itens, DateTime.UtcNow);

        Assert.NotEmpty(bytes);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(bytes, 0, 4));
    }
}
