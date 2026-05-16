using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tywynh.Domain.Enums;

namespace Tywynh.Application.Confessions.DTOs
{
    public class ConfessionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public ConfessionCategory Category { get; set; }
        public short Intensity { get; set; }
        public string Alias { get; set; } = string.Empty;
        public Guid? AuthorId { get; set; }
        public bool Approved { get; set; }
        public int ResonanceCount { get; set; }
        public int EchoCount { get; set; }
        public bool Burned { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
