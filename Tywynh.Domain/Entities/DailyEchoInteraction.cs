using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.Entities
{
    [Table("daily_echo_interactions")]
    public class DailyEchoInteraction
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public DateTime EchoDate { get; set; }

        [ForeignKey(nameof(EchoDate))]
        public DailyEcho DailyEcho { get; set; } = default!;

        public Guid? UserId { get; set; }

        [MaxLength(255)]
        public string? AnonFingerprint { get; set; }

        [Required]
        public bool RitualCompleted { get; set; } = false;

        [Required]
        public bool Echoed { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
