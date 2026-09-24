using EstoqueIgreja.Application.Common.Exceptions;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.AcaoSocial.Movimentacoes.Common;

internal static class ItemSocialTravado
{
    /// <summary>
    /// Mesma estratégia da contagem dos Obreiros: FOR UPDATE enfileira quem mexe no
    /// mesmo item, e o filtro por Ativo fica dentro da query travada para não
    /// movimentar um item removido no meio do caminho. Precisa de transação aberta.
    /// </summary>
    public static async Task<ItemSocial> BuscarAsync(IAppDbContext db, Guid itemId, CancellationToken ct)
    {
        var item = await db.ItensSociais
            .FromSqlInterpolated($"SELECT * FROM \"ItensSociais\" WHERE \"Id\" = {itemId} AND \"Ativo\" FOR UPDATE")
            .FirstOrDefaultAsync(ct);

        return item ?? throw new NaoEncontradoException("Item não encontrado");
    }
}
