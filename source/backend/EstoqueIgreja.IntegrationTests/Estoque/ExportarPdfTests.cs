using System.Net;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Application.Estoque.Queries.ExportarEstoquePdf;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.Extensions.DependencyInjection;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class ExportarPdfTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task ExportarPdf_RetornaArquivoPdf()
    {
        var admin = await ClienteAdmin();
        await CriarItem(admin, "Sabão");

        var resposta = await (await ClienteVoluntario()).GetAsync("/api/estoque/exportar-pdf");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal("application/pdf", resposta.Content.Headers.ContentType?.MediaType);
        Assert.Equal("estoque.pdf", resposta.Content.Headers.ContentDisposition?.FileName);

        var bytes = await resposta.Content.ReadAsByteArrayAsync();
        Assert.Equal("%PDF"u8.ToArray(), bytes[..4]);
    }

    [Fact]
    public async Task ExportarPdf_NaoIncluiItemInativo()
    {
        var gerador = new GeradorQueCaptura();
        await using var comFake = FactoryCom(servicos => servicos.AddSingleton<IGeradorPdf>(gerador));

        var admin = await ClienteAdmin(comFake);
        await CriarItem(admin, "Sabão");
        var inativo = await CriarItem(admin, "Vassoura");
        await AtualizarEstoque(admin, inativo.Id, 1);
        await admin.DeleteAsync($"/api/itens/{inativo.Id}");

        await admin.GetAsync("/api/estoque/exportar-pdf");

        var item = Assert.Single(gerador.Itens);
        Assert.Equal("Sabão", item.Nome);
    }

    [Fact]
    public async Task ExportarPdf_SemCookie_Retorna401()
    {
        var resposta = await ClienteAnonimo().GetAsync("/api/estoque/exportar-pdf");

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    private sealed class GeradorQueCaptura : IGeradorPdf
    {
        public IReadOnlyList<ItemDoRelatorio> Itens { get; private set; } = [];

        public byte[] GerarRelatorioEstoque(IReadOnlyList<ItemDoRelatorio> itens, DateTime geradoEmUtc)
        {
            Itens = itens;
            return "%PDF"u8.ToArray();
        }
    }
}
