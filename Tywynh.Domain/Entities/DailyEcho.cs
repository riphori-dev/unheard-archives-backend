using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.Entities
{
    [Table("daily_echoes")]
    public class DailyEcho
    {
        [Key]
        [Column(TypeName = "date")]
        public DateTime EchoDate { get; set; }

        [Required]
        public Guid ConfessionId { get; set; }

        [ForeignKey(nameof(ConfessionId))]
        public Confession Confession { get; set; } = default!;

        [Required]
        public int EchoCount { get; set; } = 0;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
