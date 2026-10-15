using System.ComponentModel.DataAnnotations;
namespace Tywynh.Domain.Entities;

public class SubmissionReceipt
{
    public Guid Id { get; set; }
    public Guid ConfessionId { get; set; }
    [MaxLength(64)] public string IdempotencyHash { get; set; } = "";
    [MaxLength(64)] public string RequestHash { get; set; } = "";
    [MaxLength(64)] public string ReceiptHash { get; set; } = "";
    public bool ArchiveConsent { get; set; }
    public bool TikTokConsent { get; set; }
    public DateTime ConsentedAt { get; set; }
    [MaxLength(30)] public string ConsentVersion { get; set; } = "2026-10-14";
}
public class PublicationRecord
{
    public Guid Id { get; set; }
    public Guid ConfessionId { get; set; }
    [MaxLength(500)] public string Excerpt { get; set; } = "";
    public DateTime ReviewedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    [MaxLength(1000)] public string? PostUrl { get; set; }
    public DateTime? RemovalRequestedAt { get; set; }
    public DateTime? RemovedAt { get; set; }
}
public class ContentReport
{
    public Guid Id { get; set; }
    public Guid ConfessionId { get; set; }
    [MaxLength(100)] public string Reason { get; set; } = "";
    [MaxLength(500)] public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
