using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.Entities
{
    [Table("constellation_nodes")]
    public class ConstellationNode
    {
        [Key]
        public Guid ConfessionId { get; set; }

        [ForeignKey(nameof(ConfessionId))]
        public Confession Confession { get; set; } = default!;

        [Required]
        public double X { get; set; }

        [Required]
        public double Y { get; set; }

        [MaxLength(255)]
        public string? ClusterKey { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
