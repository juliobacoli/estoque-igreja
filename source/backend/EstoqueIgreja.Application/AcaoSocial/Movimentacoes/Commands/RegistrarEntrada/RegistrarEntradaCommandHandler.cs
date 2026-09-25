using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Common;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.RegistrarEntrada;

public class RegistrarEntradaCommandHandler
    : IRequestHandler<RegistrarEntradaCommand, EstoqueSocialAlteradoResult>
{
    private readonly IAppDbContext _db;
    private readonly IUsuarioAtual _usuarioAtual;

    public RegistrarEntradaCommandHandler(IAppDbContext db, IUsuarioAtual usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<EstoqueSocialAlteradoResult> Handle(RegistrarEntradaCommand request, CancellationToken ct)
    {
        using var transaction = await _db.BeginTransactionAsync(ct);

        var item = await ItemSocialTravado.BuscarAsync(_db, request.ItemId, ct);
        var quantidadeAnterior = item.EstoqueAtual;

        item.Adicionar(request.Quantidade);

        _db.MovimentacoesSociais.Add(MovimentacaoSocial.Entrada(
            item.Id, quantidadeAnterior, item.EstoqueAtual, _usuarioAtual.Id));

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new EstoqueSocialAlteradoResult(item.Id, quantidadeAnterior, item.EstoqueAtual);
    }
}
