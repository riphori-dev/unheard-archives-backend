using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tywynh.Domain.Entities;

namespace Tywynh.Infrastructure.Persistence.Configuration
{
    internal class DailyEchoConfiguration : IEntityTypeConfiguration<DailyEcho>
    {
        public void Configure(EntityTypeBuilder<DailyEcho> builder)
        {
            builder.ToTable("daily_echoes");

            builder.HasKey(d => d.EchoDate);

            builder.Property(d => d.EchoDate)
                .HasColumnName("echo_date")
                .HasColumnType("date");

            builder.Property(d => d.ConfessionId)
                .HasColumnName("confession_id")
                .IsRequired();

            builder.Property(d => d.EchoCount)
                .HasColumnName("echo_count")
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(d => d.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure relationship
            builder.HasOne(d => d.Confession)
                .WithMany()
                .HasForeignKey(d => d.ConfessionId);

            // Ensure unique date for each daily echo
            builder.HasIndex(d => d.EchoDate)
                .IsUnique();
        }
    }
}
