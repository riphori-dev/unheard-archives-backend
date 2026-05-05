using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tywynh.Domain.Enums;
using Tywynh.Domain.Exceptions;

namespace Tywynh.Domain.Entities
{
    [Table("confessions")]
    public class Confession
    {
        [Key]
        public Guid Id { get; private set; }

        [Required, MaxLength(500)]
        public string Text { get; private set; } = default!;

        [Required]
        public ConfessionCategory Category { get; private set; }

        [Required, Range(1, 5)]
        public short Intensity { get; private set; }

        [Required, MaxLength(255)]
        public string Alias { get; private set; } = default!;

        public Guid? AuthorId { get; private set; }

        [Required]
        public bool Approved { get; private set; }

        [Required]
        public int ResonanceCount { get; private set; }

        [Required]
        public int EchoCount { get; private set; }

        [Required]
        public bool Burned { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; }

        public DateTime? ApprovedAt { get; private set; }

        // Required by EF
        private Confession() { }

        // =========================
        // FACTORY METHOD
        // =========================
        public static Confession Create(
            string text,
            ConfessionCategory category,
            short intensity,
            Guid? authorId,
            string alias)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new DomainException("Confession text is required.");

            if (string.IsNullOrWhiteSpace(alias))
                throw new DomainException("Alias is required.");

            if (intensity < 1 || intensity > 5)
                throw new DomainException("Intensity must be between 1 and 5.");

            return new Confession
            {
                Id = Guid.NewGuid(),
                Text = text.Trim(),
                Category = category,
                Intensity = intensity,
                AuthorId = authorId,
                Alias = alias.Trim(),
                Approved = false,
                Burned = false,
                ResonanceCount = 0,
                EchoCount = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        // =========================
        // DOMAIN BEHAVIORS
        // =========================

        public void Approve()
        {
            if (Approved)
                return;

            Approved = true;
            ApprovedAt = DateTime.UtcNow;
        }

        public void Burn()
        {
            Burned = true;
        }

        public void AddResonance()
        {
            ResonanceCount++;
        }

        public void RemoveResonance()
        {
            if (ResonanceCount > 0)
                ResonanceCount--;
        }

        public void AddEcho()
        {
            EchoCount++;
        }
    }
}