using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tywynh.Domain.Entities;
using Tywynh.Domain.Enums;

namespace Tywynh.Infrastructure.Persistence.Configuration
{
    internal class ConfessionConfiguration : IEntityTypeConfiguration<Confession>
    {
        public void Configure(EntityTypeBuilder<Confession> builder)
        {
            builder.ToTable("confessions");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();


            builder.Property(c => c.Text)
                .HasColumnName("text")
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(c => c.Category)
                .HasColumnName("category")
                .IsRequired()
                .HasConversion(
                    v => v.ToString().ToLower(),
                    v => Enum.Parse<ConfessionCategory>(v, true));

            builder.Property(c => c.Intensity)
                .HasColumnName("intensity")
                .IsRequired();

            builder.Property(c => c.Alias)
                .HasColumnName("alias")
                .HasMaxLength(100);

            builder.Property(c => c.AuthorId)
                .HasColumnName("author_id");

            builder.Property(c => c.Approved)
                .HasColumnName("approved")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.ResonanceCount)
                .HasColumnName("resonance_count")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.EchoCount)
                .HasColumnName("echo_count")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(c => c.Burned)
                .HasColumnName("burned")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Moderation properties
            builder.Property(c => c.ModerationStatus)
                .HasColumnName("moderation_status")
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pending");

            builder.Property(c => c.RejectionReason)
                .HasColumnName("rejection_reason")
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(c => c.ModeratedAt)
                .HasColumnName("moderated_at")
                .IsRequired(false)
                .HasColumnType("timestamptz");

            builder.Property(c => c.ApprovedAt)
                .HasColumnName("approved_at")
                .IsRequired(false);
            // Indexes
            builder.HasIndex(c => c.ModerationStatus).HasDatabaseName("ix_confessions_moderation_status");
            builder.HasIndex(c => c.Category).HasDatabaseName("ix_confessions_category");
            builder.HasIndex(c => c.CreatedAt).HasDatabaseName("ix_confessions_created_at");
            builder.HasIndex(c => c.ResonanceCount).HasDatabaseName("ix_confessions_resonance_count");
        }
    }
}
