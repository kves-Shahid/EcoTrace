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

        // NEW: Tell the database to create the ChatMessages table
        public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // 1. Link Event to Admin (Creator)
            builder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2. Link EventRegistration to Event
            builder.Entity<EventRegistration>()
                .HasOne(er => er.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3. Link EventRegistration to User
            builder.Entity<EventRegistration>()
                .HasOne(er => er.User)
                .WithMany()
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 4. NEW: Link ChatMessage to Event
            builder.Entity<ChatMessage>()
                .HasOne(c => c.Event)
                .WithMany()
                .HasForeignKey(c => c.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            // 5. NEW: Link ChatMessage to User
            builder.Entity<ChatMessage>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}