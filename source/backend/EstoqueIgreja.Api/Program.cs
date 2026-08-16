using EstoqueIgreja.Api.Middleware;
using EstoqueIgreja.Api.Services;
using EstoqueIgreja.Application.Common.Behaviors;
using EstoqueIgreja.Application.Common.Interfaces;
using EstoqueIgreja.Infrastructure;
using EstoqueIgreja.Infrastructure.Data;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Banco local de desenvolvimento. Em produção a connection string vem sempre da
// variável de ambiente ConnectionStrings__DefaultConnection, e este bloco não roda.
if (builder.Environment.IsDevelopment() &&
    string.IsNullOrWhiteSpace(builder.Configuration.GetConnectionString("DefaultConnection")))
{
    builder.Configuration["ConnectionStrings:DefaultConnection"] =
        "Host=localhost;Port=5432;Database=estoque;Username=postgres;Password=postgres";
}

var assemblyApplication = typeof(ValidationBehavior<,>).Assembly;

builder.Services.AddControllers();

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assemblyApplication));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssembly(assemblyApplication);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUsuarioAtual, UsuarioAtual>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "estoque.auth";
        options.Cookie.HttpOnly = true;
        // Em produção o cookie é sempre Secure. Em Development cai para
        // SameAsRequest, senão o navegador do celular recusa o cookie ao acessar
        // o dev-server por IP de rede via http e o login nunca persiste.
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;

        // Sem isso o cookie auth responde 302 para /Account/Login e /Account/AccessDenied,
        // e o Angular receberia HTML no lugar do erro.
        options.Events.OnRedirectToLogin = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };

        options.Events.OnRedirectToAccessDenied = contexto =>
        {
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });

// Todo endpoint exige autenticação por padrão; quem for público declara [AllowAnonymous].
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

var app = builder.Build();

app.UseMiddleware<TratamentoErroMiddleware>();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// O index.html do Angular é servido em qualquer rota não-API (deep link como /teste),
// e precisa continuar público mesmo com a FallbackPolicy exigindo autenticação.
app.MapFallbackToFile("index.html").AllowAnonymous();

await AplicarMigrationsAsync(app);

app.Run();

// As 2 contas fixas são inseridas direto no banco, com o hash BCrypt já pronto —
// nenhuma senha em texto puro passa por arquivo de configuração ou pelo código.
static async Task AplicarMigrationsAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();
}
