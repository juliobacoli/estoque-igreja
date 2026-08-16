using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Itens.Commands.RemoverItem;

public class RemoverItemCommandHandler : IRequestHandler<RemoverItemCommand, ItemRemovidoResult>
{
    private readonly IAppDbContext _db;

    public RemoverItemCommandHandler(IAppDbContext db)
    {
        _db = db;
    }

    public async Task<ItemRemovidoResult> Handle(RemoverItemCommand request, CancellationToken ct)
    {
        var item = await _db.Itens
            .FirstOrDefaultAsync(i => i.Id == request.Id && i.Ativo, ct);

        if (item is null)
        {
            throw new NaoEncontradoException("Item não encontrado");
        }

        var jaFoiContado = await _db.AtualizacoesEstoque
            .AnyAsync(a => a.ItemId == item.Id, ct);

        // Item nunca contado foi engano de cadastro: sai do banco. Item com
        // histórico só fica inativo, senão os registros antigos perderiam o nome
        // do produto — e a FK com Restrict recusaria a exclusão de qualquer forma.
        if (jaFoiContado)
        {
            item.Inativar();
        }
        else
        {
            _db.Itens.Remove(item);
        }

        await _db.SaveChangesAsync(ct);

        return new ItemRemovidoResult(!jaFoiContado);
    }
}
