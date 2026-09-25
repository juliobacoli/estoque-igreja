using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace EstoqueIgreja.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options), IAppDbContext
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Item> Itens => Set<Item>();
    public DbSet<AtualizacaoEstoque> AtualizacoesEstoque => Set<AtualizacaoEstoque>();
    public DbSet<ItemSocial> ItensSociais => Set<ItemSocial>();
    public DbSet<MovimentacaoSocial> MovimentacoesSociais => Set<MovimentacaoSocial>();
    public DbSet<ModeloCesta> ModelosCesta => Set<ModeloCesta>();
    public DbSet<ModeloCestaItem> ModeloCestaItens => Set<ModeloCestaItem>();
    public DbSet<MontagemCesta> MontagensCesta => Set<MontagemCesta>();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        => Database.BeginTransactionAsync(cancellationToken);

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

        modelBuilder.Entity<MovimentacaoSocial>()
            .HasOne<MontagemCesta>()
            .WithMany()
            .HasForeignKey(m => m.MontagemCestaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ModeloCesta>(entidade =>
        {
            entidade.HasKey(m => m.Id);

            entidade.HasMany(m => m.Itens)
                .WithOne()
                .HasForeignKey(i => i.ModeloCestaId)
                .OnDelete(DeleteBehavior.Cascade);

            entidade.Navigation(m => m.Itens).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<ModeloCestaItem>(entidade =>
        {
            entidade.HasKey(i => i.Id);

            // O Id nasce no código (ModeloCestaItem.Criar). Sem isso, o EF vê um item novo
            // com Id preenchido na coleção do modelo e tenta um UPDATE em vez de INSERT.
            entidade.Property(i => i.Id).ValueGeneratedNever();
            entidade.HasIndex(i => new { i.ModeloCestaId, i.ItemSocialId }).IsUnique();

            entidade.HasOne(i => i.ItemSocial)
                .WithMany()
                .HasForeignKey(i => i.ItemSocialId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<MontagemCesta>(entidade =>
        {
            entidade.HasKey(m => m.Id);

            entidade.HasOne(m => m.Usuario)
                .WithMany()
                .HasForeignKey(m => m.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            entidade.HasIndex(m => m.Data);
        });

        base.OnModelCreating(modelBuilder);
    }
}
