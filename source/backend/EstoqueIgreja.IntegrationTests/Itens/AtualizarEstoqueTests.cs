using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using EstoqueIgreja.Application.Itens.Commands.AtualizarEstoque;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.IntegrationTests.Infra;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace EstoqueIgreja.IntegrationTests;

[Collection(ApiCollection.Nome)]
public class AtualizarEstoqueTests(ApiFactory factory) : TesteDeIntegracao(factory)
{
    [Fact]
    public async Task Admin_AtualizaEstoque_GravaNovaQuantidade()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");

        var resposta = await AtualizarEstoque(admin, item.Id, 7);

        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var resultado = (await resposta.Content.ReadFromJsonAsync<EstoqueAtualizadoResult>())!;
        Assert.Equal(new EstoqueAtualizadoResult(item.Id, 0, 7), resultado);

        var salvo = await NoBanco(db => db.Itens.AsNoTracking().SingleAsync(i => i.Id == item.Id));
        Assert.Equal(7, salvo.EstoqueAtual);
    }

    [Fact]
    public async Task AtualizarEstoque_GravaHistoricoComUsuarioEQuantidadeAnteriorDoBanco()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");

        await AtualizarEstoque(admin, item.Id, 7);
        await AtualizarEstoque(admin, item.Id, 3);

        var registros = await RegistrosDoItem(item.Id);
        Assert.Equal(2, registros.Count);
        Assert.All(registros, r => Assert.Equal(Admin.Id, r.UsuarioId));
        Assert.Contains(registros, r => r.QuantidadeAnterior == 0 && r.QuantidadeNova == 7);
        Assert.Contains(registros, r => r.QuantidadeAnterior == 7 && r.QuantidadeNova == 3);
    }

    [Fact]
    public async Task ItemInexistente_Retorna404SemHistorico()
    {
        var resposta = await AtualizarEstoque(await ClienteAdmin(), Guid.NewGuid(), 1);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Equal("Item não encontrado", (await LerJson(resposta)).GetProperty("error").GetString());
        Assert.Equal(0, await NoBanco(db => db.AtualizacoesEstoque.CountAsync()));
    }

    [Fact]
    public async Task ItemInativo_Retorna404SemHistorico()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");
        await AtualizarEstoque(admin, item.Id, 1);
        await admin.DeleteAsync($"/api/itens/{item.Id}");

        var resposta = await AtualizarEstoque(admin, item.Id, 2);

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Single(await RegistrosDoItem(item.Id));
    }

    /// <summary>
    /// Comportamento atual documentado: o [Range] do request é validado pelo [ApiController]
    /// antes do handler, e a resposta sai no formato ProblemDetails, sem a propriedade "error".
    /// </summary>
    [Fact]
    public async Task QuantidadeNegativa_Retorna400NoFormatoProblemDetails()
    {
        var item = await CriarItem(await ClienteAdmin(), "Sabão");

        var resposta = await AtualizarEstoque(await ClienteAdmin(), item.Id, -1);

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
        Assert.Equal("application/problem+json", resposta.Content.Headers.ContentType?.MediaType);

        var corpo = await LerJson(resposta);
        Assert.True(corpo.TryGetProperty("errors", out _));
        Assert.False(corpo.TryGetProperty("error", out _));
    }

    [Fact]
    public async Task CorpoSemQuantidade_Retorna400()
    {
        var item = await CriarItem(await ClienteAdmin(), "Sabão");

        var resposta = await (await ClienteAdmin()).PutAsJsonAsync($"/api/itens/{item.Id}/estoque", new { });

        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact]
    public async Task IdQueNaoEhGuid_Retorna404()
    {
        var resposta = await (await ClienteAdmin()).PutAsJsonAsync("/api/itens/abc/estoque", new { novaQuantidade = 1 });

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
    }

    [Fact]
    public async Task ItemTravadoPorOutraTransacao_EsperaELeQuantidadeAtualizada()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");

        await using var conexao = await AbrirConexao();
        await using var transacao = await conexao.BeginTransactionAsync();
        await Executar(conexao, "SELECT 1 FROM \"Itens\" WHERE \"Id\" = @id FOR UPDATE", item.Id);

        var atualizacao = AtualizarEstoque(admin, item.Id, 9);
        await AguardarBloqueio();

        await Executar(conexao, "UPDATE \"Itens\" SET \"EstoqueAtual\" = 5 WHERE \"Id\" = @id", item.Id);
        await transacao.CommitAsync();

        var resultado = (await (await atualizacao).Content.ReadFromJsonAsync<EstoqueAtualizadoResult>())!;
        Assert.Equal(5, resultado.QuantidadeAnterior);
        Assert.Equal(9, resultado.QuantidadeNova);
    }

    [Fact]
    public async Task AtualizacoesSimultaneas_FormamHistoricoEncadeado()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");
        var quantidades = Enumerable.Range(1, 10).ToList();

        var respostas = await Task.WhenAll(quantidades.Select(q => AtualizarEstoque(admin, item.Id, q)));

        Assert.All(respostas, r => Assert.Equal(HttpStatusCode.OK, r.StatusCode));

        // Cada registro precisa partir do valor gravado pelo anterior: 0 -> a -> b -> ... -> final.
        var registros = await RegistrosDoItem(item.Id);
        var porAnterior = registros.ToDictionary(r => r.QuantidadeAnterior);
        var atual = 0;

        foreach (var _ in registros)
            atual = porAnterior[atual].QuantidadeNova;

        Assert.Equal(quantidades.Order(), registros.Select(r => r.QuantidadeNova).Order());
        var salvo = await NoBanco(db => db.Itens.AsNoTracking().SingleAsync(i => i.Id == item.Id));
        Assert.Equal(atual, salvo.EstoqueAtual);
    }

    [Fact]
    public async Task ItemRemovidoDuranteAtualizacao_Retorna404SemHistorico()
    {
        var admin = await ClienteAdmin();
        var item = await CriarItem(admin, "Sabão");

        await using var conexao = await AbrirConexao();
        await using var transacao = await conexao.BeginTransactionAsync();
        await Executar(conexao, "UPDATE \"Itens\" SET \"Ativo\" = false WHERE \"Id\" = @id", item.Id);

        var atualizacao = AtualizarEstoque(admin, item.Id, 4);
        await AguardarBloqueio();
        await transacao.CommitAsync();

        var resposta = await atualizacao;

        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);
        Assert.Empty(await RegistrosDoItem(item.Id));
    }

    [Fact]
    public async Task FalhaNoCommit_DesfazEstoqueEHistorico()
    {
        var item = await CriarItem(await ClienteAdmin(), "Sabão");

        await using var comFalha = FactoryCom(servicos =>
            servicos.ConfigureDbContext<AppDbContext>(o => o.AddInterceptors(new FalhaAoConfirmarContagem())));

        var resposta = await AtualizarEstoque(await ClienteAdmin(comFalha), item.Id, 8);

        Assert.Equal(HttpStatusCode.InternalServerError, resposta.StatusCode);
        var salvo = await NoBanco(db => db.Itens.AsNoTracking().SingleAsync(i => i.Id == item.Id));
        Assert.Equal(0, salvo.EstoqueAtual);
        Assert.Empty(await RegistrosDoItem(item.Id));
    }

    private Task<List<AtualizacaoEstoque>> RegistrosDoItem(Guid itemId) =>
        NoBanco(db => db.AtualizacoesEstoque.AsNoTracking().Where(a => a.ItemId == itemId).ToListAsync());

    /// <summary>
    /// Falha só no commit de uma contagem, depois do SaveChanges já ter enviado os comandos.
    /// Commits sem contagem (como os das migrations) passam normalmente.
    /// </summary>
    private sealed class FalhaAoConfirmarContagem : DbTransactionInterceptor
    {
        public override ValueTask<InterceptionResult> TransactionCommittingAsync(
            DbTransaction transaction,
            TransactionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            if (eventData.Context?.ChangeTracker.Entries<AtualizacaoEstoque>().Any() == true)
                throw new InvalidOperationException("Falha simulada no commit.");

            return ValueTask.FromResult(result);
        }
    }
}
