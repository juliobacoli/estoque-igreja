using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Infrastructure.Data;
using EstoqueIgreja.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QuestPDF.Infrastructure;

namespace EstoqueIgreja.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
                services.AddDbContextPool<AppDbContext>(options =>                                                                                                                                                        
    {                                                                                                                                                                                                         
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));                                                                                                                                                                                                                                                                                          
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);                                                                                                                                   
    });

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IGeradorPdf, GeradorPdf>();

        QuestPDF.Settings.License = LicenseType.Community;

        return services;
    }
}
