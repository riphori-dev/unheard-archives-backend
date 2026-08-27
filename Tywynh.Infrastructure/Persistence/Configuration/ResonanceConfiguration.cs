using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tywynh.Domain.Entities;

namespace Tywynh.Infrastructure.Persistence.Configuration
{
    internal class ResonanceConfiguration : IEntityTypeConfiguration<Resonance>
    {
        public void Configure(EntityTypeBuilder<Resonance> builder)
        {
            builder.ToTable("resonances");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            builder.Property(r => r.ConfessionId)
                .HasColumnName("confession_id")
                .IsRequired();

            builder.Property(r => r.UserId)
                .HasColumnName("user_id")
                .IsRequired(false);

            builder.Property(r => r.VisitorTokenHash)
                .HasColumnName("visitor_token_hash")
                .HasMaxLength(255);

            builder.Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("NOW()");

            // Configure relationship
            builder.HasOne(r => r.Confession)
                .WithMany()
                .HasForeignKey(r => r.ConfessionId);

            // Unique index to prevent duplicate resonances per visitor per confession
            builder.HasIndex(r => new { r.ConfessionId, r.VisitorTokenHash }).IsUnique().HasDatabaseName("ix_resonances_confession_visitor");
        }
    }
}
