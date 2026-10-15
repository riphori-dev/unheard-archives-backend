using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Tywynh.API.Filters;
using Tywynh.Application.Confessions.Queries.GetModeratedConfessions;
using Tywynh.Application.Confessions.Commands.ApproveConfession;
using Tywynh.Application.Confessions.Commands.RejectConfession;
using Tywynh.Application.Confessions.Commands.SoftDeleteConfession;

[ApiController]
[Route("api/moderation")]
[AdminKeyAuth]
[Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("owner")]
public class ModerationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly Tywynh.Infrastructure.Persistence.AppDbContext _db;
    public ModerationController(IMediator mediator, Tywynh.Infrastructure.Persistence.AppDbContext db) { _mediator = mediator; _db = db; }

    [HttpGet("confessions")]
    public async Task<IActionResult> Get([FromQuery] string status = "pending")
    {
        if (!new[] { "pending", "approved", "rejected", "deleted" }.Contains(status)) return BadRequest(new { title = "Invalid review status." });
        var result = await (from c in _db.Confessions.AsNoTracking()
                            join r in _db.SubmissionReceipts on c.Id equals r.ConfessionId into receipts
                            from r in receipts.DefaultIfEmpty()
                            where c.ModerationStatus == status
                            orderby c.CreatedAt, c.Id
                            select new { c.Id, c.Text, c.Category, moderation_status = c.ModerationStatus, c.CreatedAt, archive_consent = r != null && r.ArchiveConsent, tiktok_consent = r != null && r.TikTokConsent }).Take(100).ToListAsync(HttpContext.RequestAborted);
        return Ok(result);
    }

    [HttpPost("confessions/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        if (!_db.SubmissionReceipts.Any(r => r.ConfessionId == id && r.ArchiveConsent)) return Conflict(new { title = "Archive publication permission is missing." });
        await _mediator.Send(new ApproveConfessionCommand(id));
        return NoContent();
    }

    public record RejectRequest(string? Reason);

    [HttpPost("confessions/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest req)
    {
        if (req.Reason?.Length > 500) return BadRequest(new { title = "Keep the reason under 500 characters." });
        await _mediator.Send(new RejectConfessionCommand(id, req.Reason));
        return NoContent();
    }

    [HttpDelete("confessions/{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        await _mediator.Send(new SoftDeleteConfessionCommand(id));
        return NoContent();
    }
}
