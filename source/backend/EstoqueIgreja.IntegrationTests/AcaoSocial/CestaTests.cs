using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.Application.AcaoSocial.Cesta.Commands.MontarCestas;
using EstoqueIgreja.Application.AcaoSocial.Cesta.Common;
using EstoqueIgreja.Application.AcaoSocial.Cesta.Queries.ObterCesta;
using EstoqueIgreja.Application.AcaoSocial.Itens.Commands.CriarItemSocial;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class CestaTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    private const string Rota = "/api/acao-social/cesta";

    private static async Task<Guid> ItemComEstoque(HttpClient cliente, string nome, int estoque, string unidade = "kg")
    {
        var resposta = await cliente.PostAsJsonAsync("/api/acao-social/itens", new { nome, unidade });
        var item = (await resposta.Content.ReadFromJsonAsync<ItemSocialCriadoResult>())!;

        if (estoque > 0)
            await cliente.PostAsJsonAsync($"/api/acao-social/itens/{item.Id}/entradas", new { quantidade = estoque });

        return item.Id;
    }

    private static Task<HttpResponseMessage> DefinirModelo(HttpClient cliente, params (Guid ItemId, int Quantidade)[] itens) =>
        cliente.PutAsJsonAsync($"{Rota}/modelo", new { itens = itens.Select(i => new { itemId = i.ItemId, quantidade = i.Quantidade }) });

    private static Task<HttpResponseMessage> Montar(HttpClient cliente, int quantidade) =>
        cliente.PostAsJsonAsync($"{Rota}/montagens", new { quantidade });

    private static async Task<CestaResumo> Resumo(HttpClient cliente) =>
        (await cliente.GetFromJsonAsync<CestaResumo>(Rota))!;

    [Fact]
    public async Task SemModelo_ResumoVazio()
    {
        var resumo = await Resumo(await ClienteSocial());

        Assert.Equal(0, resumo.CestasProntas);
        Assert.Equal(0, resumo.PodeMontar);
        Assert.Empty(resumo.Itens);
        Assert.Empty(resumo.FaltasParaProxima);
    }

    [Fact]
    public async Task Resumo_CalculaQuantasDaParaMontarEOQueFaltaParaAProxima()
    {
        var social = await ClienteSocial();
        var arroz = await ItemComEstoque(social, "Arroz", 12);
        var oleo = await ItemComEstoque(social, "Óleo", 2, "garrafa");

        Assert.Equal(HttpStatusCode.NoContent, (await DefinirModelo(social, (arroz, 5), (oleo, 1))).StatusCode);

        var resumo = await Resumo(social);
        Assert.Equal(2, resumo.PodeMontar);
        Assert.Equal(["Arroz", "Óleo"], resumo.Itens.Select(i => i.Nome));
        Assert.Equal([new FaltaParaCesta("Arroz", "kg", 12, 15), new FaltaParaCesta("Óleo", "garrafa", 2, 3)], resumo.FaltasParaProxima);
    }

    [Fact]
    public async Task Montar_BaixaOsItensSomaCestasEGravaMovimentacoes()
    {
        var social = await ClienteSocial();
        var arroz = await ItemComEstoque(social, "Arroz", 12);
        var oleo = await ItemComEstoque(social, "Óleo", 5, "garrafa");
        await DefinirModelo(social, (arroz, 5), (oleo, 1));

        var resposta = await Montar(social, 2);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(new CestasMontadasResult(2, 2), await resposta.Content.ReadFromJsonAsync<CestasMontadasResult>());

        var resumo = await Resumo(social);
        Assert.Equal(2, resumo.CestasProntas);
        Assert.Equal([2, 3], resumo.Itens.Select(i => i.EstoqueAtual));

        var montagem = await NoBanco(db => db.MontagensCesta.AsNoTracking().SingleAsync());
        Assert.Equal((2, 0, 2, Social.Id), (montagem.Quantidade, montagem.CestasAnteriores, montagem.CestasNovas, montagem.UsuarioId));

        var movimentacoes = await NoBanco(db => db.MovimentacoesSociais.AsNoTracking()
            .Where(m => m.Tipo == TipoMovimentacaoSocial.Montagem).ToListAsync());
        Assert.Equal(2, movimentacoes.Count);
        Assert.All(movimentacoes, m => Assert.Equal(montagem.Id, m.MontagemCestaId));
        Assert.Contains(movimentacoes, m => m.ItemSocialId == arroz && m.QuantidadeAnterior == 12 && m.QuantidadeNova == 2);
    }

    [Fact]
    public async Task Montar_SemEstoque_Retorna400ComOQueFaltaESemMexerEmNada()
    {
        var social = await ClienteSocial();
        var arroz = await ItemComEstoque(social, "Arroz", 50);
        var oleo = await ItemComEstoque(social, "Óleo", 7, "garrafa");
        await DefinirModelo(social, (arroz, 5), (oleo, 1));

        var resposta = await Montar(social, 10);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal(
            "Não dá para montar 10 cestas. Falta: Óleo (tem 7, precisa de 10). Dá para montar até 7.",
            (await LerJson(resposta)).GetProperty("error").GetString());

        var resumo = await Resumo(social);
        Assert.Equal(0, resumo.CestasProntas);
        Assert.Equal([50, 7], resumo.Itens.Select(i => i.EstoqueAtual));
        Assert.False(await NoBanco(db => db.MontagensCesta.AnyAsync()));
    }

    [Fact]
    public async Task Montar_SemModelo_Retorna400()
    {
        var resposta = await Montar(await ClienteSocial(), 1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Defina o modelo da cesta antes de montar.", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Montar_QuantidadeZero_Retorna400()
    {
        var resposta = await Montar(await ClienteSocial(), 0);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Informe quantas cestas vai montar.", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task MontagensSimultaneas_NaoPassamDoEstoque()
    {
        var social = await ClienteSocial();
        var arroz = await ItemComEstoque(social, "Arroz", 25);
        await DefinirModelo(social, (arroz, 5));

        var respostas = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Montar(social, 1)));

        Assert.Equal(5, respostas.Count(r => r.StatusCode == HttpStatusCode.OK));
        Assert.Equal(5, respostas.Count(r => r.StatusCode == HttpStatusCode.BadRequest));

        var resumo = await Resumo(social);
        Assert.Equal(5, resumo.CestasProntas);
        Assert.Equal(0, resumo.Itens.Single().EstoqueAtual);
    }

    [Fact]
    public async Task RedefinirModelo_SubstituiOsItensENaoMexeNasCestasProntas()
    {
        var social = await ClienteSocial();
        var arroz = await ItemComEstoque(social, "Arroz", 10);
        var feijao = await ItemComEstoque(social, "Feijão", 10);
        await DefinirModelo(social, (arroz, 5));
        await Montar(social, 1);

        await DefinirModelo(social, (arroz, 2), (feijao, 1));

        var resumo = await Resumo(social);
        Assert.Equal(1, resumo.CestasProntas);
        Assert.Equal([("Arroz", 2), ("Feijão", 1)], resumo.Itens.Select(i => (i.Nome, i.QuantidadePorCesta)));
    }

    [Fact]
    public async Task Modelo_ComItemInexistente_Retorna404()
    {
        var resposta = await DefinirModelo(await ClienteSocial(), (Guid.NewGuid(), 1));

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task Modelo_ItemRepetido_Retorna400()
    {
        var social = await ClienteSocial();
        var arroz = await ItemComEstoque(social, "Arroz", 0);

        var resposta = await DefinirModelo(social, (arroz, 1), (arroz, 2));

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Um item aparece mais de uma vez na cesta.", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task RemoverItemQueEstaNaCesta_Retorna400()
    {
        var social = await ClienteSocial();
        var arroz = await ItemComEstoque(social, "Arroz", 0);
        await DefinirModelo(social, (arroz, 1));

        var resposta = await social.DeleteAsync($"/api/acao-social/itens/{arroz}");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Tire o item da cesta antes de remover.", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Theory]
    [InlineData("GET", Rota)]
    [InlineData("PUT", Rota + "/modelo")]
    [InlineData("POST", Rota + "/montagens")]
    public async Task RotasDaCesta_UsuarioSemOModulo_Retornam403(string metodo, string rota)
    {
        var resposta = await (await ClienteAdmin()).SendAsync(new HttpRequestMessage(new HttpMethod(metodo), rota)
        {
            Content = JsonContent.Create(new { quantidade = 1, itens = Array.Empty<object>() })
        });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }
}
