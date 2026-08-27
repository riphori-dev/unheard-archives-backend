using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tywynh.Domain.Exceptions;

namespace Tywynh.Domain.Entities
{
    [Table("resonances")]
    public class Resonance
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public Guid ConfessionId { get; private set; }

        [ForeignKey(nameof(ConfessionId))]
        public Confession Confession { get; private set; } = default!;

        public Guid? UserId { get; private set; }

        [MaxLength(255)]
        public string? VisitorTokenHash { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; }

        // Required by EF
        private Resonance() { }

        public static Resonance Create(
            Guid confessionId,
            Guid? userId,
            string? visitorTokenHash)
        {
            if (string.IsNullOrWhiteSpace(visitorTokenHash))
            {
                throw new DomainException(
                    "VisitorTokenHash is required.");
            }

            return new Resonance
            {
                Id = Guid.NewGuid(),
                ConfessionId = confessionId,
                UserId = userId,
                VisitorTokenHash = visitorTokenHash,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}