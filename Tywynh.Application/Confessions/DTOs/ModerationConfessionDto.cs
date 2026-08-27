using System;

namespace Tywynh.Application.Confessions.DTOs
{
    public record ModerationConfessionDto(
        Guid Id,
        string Text,
        string Category,
        string ModerationStatus,
        string? RejectionReason,
        DateTime? ModeratedAt,
        DateTime CreatedAt
    );
}
