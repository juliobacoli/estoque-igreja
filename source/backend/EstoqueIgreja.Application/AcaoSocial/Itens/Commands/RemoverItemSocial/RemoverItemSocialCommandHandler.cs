using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.AcaoSocial.Itens.Commands.RemoverItemSocial;

public class RemoverItemSocialCommandHandler(IAppDbContext db)
    : IRequestHandler<RemoverItemSocialCommand, ItemSocialRemovidoResult>
{
    private readonly IAppDbContext _db = db;

    public async Task<ItemSocialRemovidoResult> Handle(RemoverItemSocialCommand request, CancellationToken ct)
    {
        var item = await _db.ItensSociais
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.Ativo, ct);

        if (item is null)
            throw new NaoEncontradoException("Item não encontrado");

        // Sem essa trava, a cesta passaria a pedir um item que sumiu do estoque e
        // nenhuma montagem seria possível até alguém refazer o modelo.
        if (await _db.ModeloCestaItens.AnyAsync(i => i.ItemSocialId == item.Id, ct))
            throw new ConflitoException("Tire o item da cesta antes de remover.");

        var jaFoiMovimentado = await _db.MovimentacoesSociais
            .AnyAsync(m => m.ItemSocialId == item.Id, ct);

        if (jaFoiMovimentado)
            item.Inativar();
        else
            _db.ItensSociais.Remove(item);

        await _db.SaveChangesAsync(ct);

        return new ItemSocialRemovidoResult(!jaFoiMovimentado);
    }
}
