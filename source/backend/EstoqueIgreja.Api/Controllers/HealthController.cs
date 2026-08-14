using EstoqueIgreja.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EstoqueIgreja.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext db, ILogger<HealthController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

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
