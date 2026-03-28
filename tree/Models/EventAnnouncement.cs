using System.ComponentModel.DataAnnotations;

namespace EcoTraceApp.Models
{
    public class EventAnnouncement
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public virtual Event? Event { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}