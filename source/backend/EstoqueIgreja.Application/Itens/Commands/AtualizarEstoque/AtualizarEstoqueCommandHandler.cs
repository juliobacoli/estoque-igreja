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
        var item = await _db.Itens.FirstOrDefaultAsync(i => i.Id == request.ItemId, ct);

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

        return new EstoqueAtualizadoResult(item.Id, quantidadeAnterior, item.EstoqueAtual);
    }
}
