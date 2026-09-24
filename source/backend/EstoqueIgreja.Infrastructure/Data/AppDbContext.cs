using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EstoqueIgreja.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Item> Itens => Set<Item>();
    public DbSet<AtualizacaoEstoque> AtualizacoesEstoque => Set<AtualizacaoEstoque>();
    public DbSet<ItemSocial> ItensSociais => Set<ItemSocial>();
    public DbSet<MovimentacaoSocial> MovimentacoesSociais => Set<MovimentacaoSocial>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        return Database.BeginTransactionAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>(entidade =>
        {
            entidade.HasKey(u => u.Id);
            entidade.Property(u => u.Login).HasMaxLength(60).IsRequired();
            entidade.HasIndex(u => u.Login).IsUnique();
            entidade.Property(u => u.SenhaHash).HasMaxLength(200).IsRequired();
            entidade.Property(u => u.Perfil).HasConversion<string>().HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Item>(entidade =>
        {
            entidade.HasKey(i => i.Id);
            entidade.Property(i => i.Nome).HasMaxLength(120).IsRequired();
            entidade.Property(i => i.NomeNormalizado).HasMaxLength(120).IsRequired();
            entidade.HasIndex(i => i.NomeNormalizado).IsUnique();
            entidade.Property(i => i.Unidade).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<AtualizacaoEstoque>(entidade =>
        {
            entidade.HasKey(a => a.Id);

            entidade.HasOne(a => a.Item)
                .WithMany()
                .HasForeignKey(a => a.ItemId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasIndex(a => new { a.ItemId, a.Data });
            entidade.HasIndex(a => a.Data);
        });

        modelBuilder.Entity<ItemSocial>(entidade =>
        {
            entidade.HasKey(i => i.Id);
            entidade.Property(i => i.Nome).HasMaxLength(120).IsRequired();
            entidade.Property(i => i.NomeNormalizado).HasMaxLength(120).IsRequired();
            entidade.HasIndex(i => i.NomeNormalizado).IsUnique();
            entidade.Property(i => i.Unidade).HasMaxLength(40).IsRequired();
        });

        modelBuilder.Entity<MovimentacaoSocial>(entidade =>
        {
            entidade.HasKey(m => m.Id);
            entidade.Property(m => m.Tipo).HasConversion<string>().HasMaxLength(20).IsRequired();
            entidade.Property(m => m.Doador).HasMaxLength(120);
            entidade.Property(m => m.Motivo).HasMaxLength(200);

            entidade.HasOne(m => m.ItemSocial)
                .WithMany()
                .HasForeignKey(m => m.ItemSocialId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasIndex(m => new { m.ItemSocialId, m.Data });
            entidade.HasIndex(m => m.Data);
        });

        base.OnModelCreating(modelBuilder);
    }
}
