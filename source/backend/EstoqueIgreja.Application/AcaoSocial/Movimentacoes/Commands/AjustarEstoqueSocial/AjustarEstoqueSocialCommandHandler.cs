using EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Common;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using MediatR;

namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Commands.AjustarEstoqueSocial;

public class AjustarEstoqueSocialCommandHandler
    : IRequestHandler<AjustarEstoqueSocialCommand, EstoqueSocialAlteradoResult>
{
    private readonly IAppDbContext _db;
    private readonly IUsuarioAtual _usuarioAtual;

    public AjustarEstoqueSocialCommandHandler(IAppDbContext db, IUsuarioAtual usuarioAtual)
    {
        _db = db;
        _usuarioAtual = usuarioAtual;
    }

    public async Task<EstoqueSocialAlteradoResult> Handle(AjustarEstoqueSocialCommand request, CancellationToken ct)
    {
        using var transaction = await _db.BeginTransactionAsync(ct);

        var item = await ItemSocialTravado.BuscarAsync(_db, request.ItemId, ct);
        var quantidadeAnterior = item.EstoqueAtual;

        item.Ajustar(request.NovaQuantidade);

        _db.MovimentacoesSociais.Add(MovimentacaoSocial.Ajuste(
            item.Id, quantidadeAnterior, item.EstoqueAtual, request.Motivo, _usuarioAtual.Id));

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new EstoqueSocialAlteradoResult(item.Id, quantidadeAnterior, item.EstoqueAtual);
    }
}
