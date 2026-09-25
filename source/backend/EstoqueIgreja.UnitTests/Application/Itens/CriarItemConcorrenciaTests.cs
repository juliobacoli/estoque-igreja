using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Itens.Commands.CriarItem;
using EstoqueIgreja.Domain.Entities;
using EstoqueIgreja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.UnitTests.Application.Itens;

public class CriarItemConcorrenciaTests
{
    private readonly string _nomeDoBanco = Guid.NewGuid().ToString();

    private DbContextOptions<AppDbContext> Opcoes() =>
        new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(_nomeDoBanco).Options;

    /// <summary>
    /// O provider em memória não tem índice único: este contexto falha no SaveChanges
    /// como o PostgreSQL falharia, e permite gravar o item concorrente antes disso.
    /// </summary>
    private sealed class BancoQueFalhaAoSalvar(DbContextOptions<AppDbContext> opcoes, Action antesDeFalhar)
        : AppDbContext(opcoes)
    {
        private readonly Action _antesDeFalhar = antesDeFalhar;

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _antesDeFalhar();

            throw new DbUpdateException("Violação de índice único simulada");
        }
    }

    private static Task<ItemCriadoResult> Cadastrar(AppDbContext db, string nome = "Sabão") =>
        new CriarItemCommandHandler(db).Handle(new CriarItemCommand(nome, "unidade"), CancellationToken.None);

    [Fact]
    public async Task OutraRequisicaoGravouOMesmoNome_LancaConflito()
    {
        // A requisição concorrente grava "Sabão" enquanto esta tenta salvar.
        using var db = new BancoQueFalhaAoSalvar(Opcoes(), () =>
        {
            using var concorrente = new AppDbContext(Opcoes());

            concorrente.Itens.Add(Item.Criar("Sabão", "unidade"));
            concorrente.SaveChanges();
        });

        var excecao = await Assert.ThrowsAsync<ConflitoException>(() => Cadastrar(db));

        Assert.Equal("Já existe um item com esse nome", excecao.Message);
    }

    [Fact]
    public async Task FalhaSemNomeDuplicado_PropagaErroOriginal()
    {
        using var db = new BancoQueFalhaAoSalvar(Opcoes(), () => { });

        await Assert.ThrowsAsync<DbUpdateException>(() => Cadastrar(db));
    }
}
