using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcoTraceApp.Models;

namespace EcoTraceApp.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<AiChatMessage> AiChatMessages { get; set; }

      
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notification> Notifications { get; set; } 
        public DbSet<EventGalleryImage> EventGalleryImages { get; set; }
        public DbSet<EventTask> EventTasks { get; set; }
        public DbSet<EventAnnouncement> EventAnnouncements { get; set; }

        public DbSet<Donation> Donations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

        
            builder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            
            builder.Entity<EventRegistration>()
                .HasOne(er => er.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EventRegistration>()
                .HasOne(er => er.User)
                .WithMany()
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade);

           
            builder.Entity<ChatMessage>()
                .HasOne(c => c.Event)
                .WithMany()
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ChatMessage>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.Entity<Comment>()
                .HasOne(c => c.Event)
                .WithMany(e => e.Comments)
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

           
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            
            builder.Entity<EventTask>()
                .HasOne(t => t.AssignedUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedUserId)
                .OnDelete(DeleteBehavior.SetNull); 
        }
    }
}