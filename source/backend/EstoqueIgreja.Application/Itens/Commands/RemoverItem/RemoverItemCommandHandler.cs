using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Itens.Commands.RemoverItem;

public class RemoverItemCommandHandler(IAppDbContext db) : IRequestHandler<RemoverItemCommand, ItemRemovidoResult>
{
    private readonly IAppDbContext _db = db;

    public async Task<ItemRemovidoResult> Handle(RemoverItemCommand request, CancellationToken ct)
    {
        var item = await _db.Itens
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.Ativo, ct);

        if (item is null)
            throw new NaoEncontradoException("Item não encontrado");

        var jaFoiContado = await _db.AtualizacoesEstoque
            .AnyAsync(a => a.ItemId == item.Id, ct);

        if (jaFoiContado)
            item.Inativar();
        else
            _db.Itens.Remove(item);

        await _db.SaveChangesAsync(ct);

        return new ItemRemovidoResult(!jaFoiContado);
    }
}
