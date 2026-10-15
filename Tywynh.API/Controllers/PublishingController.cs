using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Tywynh.API.Filters;
using Tywynh.API.Services;
using Tywynh.Domain.Entities;
using Tywynh.Infrastructure.Persistence;
[ApiController, Route("api/moderation/publishing"), AdminKeyAuth, EnableRateLimiting("owner")]
public class PublishingController(AppDbContext db) : ControllerBase
{
    public record ExcerptRequest(string Excerpt);
    public record PostRequest(string PostUrl);
    [HttpGet]
    public async Task<IActionResult> Queue(CancellationToken ct) => Ok(await (from c in db.Confessions.AsNoTracking() join r in db.SubmissionReceipts on c.Id equals r.ConfessionId where c.Approved && !c.Burned && c.ModerationStatus == "approved" && r.ArchiveConsent && r.TikTokConsent orderby c.CreatedAt, c.Id select new { c.Id, c.Text, c.Alias, c.Category, r.ConsentVersion, r.ConsentedAt }).Take(100).ToListAsync(ct));
    [HttpGet("records")]
    public async Task<IActionResult> Records(CancellationToken ct) => Ok(await db.Publications.AsNoTracking().OrderByDescending(p => p.RemovalRequestedAt != null && p.RemovedAt == null).ThenByDescending(p => p.ReviewedAt).ThenBy(p => p.Id).Take(200).ToListAsync(ct));
    [HttpGet("reports")]
    public async Task<IActionResult> Reports(CancellationToken ct) => Ok(await (from report in db.ContentReports.AsNoTracking() join c in db.Confessions.AsNoTracking() on report.ConfessionId equals c.Id where report.ResolvedAt == null orderby report.CreatedAt, report.Id select new { report.Id, report.ConfessionId, report.Reason, report.Details, report.CreatedAt, c.Text, c.Category, c.ModerationStatus }).Take(100).ToListAsync(ct));
    [HttpPost("reports/{id:guid}/resolve")]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken ct)
    {
        var report = await db.ContentReports.FindAsync(new object[] { id }, ct); if (report == null) return NotFound();
        report.ResolvedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); return NoContent();
    }
    private async Task<bool> Eligible(Guid id, CancellationToken ct) => await db.Confessions.AnyAsync(c => c.Id == id && c.Approved && !c.Burned && c.ModerationStatus == "approved", ct) && await db.SubmissionReceipts.AnyAsync(r => r.ConfessionId == id && r.ArchiveConsent && r.TikTokConsent, ct);
    [HttpPost("{id:guid}/review")]
    public Task<IActionResult> Review(Guid id, ExcerptRequest req, CancellationToken ct) => db.ExecuteSerializedAsync<IActionResult>("confession:" + id, async () =>
    {
        if (!await Eligible(id, ct)) return NotFound();
        var record = new PublicationRecord { Id = Guid.NewGuid(), ConfessionId = id, Excerpt = ArchiveWorkflowService.Text(req.Excerpt), ReviewedAt = DateTime.UtcNow };
        db.Publications.Add(record); await db.SaveChangesAsync(ct); return Ok(new { id = record.Id });
    }, ct);
    private async Task<IActionResult> WithRecord(Guid id, Func<PublicationRecord, Task<IActionResult>> action, CancellationToken ct)
    {
        var confessionId = await db.Publications.AsNoTracking().Where(p => p.Id == id).Select(p => (Guid?)p.ConfessionId).SingleOrDefaultAsync(ct);
        if (confessionId == null) return NotFound();
        return await db.ExecuteSerializedAsync<IActionResult>("confession:" + confessionId.Value, async () =>
        {
            var record = await db.Publications.SingleAsync(p => p.Id == id, ct);
            return await action(record);
        }, ct);
    }
    [HttpGet("{id:guid}/export")]
    public Task<IActionResult> Export(Guid id, CancellationToken ct) => WithRecord(id, async record =>
    {
        if (record.RemovalRequestedAt != null || record.RemovedAt != null || !await Eligible(record.ConfessionId, ct)) return NotFound();
        return Ok(new { record.Id, record.ConfessionId, record.Excerpt, channel = "TikTok", record.ReviewedAt });
    }, ct);
    [HttpPost("{id:guid}/published")]
    public Task<IActionResult> Published(Guid id, PostRequest req, CancellationToken ct) => WithRecord(id, async record =>
    {
        if (req.PostUrl?.Length > 1000 || !Uri.TryCreate(req.PostUrl, UriKind.Absolute, out var url) || url.Scheme != "https" || !(url.Host == "tiktok.com" || url.Host.EndsWith(".tiktok.com"))) return BadRequest(new { title = "Enter the HTTPS TikTok post URL (under 1000 characters)." });
        if (record.RemovalRequestedAt != null || record.RemovedAt != null || !await Eligible(record.ConfessionId, ct)) return NotFound();
        record.PostUrl = url.ToString(); record.PublishedAt ??= DateTime.UtcNow; await db.SaveChangesAsync(ct); return NoContent();
    }, ct);
    [HttpPost("{id:guid}/removed")]
    public Task<IActionResult> Removed(Guid id, CancellationToken ct) => WithRecord(id, async record =>
    {
        record.RemovedAt ??= DateTime.UtcNow; await db.SaveChangesAsync(ct); return NoContent();
    }, ct);
}
