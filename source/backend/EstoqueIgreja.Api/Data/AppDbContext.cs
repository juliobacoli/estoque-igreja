using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets das entidades entram no Capítulo 2 (Telas e Modelo de Dados).
}
