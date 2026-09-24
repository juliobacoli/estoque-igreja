using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.Application.Historico.Queries.ListarHistorico;
using EstoqueIgreja.IntegrationTests.Infra;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class HistoricoTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task ListaEmOrdemDecrescenteComNomeDoItemEPerfil()
    {
        var admin = await ClienteAdmin();
        var sabao = await CriarItem(admin, "Sabão");
        var esponja = await CriarItem(admin, "Esponja");

        await AtualizarEstoque(admin, sabao.Id, 1);
        await AtualizarEstoque(admin, esponja.Id, 2);
        await AtualizarEstoque(admin, sabao.Id, 3);

        var historico = (await admin.GetFromJsonAsync<HistoricoPaginado>("/api/historico"))!;

        Assert.Equal([3, 2, 1], historico.Registros.Select(r => r.QuantidadeNova));
        Assert.Equal(["Sabão", "Esponja", "Sabão"], historico.Registros.Select(r => r.ItemNome));
        Assert.Equal(["Admin", "Admin", "Admin"], historico.Registros.Select(r => r.Perfil));
    }

    [Fact]
    public async Task FiltroPorItem_IncluiItemInativo()
    {
        var admin = await ClienteAdmin();
        var sabao = await CriarItem(admin, "Sabão");
        var esponja = await CriarItem(admin, "Esponja");
        await AtualizarEstoque(admin, sabao.Id, 1);
        await AtualizarEstoque(admin, esponja.Id, 2);
        await admin.DeleteAsync($"/api/itens/{sabao.Id}");

        var historico = (await admin.GetFromJsonAsync<HistoricoPaginado>($"/api/historico?itemId={sabao.Id}"))!;

        var registro = Assert.Single(historico.Registros);
        Assert.Equal("Sabão", registro.ItemNome);
    }

    [Fact]
    public async Task Paginacao_IndicaSeHaMaisPaginas()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");

        for (var i = 1; i <= 5; i++)
            await AtualizarEstoque(admin, item.Id, i);

        var pagina1 = (await admin.GetFromJsonAsync<HistoricoPaginado>("/api/historico?pagina=1&tamanhoPagina=2"))!;
        var pagina2 = (await admin.GetFromJsonAsync<HistoricoPaginado>("/api/historico?pagina=2&tamanhoPagina=2"))!;
        var pagina3 = (await admin.GetFromJsonAsync<HistoricoPaginado>("/api/historico?pagina=3&tamanhoPagina=2"))!;

        Assert.True(pagina1.TemMaisPaginas);
        Assert.True(pagina2.TemMaisPaginas);
        Assert.False(pagina3.TemMaisPaginas);
        Assert.Equal([5, 4, 3, 2, 1],
            pagina1.Registros.Concat(pagina2.Registros).Concat(pagina3.Registros).Select(r => r.QuantidadeNova));
    }

    [Theory]
    [InlineData("pagina=0&tamanhoPagina=20")]
    [InlineData("pagina=1&tamanhoPagina=101")]
    public async Task ParametrosInvalidos_Retorna400NoFormatoDoMiddleware(string query)
    {
        var resposta = await (await ClienteAdmin()).GetAsync($"/api/historico?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.True((await LerJson(resposta)).TryGetProperty("error", out _));
    }

    [Fact]
    public async Task SemParametros_UsaPaginaDe20()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");

        for (var i = 1; i <= 21; i++)
            await AtualizarEstoque(admin, item.Id, i);

        var historico = (await admin.GetFromJsonAsync<HistoricoPaginado>("/api/historico"))!;

        Assert.Equal(20, historico.Registros.Count);
        Assert.True(historico.TemMaisPaginas);
    }
}
