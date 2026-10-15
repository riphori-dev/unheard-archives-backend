using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Enums;
using Tywynh.Domain.Services;
using Tywynh.Infrastructure.Persistence;
namespace Tywynh.API.Services;

public class ArchiveWorkflowService(AppDbContext db, IAliasGenerator aliases, IConfiguration config)
{
    private static Exception InputError(string field, string message) => new FluentValidation.ValidationException(new[] { new FluentValidation.Results.ValidationFailure(field, message) });
    public record Submission(string Text, string Category, bool ArchiveConsent, bool TikTokConsent, short Intensity = 3);
    public static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value))).ToLowerInvariant();
    public static ConfessionCategory Category(string value)
    {
        if (!Enum.TryParse<ConfessionCategory>(value, true, out var category) || !Enum.IsDefined(category) || int.TryParse(value, out _))
            throw InputError("category", "Choose a valid category.");
        return category;
    }
    public static string Text(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > 500) throw InputError("text", "Writing must contain 1–500 characters.");
        return value.Trim();
    }
    public Task<object> Submit(Submission input, string key, CancellationToken ct)
    {
        if (!Guid.TryParseExact(key, "N", out _)) throw InputError("idempotency_key", "A valid idempotency key is required.");
        if (!input.ArchiveConsent) throw InputError("archive_consent", "Archive publication permission is required.");
        var text = Text(input.Text); var category = Category(input.Category);
        if (input.Intensity < 1 || input.Intensity > 5) throw InputError("intensity", "Intensity must be between 1 and 5.");
        var secret = config["Receipts:SigningKey"];
        if (string.IsNullOrWhiteSpace(secret) || Encoding.UTF8.GetByteCount(secret) < 32) throw new InvalidOperationException("Receipt signing is not configured.");
        var receipt = Convert.ToHexString(HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(key))).ToLowerInvariant();
        var keyHash = Hash(key); var requestHash = Hash(JsonSerializer.Serialize(new { text, category, input.Intensity, input.ArchiveConsent, input.TikTokConsent }));
        return db.ExecuteSerializedAsync<object>("submit:" + keyHash, async () =>
        {
            var existing = await db.SubmissionReceipts.SingleOrDefaultAsync(r => r.IdempotencyHash == keyHash, ct);
            Confession confession;
            if (existing != null)
            {
                if (existing.RequestHash != requestHash) throw new Tywynh.Domain.Exceptions.DomainException("This retry key belongs to different writing. Start a new submission.");
                confession = await db.Confessions.SingleAsync(c => c.Id == existing.ConfessionId, ct);
                // Never return a management secret for withdrawn content or after key rotation.
                if (confession.Burned || existing.ReceiptHash != Hash(receipt)) throw new Tywynh.Domain.Exceptions.DomainException("This submission can no longer be retried.");
            }
            else
            {
                confession = Confession.Create(text, category, input.Intensity, null, aliases.Generate().Value);
                db.Confessions.Add(confession);
                db.SubmissionReceipts.Add(new SubmissionReceipt { Id = Guid.NewGuid(), ConfessionId = confession.Id, IdempotencyHash = keyHash, RequestHash = requestHash, ReceiptHash = Hash(receipt), ArchiveConsent = true, TikTokConsent = input.TikTokConsent, ConsentedAt = DateTime.UtcNow });
                await db.SaveChangesAsync(ct);
            }
            return new { id = confession.Id, alias = confession.Alias, status = confession.ModerationStatus, receipt_token = receipt };
        }, ct);
    }
    public async Task<SubmissionReceipt> Verify(Guid id, string token, CancellationToken ct)
    {
        if (token.Length != 64 || !token.All(Uri.IsHexDigit)) throw new KeyNotFoundException();
        var receipt = await db.SubmissionReceipts.SingleOrDefaultAsync(r => r.ConfessionId == id, ct);
        if (receipt == null || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(receipt.ReceiptHash), Encoding.UTF8.GetBytes(Hash(token)))) throw new KeyNotFoundException();
        return receipt;
    }
    public async Task RequestRemoval(Guid id, CancellationToken ct)
    {
        var records = await db.Publications.Where(p => p.ConfessionId == id && p.RemovedAt == null).ToListAsync(ct);
        foreach (var publication in records) publication.RemovalRequestedAt ??= DateTime.UtcNow;
    }
}
