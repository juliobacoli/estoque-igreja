using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Itens.Commands.AtualizarEstoque;

public class AtualizarEstoqueCommandHandler
    : IRequestHandler<AtualizarEstoqueCommand, EstoqueAtualizadoResult>
{
    private readonly IAppDbContext _db;
    private readonly IUsuarioAtual _usuarioAtual;

    public AtualizarEstoqueCommandHandler(IAppDbContext db, IUsuarioAtual usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<EstoqueAtualizadoResult> Handle(AtualizarEstoqueCommand request, CancellationToken ct)
    {
        // Inicia a transação. O banco gerenciará a fila (lock) sem disparar exceptions de concorrência.
        using var transaction = await _db.BeginTransactionAsync(ct);

        // FOR UPDATE trava exclusivamente esta linha para a transação atual no PostgreSQL.
        // O filtro por Ativo fica dentro da query travada de propósito: se o admin
        // remover o item enquanto alguém está com a tela de contagem aberta, o save
        // atrasado não pode gravar movimentação para um item que saiu do catálogo.
        var item = await _db.Itens
            .FromSqlInterpolated($"SELECT * FROM \"Itens\" WHERE \"Id\" = {request.ItemId} AND \"Ativo\" FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        if (item is null)
        {
            throw new NaoEncontradoException("Item não encontrado");
        }

        // A quantidade anterior é lida do banco aqui, e nunca enviada pelo cliente —
        // senão duas contagens simultâneas gravariam histórico com valores errados.
        var quantidadeAnterior = item.EstoqueAtual;

        item.AtualizarQuantidade(request.NovaQuantidade);

        _db.AtualizacoesEstoque.Add(
            AtualizacaoEstoque.Criar(item.Id, quantidadeAnterior, request.NovaQuantidade, _usuarioAtual.Id));

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new EstoqueAtualizadoResult(item.Id, quantidadeAnterior, item.EstoqueAtual);
    }
}
