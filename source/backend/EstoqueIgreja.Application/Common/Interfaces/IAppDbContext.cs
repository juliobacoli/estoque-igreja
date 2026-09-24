using EstoqueIgreja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EstoqueIgreja.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Item> Itens { get; }
    DbSet<AtualizacaoEstoque> AtualizacoesEstoque { get; }
    DbSet<ItemSocial> ItensSociais { get; }
    DbSet<MovimentacaoSocial> MovimentacoesSociais { get; }
    DbSet<ModeloCesta> ModelosCesta { get; }
    DbSet<ModeloCestaItem> ModeloCestaItens { get; }
    DbSet<MontagemCesta> MontagensCesta { get; }

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
