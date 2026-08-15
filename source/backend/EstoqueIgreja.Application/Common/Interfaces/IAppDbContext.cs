using EstoqueIgreja.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<Item> Itens { get; }
    DbSet<AtualizacaoEstoque> AtualizacoesEstoque { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
