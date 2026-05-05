using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.Entities
{
    [Table("resonances")]
    public class Resonance
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ConfessionId { get; set; }

        [ForeignKey(nameof(ConfessionId))]
        public Confession Confession { get; set; } = default!;

        public Guid? UserId { get; set; }

        [MaxLength(255)]
        public string? AnonFingerprint { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
