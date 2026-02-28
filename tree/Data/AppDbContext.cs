using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using EcoTraceApp.Models; // Added to access the new models

namespace EcoTraceApp.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // NEW: Tell the database to create these tables
        public DbSet<Event> Events { get; set; }
        public DbSet<EventRegistration> EventRegistrations { get; set; }

        // NEW: Configure the relationships (Foreign Keys) safely
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Crucial for the Login/Identity system to keep working

            // 1. Link Event to Admin (Creator)
            builder.Entity<Event>()
                .HasOne(e => e.Creator)
                .WithMany()
                .HasForeignKey(e => e.CreatorId)
                .OnDelete(DeleteBehavior.Restrict); // Prevents accidental deletion of an Admin if they have active events

            // 2. Link EventRegistration to Event
            builder.Entity<EventRegistration>()
                .HasOne(er => er.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(er => er.EventId)
                .OnDelete(DeleteBehavior.Cascade); // If an Event is deleted, delete all its registrations

            // 3. Link EventRegistration to User
            builder.Entity<EventRegistration>()
                .HasOne(er => er.User)
                .WithMany()
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Cascade); // If a User deletes their account, remove their registrations
        }
    }
}