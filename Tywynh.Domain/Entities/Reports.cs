using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Enums;

namespace Tywynh.Domain.Entities
{
    public class Reports
    {
        [Table("reports")]
        public class Report
        {
            [Key]
            public Guid Id { get; set; }

            [Required]
            public Guid ConfessionId { get; set; }

            [ForeignKey(nameof(ConfessionId))]
            public Confession Confession { get; set; } = default!;

            public Guid? ReporterId { get; set; }

            [MaxLength(255)]
            public string? AnonFingerprint { get; set; }

            [Required]
            public ReportReason Reason { get; set; }

            public string? Details { get; set; }

            [Required]
            public ReportStatus Status { get; set; } = ReportStatus.Pending;

            [Required]
            public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

            public DateTime? ResolvedAt { get; set; }

            public Guid? ResolvedBy { get; set; }
        }


    }
}
