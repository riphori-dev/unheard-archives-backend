using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tywynh.Domain.Entities;

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
                .HasMaxLength(2000);

            builder.Property(c => c.Category)
                .HasColumnName("category")
                .IsRequired()
                .HasMaxLength(50);

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
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(c => c.ApprovedAt)
                .HasColumnName("approved_at")
                .IsRequired(false);
        }
    }
}
