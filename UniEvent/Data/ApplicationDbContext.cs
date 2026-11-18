using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using UniEvent.Models;

namespace UniEvent.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Event>(entity =>
        {
            entity.HasOne(e => e.Organizer)
                  .WithMany(u => u.OrganizedEvents)
                  .HasForeignKey(e => e.OrganizerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EventRegistration>(entity =>
        {
            entity.HasIndex(r => new { r.StudentId, r.EventId })
                  .IsUnique();

            entity.HasOne(r => r.Event)
                  .WithMany(e => e.Registrations)
                  .HasForeignKey(r => r.EventId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(r => r.Student)
                  .WithMany(u => u.Registrations)
                  .HasForeignKey(r => r.StudentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
