using System.ComponentModel.DataAnnotations;

namespace UniEvent.Models;

public class EventRegistration
{
    public int Id { get; set; }

    [Required]
    public int EventId { get; set; }
    public Event? Event { get; set; }

    [Required]
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser? Student { get; set; }

    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
}
