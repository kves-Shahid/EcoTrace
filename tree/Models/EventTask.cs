using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EcoTraceApp.Models
{
    public class EventTask
    {
        [Key]
        public int Id { get; set; }

        public int EventId { get; set; }
        public virtual Event? Event { get; set; }

        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public string? AssignedUserId { get; set; }
        public virtual IdentityUser? AssignedUser { get; set; }

        public string? ProofImagePath { get; set; }


        public bool IsPendingVerification { get; set; } = false;


        public bool IsVerified { get; set; } = false;


        public DateTime? CompletedAt { get; set; }


        public string? AdminNotes { get; set; }
    }
}