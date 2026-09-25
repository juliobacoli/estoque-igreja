using EstoqueIgreja.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EstoqueIgreja.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("health")]
public class HealthController(AppDbContext db, ILogger<HealthController> logger) : ControllerBase
{
    private readonly AppDbContext _db = db;
    private readonly ILogger<HealthController> _logger = logger;

    [HttpGet]
    public IActionResult Get() => Ok(new { status = "healthy", timestamp = DateTime.UtcNow });

    [HttpGet("db")]
    public async Task<IActionResult> GetDb()
    {
        try
        {
            bool conectou = await _db.Database.CanConnectAsync();

            if (conectou)
            {
                return Ok(new
                {
                    status = "healthy",
                    database = "connected",
                    timestamp = DateTime.UtcNow
                });
            }

            return StatusCode(503, new
            {
                status = "unhealthy",
                database = "disconnected",
                error = "não foi possível conectar ao banco"
            });
        }
        catch (Exception ex)
        {
            // Detalhe do erro vai só para o log — a resposta pública não expõe
            // host, usuário ou qualquer parte da connection string.
            _logger.LogError(ex, "Falha ao conectar no banco durante o health check.");

            return StatusCode(503, new
            {
                status = "unhealthy",
                database = "disconnected",
                error = "não foi possível conectar ao banco"
            });
        }
    }
}
