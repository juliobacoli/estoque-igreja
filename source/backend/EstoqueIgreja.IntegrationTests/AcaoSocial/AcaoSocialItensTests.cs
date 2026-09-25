using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.Application.AcaoSocial.Itens.Commands.CriarItemSocial;
using EstoqueIgreja.Application.AcaoSocial.Itens.Queries.ListarItensSociais;
using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Common;
using EstoqueIgreja.Domain.Enums;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.IntegrationTests.AcaoSocial;

[Collection(ApiCollection.Nome)]
public class AcaoSocialItensTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    private const string Rota = "/api/acao-social/itens";

    private static async Task<ItemSocialCriadoResult> CriarItemSocial(HttpClient cliente, string nome, string unidade = "kg")
    {
        var resposta = await cliente.PostAsJsonAsync(Rota, new { nome, unidade });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        return (await resposta.Content.ReadFromJsonAsync<ItemSocialCriadoResult>())!;
    }

    private static Task<HttpResponseMessage> Entrada(HttpClient cliente, Guid itemId, int quantidade) =>
        cliente.PostAsJsonAsync($"{Rota}/{itemId}/entradas", new { quantidade });

    private static Task<HttpResponseMessage> Ajuste(HttpClient cliente, Guid itemId, int novaQuantidade, string motivo) =>
        cliente.PostAsJsonAsync($"{Rota}/{itemId}/ajustes", new { novaQuantidade, motivo });

    [Fact]
    public async Task Cadastro_ItemApareceNaListaComEstoqueZerado()
    {
        var social = await ClienteSocial();

        var item = await CriarItemSocial(social, "Arroz");

        var itens = await social.GetFromJsonAsync<List<ItemSocialListado>>(Rota);
        var listado = Assert.Single(itens!);
        Assert.Equal(new ItemSocialListado(item.Id, "Arroz", "kg", 0), listado);
    }

    [Fact]
    public async Task Cadastro_NomeDuplicadoSemAcento_Retorna400()
    {
        var social = await ClienteSocial();
        await CriarItemSocial(social, "Feijão");

        var resposta = await social.PostAsJsonAsync(Rota, new { nome = "FEIJAO", unidade = "kg" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Já existe um item com esse nome", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Cadastro_NomeIgualAoDeUmItemDosObreiros_EhPermitido()
    {
        await CriarItem(await ClienteAdmin(), "Sabão");

        await CriarItemSocial(await ClienteSocial(), "Sabão", "barra");
    }

    [Fact]
    public async Task Entrada_SomaAoEstoqueEGravaMovimentacaoComUsuario()
    {
        var social = await ClienteSocial();
        var item = await CriarItemSocial(social, "Arroz");

        await Entrada(social, item.Id, 5);
        var resposta = await Entrada(social, item.Id, 3);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var resultado = await resposta.Content.ReadFromJsonAsync<EstoqueSocialAlteradoResult>();
        Assert.Equal(new EstoqueSocialAlteradoResult(item.Id, 5, 8), resultado);

        var movimentacoes = await MovimentacoesDoItem(item.Id);
        Assert.Equal(2, movimentacoes.Count);
        Assert.All(movimentacoes, m => Assert.Equal(TipoMovimentacaoSocial.Entrada, m.Tipo));
        Assert.All(movimentacoes, m => Assert.Equal(Social.Id, m.UsuarioId));
        Assert.Contains(movimentacoes, m => m.QuantidadeAnterior == 5 && m.QuantidadeNova == 8);
        Assert.Contains(movimentacoes, m => m.QuantidadeAnterior == 0 && m.QuantidadeNova == 5);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task Entrada_QuantidadeNaoPositiva_Retorna400SemMovimentacao(int quantidade)
    {
        var social = await ClienteSocial();
        var item = await CriarItemSocial(social, "Arroz");

        var resposta = await Entrada(social, item.Id, quantidade);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("A quantidade precisa ser maior que zero.", (await LerJson(resposta)).GetProperty("error").GetString());
        Assert.Empty(await MovimentacoesDoItem(item.Id));
    }

    [Fact]
    public async Task Ajuste_DefineQuantidadeEGravaMotivo()
    {
        var social = await ClienteSocial();
        var item = await CriarItemSocial(social, "Óleo", "garrafa");
        await Entrada(social, item.Id, 10);

        var resposta = await Ajuste(social, item.Id, 7, "3 garrafas vencidas");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.Equal(new EstoqueSocialAlteradoResult(item.Id, 10, 7),
            await resposta.Content.ReadFromJsonAsync<EstoqueSocialAlteradoResult>());

        var ajuste = Assert.Single(await MovimentacoesDoItem(item.Id), m => m.Tipo == TipoMovimentacaoSocial.Ajuste);
        Assert.Equal("3 garrafas vencidas", ajuste.Motivo);
    }

    [Fact]
    public async Task Ajuste_SemMotivo_Retorna400()
    {
        var social = await ClienteSocial();
        var item = await CriarItemSocial(social, "Arroz");

        var resposta = await Ajuste(social, item.Id, 1, "");

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Informe o motivo do ajuste.", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task Movimentar_ItemInexistente_Retorna404()
    {
        var social = await ClienteSocial();

        var entrada = await Entrada(social, Guid.NewGuid(), 1);
        var ajuste = await Ajuste(social, Guid.NewGuid(), 1, "Teste");

        Assert.Equal(HttpStatusCode.NotFound, entrada.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, ajuste.StatusCode);
    }

    [Fact]
    public async Task EntradasSimultaneas_SomamTodas()
    {
        var social = await ClienteSocial();
        var item = await CriarItemSocial(social, "Arroz");

        var respostas = await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Entrada(social, item.Id, 1)));

        Assert.All(respostas, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));
        var salvo = await NoBanco(db => db.ItensSociais.AsNoTracking().SingleAsync(i => i.Id == item.Id));
        Assert.Equal(10, salvo.EstoqueAtual);
        Assert.Equal(10, (await MovimentacoesDoItem(item.Id)).Count);
    }

    [Fact]
    public async Task Remover_ItemSemMovimentacao_ApagaDoBanco()
    {
        var social = await ClienteSocial();
        var item = await CriarItemSocial(social, "Arroz");

        var resposta = await social.DeleteAsync($"{Rota}/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.True((await LerJson(resposta)).GetProperty("removido").GetBoolean());
        Assert.False(await NoBanco(db => db.ItensSociais.AnyAsync(i => i.Id == item.Id)));
    }

    [Fact]
    public async Task Remover_ItemComMovimentacao_InativaESomeDaLista()
    {
        var social = await ClienteSocial();
        var item = await CriarItemSocial(social, "Arroz");
        await Entrada(social, item.Id, 2);

        var resposta = await social.DeleteAsync($"{Rota}/{item.Id}");

        Assert.False((await LerJson(resposta)).GetProperty("removido").GetBoolean());
        Assert.Empty((await social.GetFromJsonAsync<List<ItemSocialListado>>(Rota))!);
        Assert.Equal(HttpStatusCode.NotFound, (await Entrada(social, item.Id, 1)).StatusCode);
    }

    [Theory]
    [InlineData("GET", Rota)]
    [InlineData("POST", Rota)]
    [InlineData("DELETE", Rota + "/00000000-0000-0000-0000-000000000001")]
    [InlineData("POST", Rota + "/00000000-0000-0000-0000-000000000001/entradas")]
    [InlineData("POST", Rota + "/00000000-0000-0000-0000-000000000001/ajustes")]
    public async Task RotasDaAcaoSocial_UsuarioSemOModulo_Retornam403(string metodo, string rota)
    {
        var obreiros = await ClienteAdmin();

        var resposta = await obreiros.SendAsync(new HttpRequestMessage(new HttpMethod(metodo), rota)
        {
            Content = JsonContent.Create(new { nome = "Arroz", unidade = "kg", quantidade = 1, novaQuantidade = 1, motivo = "x" })
        });

        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    private Task<List<Domain.Entities.MovimentacaoSocial>> MovimentacoesDoItem(Guid itemId) =>
        NoBanco(db => db.MovimentacoesSociais.AsNoTracking().Where(m => m.ItemSocialId == itemId).ToListAsync());
}
