using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Tywynh.Domain.Entities;
using Tywynh.Infrastructure.Persistence;
[ApiController, Route("api/reports")]
public class ReportsController(AppDbContext db) : ControllerBase
{
    public record ReportRequest(Guid ConfessionId, string Reason, string? Details);
    [HttpPost, EnableRateLimiting("report")]
    public async Task<IActionResult> Post(ReportRequest req, CancellationToken ct)
    {
        if (!new[] { "identifying details", "harassment", "threats", "other" }.Contains(req.Reason) || (req.Details?.Length ?? 0) > 500) return BadRequest(new { title = "Choose a report reason and keep details under 500 characters." });
        if (!await db.Confessions.AnyAsync(c => c.Id == req.ConfessionId && c.Approved && !c.Burned && c.ModerationStatus == "approved", ct)) return NotFound();
        db.ContentReports.Add(new ContentReport { Id = Guid.NewGuid(), ConfessionId = req.ConfessionId, Reason = req.Reason, Details = req.Details?.Trim(), CreatedAt = DateTime.UtcNow });
        await db.SaveChangesAsync(ct);
        return Accepted(new { status = "received" });
    }
}
