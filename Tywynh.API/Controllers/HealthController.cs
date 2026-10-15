using Microsoft.AspNetCore.Mvc;
using Tywynh.Infrastructure.Persistence;
[ApiController, Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    public HealthController(AppDbContext context) => _context = context;
    [HttpGet("live")] public IActionResult Live() => Ok(new { status = "ok" });
    [HttpGet]
    public async Task<IActionResult> Ready(CancellationToken ct)
    {
        var ready = await _context.Database.CanConnectAsync(ct);
        return StatusCode(ready ? 200 : 503, new { status = ready ? "ok" : "degraded", database = ready ? "ok" : "degraded" });
    }
}
