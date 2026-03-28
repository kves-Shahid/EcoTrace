using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EcoTraceApp.Models
{
    public enum NotificationType
    {
        TaskAssignment,  
        TaskVerification, 
        Announcement,     
        Achievement,      
        System            
    }

    public class Notification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual IdentityUser? User { get; set; }

        [Required]
        [MaxLength(250)] 
        public string Message { get; set; } = string.Empty;

        public string? LinkUrl { get; set; }

        
        public NotificationType Type { get; set; } = NotificationType.System;

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}