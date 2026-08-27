using Microsoft.AspNetCore.Mvc;
using Tywynh.Infrastructure.Persistence;

[ApiController, Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    public HealthController(AppDbContext context) { _context = context; }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var dbOk = await _context.Database.CanConnectAsync();
        return Ok(new {
            status = "ok",
            database = dbOk ? "ok" : "degraded",
            timestamp = DateTime.UtcNow
        });
    }
}
