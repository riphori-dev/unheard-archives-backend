using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Tywynh.API.Services;
using Tywynh.Infrastructure.Persistence;
[ApiController, Route("api/contributor/{id:guid}"), EnableRateLimiting("receipt")]
public class ContributorsController(AppDbContext db, ArchiveWorkflowService workflow) : ControllerBase
{
    public record ReceiptRequest(string ReceiptToken);
    public record Replacement(string ReceiptToken, string Text, string Category, bool TikTokConsent);
    [HttpPost("view")]
    public async Task<IActionResult> View(Guid id, ReceiptRequest req, CancellationToken ct)
    {
        var receipt = await workflow.Verify(id, req.ReceiptToken ?? "", ct);
        var confession = await db.Confessions.SingleAsync(c => c.Id == id, ct);
        return Ok(new { id, text = confession.Burned ? null : confession.Text, category = confession.Category, status = confession.ModerationStatus, tiktok_consent = receipt.TikTokConsent });
    }
    [HttpPost("withdraw")]
    public Task<IActionResult> Withdraw(Guid id, ReceiptRequest req, CancellationToken ct) => db.ExecuteSerializedAsync<IActionResult>("confession:" + id, async () =>
    {
        var receipt = await workflow.Verify(id, req.ReceiptToken ?? "", ct);
        var confession = await db.Confessions.SingleAsync(c => c.Id == id, ct);
        confession.SoftDelete(); receipt.TikTokConsent = false;
        await workflow.RequestRemoval(id, ct);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }, ct);
    [HttpPost("replace")]
    public Task<IActionResult> Replace(Guid id, Replacement req, CancellationToken ct) => db.ExecuteSerializedAsync<IActionResult>("confession:" + id, async () =>
    {
        var receipt = await workflow.Verify(id, req.ReceiptToken ?? "", ct);
        var confession = await db.Confessions.SingleAsync(c => c.Id == id, ct);
        confession.Replace(ArchiveWorkflowService.Text(req.Text), ArchiveWorkflowService.Category(req.Category));
        await db.Resonances.Where(r => r.ConfessionId == id).ExecuteDeleteAsync(ct);
        receipt.TikTokConsent = req.TikTokConsent; receipt.ConsentedAt = DateTime.UtcNow;
        await workflow.RequestRemoval(id, ct);
        await db.SaveChangesAsync(ct);
        return Ok(new { status = "pending" });
    }, ct);
}
