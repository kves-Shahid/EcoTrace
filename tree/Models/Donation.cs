using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace EcoTraceApp.Models
{
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(1.00, 10000.00)]
        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

        public string? StripeSessionId { get; set; } // To track the transaction

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- Relationships ---

        // Who donated? (Nullable in case of anonymous guest donations)
        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual IdentityUser? User { get; set; }

        // Optional: Did they donate to a specific mission, or the general platform?
        public int? EventId { get; set; }
        [ForeignKey("EventId")]
        public virtual Event? Event { get; set; }
    }
}