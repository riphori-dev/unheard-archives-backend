using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tywynh.Domain.Entities
{
    [Table("profiles")]
    public class Profile
    {
        [Key]
        public Guid Id { get; set; }

        [MaxLength(255)]
        public string? DefaultAlias { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
