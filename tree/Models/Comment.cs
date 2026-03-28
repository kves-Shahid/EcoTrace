using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EcoTraceApp.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Text { get; set; } = string.Empty;
        [Range(1, 5)]
        public int Rating { get; set; } = 5;

        public int EventId { get; set; }
        public virtual Event? Event { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual IdentityUser? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}