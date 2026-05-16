using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tywynh.Domain.Entities;

namespace Tywynh.Infrastructure.Persistence.Configuration
{
    internal class DailyEchoInteractionConfiguration : IEntityTypeConfiguration<DailyEchoInteraction>
    {
        public void Configure(EntityTypeBuilder<DailyEchoInteraction> builder)
        {
            builder.ToTable("daily_echo_interactions");

            builder.HasKey(d => d.Id);

            builder.Property(d => d.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(d => d.EchoDate)
                .HasColumnName("echo_date")
                .IsRequired()
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v,
                    v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(d => d.UserId)
                .HasColumnName("user_id");

            builder.Property(d => d.AnonFingerprint)
                .HasColumnName("anon_fingerprint")
                .HasMaxLength(255);

            builder.Property(d => d.RitualCompleted)
                .HasColumnName("ritual_completed")
                .IsRequired();

            builder.Property(d => d.Echoed)
                .HasColumnName("echoed")
                .IsRequired();

            builder.Property(d => d.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasConversion(
                    v => v.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(v, DateTimeKind.Utc) : v,
                    v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc))
                .HasDefaultValueSql("GETUTCDATE()");

            // Ensure one interaction per user per daily echo
            builder.HasIndex(d => new { d.EchoDate, d.AnonFingerprint })
                .IsUnique();
        }
    }
}
