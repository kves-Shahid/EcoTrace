using System.ComponentModel.DataAnnotations;

namespace EcoTraceApp.Models
{
    public class EventGalleryImage
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        public virtual Event? Event { get; set; }

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}