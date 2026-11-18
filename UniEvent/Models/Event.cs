using System.ComponentModel.DataAnnotations;

namespace UniEvent.Models;

public class Event
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    [StringLength(150)]
    public string Venue { get; set; } = string.Empty;

    [StringLength(80)]
    public string? Category { get; set; }

    [Required]
    public string OrganizerId { get; set; } = string.Empty;
    public ApplicationUser? Organizer { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}
