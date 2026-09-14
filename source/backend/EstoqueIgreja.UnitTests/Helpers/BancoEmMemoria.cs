using EstoqueIgreja.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.UnitTests.Helpers;

public static class BancoEmMemoria
{
    /// <summary>
    /// Cada chamada devolve um banco isolado, para um teste não enxergar os dados de outro.
    /// </summary>
    public static AppDbContext Criar()
    {
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(opcoes);
    }
}
