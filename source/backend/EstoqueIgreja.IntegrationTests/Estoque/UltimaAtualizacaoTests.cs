using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.Application.Estoque.Queries.ObterUltimaAtualizacao;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.IntegrationTests.Estoque;

[Collection(ApiCollection.Nome)]
public class UltimaAtualizacaoTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    private const string Rota = "/api/estoque/ultima-atualizacao";

    [Fact]
    public async Task SemContagens_RetornaDataNula()
    {
        var resposta = await (await ClienteAdmin()).GetAsync(Rota);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Null((await resposta.Content.ReadFromJsonAsync<UltimaAtualizacaoResult>())!.Data);
    }

    [Fact]
    public async Task ComContagens_RetornaDataDaMaisRecente()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");
        await AtualizarEstoque(admin, item.Id, 1);
        await AtualizarEstoque(admin, item.Id, 2);

        var resultado = await (await ClienteAdmin()).GetFromJsonAsync<UltimaAtualizacaoResult>(Rota);

        var maisRecente = await NoBanco(db => db.AtualizacoesEstoque.MaxAsync(a => a.Data));
        Assert.Equal(maisRecente, resultado!.Data);
    }

    [Fact]
    public async Task SemCookie_Retorna401()
    {
        var resposta = await ClienteAnonimo().GetAsync(Rota);

        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
