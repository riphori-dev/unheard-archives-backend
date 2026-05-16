using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Tywynh.Domain.Enums;
using Tywynh.Domain.Exceptions;

namespace Tywynh.Domain.Entities
{
    [Table("daily_echo_interactions")]
    public class DailyEchoInteraction
    {
        [Key]
        public Guid Id { get; private set; }

        [Required]
        public DateTime EchoDate { get; private set; }

        public Guid? UserId { get; private set; }

        [MaxLength(255)]
        public string? AnonFingerprint { get; private set; }

        [Required]
        public bool RitualCompleted { get; private set; }

        [Required]
        public bool Echoed { get; private set; }

        [Required]
        public DateTime CreatedAt { get; private set; }

        // Required by EF
        public DailyEchoInteraction() { }

        public static DailyEchoInteraction Create(
            DateTime echoDate,
            Guid? userId,
            string? anonFingerprint,
            bool ritualCompleted = false,
            bool echoed = false)
        {
            return new DailyEchoInteraction
            {
                Id = Guid.NewGuid(),
                EchoDate = echoDate,
                UserId = userId,
                AnonFingerprint = anonFingerprint,
                RitualCompleted = ritualCompleted,
                Echoed = echoed,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkRitualCompleted()
        {
            RitualCompleted = true;
        }

        public void MarkEchoed()
        {
            Echoed = true;
        }
    }
}
