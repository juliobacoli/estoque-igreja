using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.Application.Historico.Queries.ListarHistorico;
using EstoqueIgreja.Application.Itens.Commands.CriarItem;
using EstoqueIgreja.Application.Itens.Queries.ListarItens;
using EstoqueIgreja.IntegrationTests.Infra;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class CadastroItemTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task Admin_CadastraItem_Retorna201EItemApareceNaLista()
    {
        var admin = await ClienteAdmin();

        var item = await CriarItem(admin, "Sabão");

        Assert.Equal(0, item.EstoqueAtual);
        var itens = await admin.GetFromJsonAsync<List<ItemListado>>("/api/itens");
        Assert.Contains(itens!, i => i.Id == item.Id);
    }

    [Theory]
    [InlineData("Sabão")]
    [InlineData("SABAO")]
    [InlineData("  sabão ")]
    public async Task NomeDuplicado_Retorna400(string nome)
    {
        var admin = await ClienteAdmin();
        await CriarItem(admin, "Sabão");

        var resposta = await admin.PostAsJsonAsync("/api/itens", new { nome, unidade = "unidade" });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Já existe um item com esse nome", (await LerJson(resposta)).GetProperty("error").GetString());
    }

    [Fact]
    public async Task RecadastroDeItemInativo_ReativaMesmoItemEMantemHistorico()
    {
        var admin = await ClienteAdmin();
        var original = await CriarItem(admin, "Sabão", "unidade");
        await AtualizarEstoque(admin, original.Id, 5);
        await admin.DeleteAsync($"/api/itens/{original.Id}");

        var resposta = await admin.PostAsJsonAsync("/api/itens", new { nome = "Sabão", unidade = "pacote" });

        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
        var reativado = (await resposta.Content.ReadFromJsonAsync<ItemCriadoResult>())!;
        Assert.Equal(original.Id, reativado.Id);
        Assert.Equal(0, reativado.EstoqueAtual);
        Assert.Equal("pacote", reativado.Unidade);

        var historico = await admin.GetFromJsonAsync<HistoricoPaginado>($"/api/historico?itemId={original.Id}");
        Assert.Single(historico!.Registros);
    }

    [Theory]
    [InlineData(0, 7)]
    [InlineData(121, 7)]
    [InlineData(5, 0)]
    [InlineData(5, 41)]
    public async Task DadosInvalidos_Retorna400NoFormatoDoMiddleware(int tamanhoNome, int tamanhoUnidade)
    {
        var admin = await ClienteAdmin();

        var resposta = await admin.PostAsJsonAsync("/api/itens",
            new { nome = new string('a', tamanhoNome), unidade = new string('u', tamanhoUnidade) });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        var corpo = await LerJson(resposta);
        Assert.False(string.IsNullOrEmpty(corpo.GetProperty("error").GetString()));
        Assert.True(corpo.TryGetProperty("errors", out _));
    }

    /// <summary>
    /// A verificação de duplicata não trava nada, então quem perde a corrida esbarra no
    /// índice único. O handler traduz isso para o mesmo 400 do cadastro duplicado comum.
    /// </summary>
    [Fact]
    public async Task CadastroSimultaneoComMesmoNome_PerdedorRecebe400()
    {
        var admin = await ClienteAdmin();

        await using var conexao = await AbrirConexao();
        await using var transacao = await conexao.BeginTransactionAsync();
        await Executar(conexao,
            "INSERT INTO \"Itens\" (\"Id\", \"Nome\", \"NomeNormalizado\", \"Unidade\", \"EstoqueAtual\", \"CriadoEm\", \"Ativo\") " +
            "VALUES (@id, 'Sabão', 'sabao', 'unidade', 0, now(), true)",
            Guid.NewGuid());

        // A requisição não enxerga a linha ainda não confirmada e trava no índice único.
        var cadastro = admin.PostAsJsonAsync("/api/itens", new { nome = "Sabão", unidade = "unidade" });
        await AguardarBloqueio();
        await transacao.CommitAsync();

        var resposta = await cadastro;

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("Já existe um item com esse nome", (await LerJson(resposta)).GetProperty("error").GetString());
    }
}
