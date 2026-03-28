using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EcoTraceApp.Models
{
    public class Event
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Mission Date")]
        public DateTime EventDate { get; set; }

        [Required]
        public string EventType { get; set; } = "Tree Planting";

        public string? LocationName { get; set; }

    
        public string? MapUrl { get; set; }

   

        [Range(-90, 90)]
        public double? Latitude { get; set; } = 23.8103; 

        [Range(-180, 180)]
        public double? Longitude { get; set; } = 90.4125;

        

        public string? MediaFilePath { get; set; }
        public string? VideoUrl { get; set; }

        [Required]
        [Range(1, 1000)]
        public int MaxVolunteers { get; set; }

        public bool IsCompleted { get; set; } = false;
        public string? ImpactSummary { get; set; }

        [Required]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public virtual IdentityUser? Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

  

        public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<EventGalleryImage> GalleryImages { get; set; } = new List<EventGalleryImage>();

        public virtual ICollection<EventTask> Tasks { get; set; } = new List<EventTask>();

        public virtual ICollection<EventAnnouncement> Announcements { get; set; } = new List<EventAnnouncement>();
    }
}