using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Exceptions;

namespace Tywynh.Domain.Entities
{
    [Table("daily_echoes")]
    public class DailyEcho
    {
        [Key]
        [Column(TypeName = "date")]
        public DateTime EchoDate { get; private set; }

        [Required]
        public Guid ConfessionId { get; private set; }

        [ForeignKey(nameof(ConfessionId))]
        public Confession Confession { get; private set; } = default!;

        [Required]
        public int EchoCount { get; private set; } = 0;

        [Required]
        public DateTime CreatedAt { get; private set; }

        // Required by EF
        private DailyEcho() { }

        public static DailyEcho Create(DateTime echoDate, Guid confessionId)
        {
            if (echoDate == default)
                throw new DomainException("Echo date is required.");

            if (confessionId == Guid.Empty)
                throw new DomainException("Confession ID is required.");

            return new DailyEcho
            {
                EchoDate = echoDate.Date,
                ConfessionId = confessionId,
                EchoCount = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void AddEcho()
        {
            EchoCount++;
        }
    }
}
