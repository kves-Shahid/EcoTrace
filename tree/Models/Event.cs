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
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string EventType { get; set; } = "Tree Planting";

        public string? LocationName { get; set; }
        public string? MapUrl { get; set; }

        public string? MediaFilePath { get; set; }
        public string? VideoUrl { get; set; }

        [Required]
        public int MaxVolunteers { get; set; }

        [Required]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        public virtual IdentityUser? Creator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Ensure this is virtual for lazy loading or included in queries
        public virtual ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
    }
}