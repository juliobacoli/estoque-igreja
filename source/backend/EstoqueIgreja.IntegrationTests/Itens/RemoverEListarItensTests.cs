using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.Application.Itens.Commands.RemoverItem;
using EstoqueIgreja.Application.Itens.Queries.ListarItens;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class RemoverItemTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task ItemNuncaContado_ExcluiDoBanco()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");

        var resposta = await admin.DeleteAsync($"/api/itens/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.True((await resposta.Content.ReadFromJsonAsync<ItemRemovidoResult>())!.Removido);
        Assert.False(await NoBanco(db => db.Itens.AnyAsync(i => i.Id == item.Id)));
    }

    [Fact]
    public async Task ItemComHistorico_FicaInativoEForaDaListaPadrao()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");
        await AtualizarEstoque(admin, item.Id, 2);

        var resposta = await admin.DeleteAsync($"/api/itens/{item.Id}");

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        Assert.False((await resposta.Content.ReadFromJsonAsync<ItemRemovidoResult>())!.Removido);

        var ativos = await admin.GetFromJsonAsync<List<ItemListado>>("/api/itens");
        var todos = await admin.GetFromJsonAsync<List<ItemListado>>("/api/itens?incluirInativos=true");
        Assert.DoesNotContain(ativos!, i => i.Id == item.Id);
        Assert.Contains(todos!, i => i.Id == item.Id);
    }

    [Fact]
    public async Task RemoverDuasVezes_Retorna404()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");
        await AtualizarEstoque(admin, item.Id, 2);
        await admin.DeleteAsync($"/api/itens/{item.Id}");

        var resposta = await admin.DeleteAsync($"/api/itens/{item.Id}");

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }
}

[Collection(ApiCollection.Nome)]
public class ListarItensTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task OrdenaPeloNomeSemAcento()
    {
        var admin = await ClienteAdmin();
        await CriarItem(admin, "Sabão");
        await CriarItem(admin, "Álcool em gel");
        await CriarItem(admin, "Esponja");

        var itens = await admin.GetFromJsonAsync<List<ItemListado>>("/api/itens");

        Assert.Equal(["Álcool em gel", "Esponja", "Sabão"], itens!.Select(i => i.Nome));
    }

    [Fact]
    public async Task IncluirInativos_TrazAtivosEInativos()
    {
        var admin = await ClienteAdmin();
        await CriarItem(admin, "Sabão");
        var inativo = await CriarItem(admin, "Vassoura");
        await AtualizarEstoque(admin, inativo.Id, 1);
        await admin.DeleteAsync($"/api/itens/{inativo.Id}");

        var padrao = await admin.GetFromJsonAsync<List<ItemListado>>("/api/itens");
        var todos = await admin.GetFromJsonAsync<List<ItemListado>>("/api/itens?incluirInativos=true");

        Assert.Single(padrao!);
        Assert.Equal(2, todos!.Count);
    }
}
