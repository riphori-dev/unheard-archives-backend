using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.Entities
{
    [Table("burn_events")]
    public class BurnEvent
    {
        [Key]
        public Guid Id { get; set; }

        public Guid? UserId { get; set; }

        [MaxLength(255)]
        public string? AnonFingerprint { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int CharCount { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
