using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EcoTraceApp.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Reason { get; set; } = string.Empty;
        public string TargetType { get; set; } = "Event"; 
        public int TargetId { get; set; } 

        public string ReportedById { get; set; } = string.Empty;
        public virtual IdentityUser? ReportedBy { get; set; }

        public bool IsResolved { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}