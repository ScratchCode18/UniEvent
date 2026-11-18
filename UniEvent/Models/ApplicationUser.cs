using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace UniEvent.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [StringLength(100)]
    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;

    public ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();
    public ICollection<EventRegistration> Registrations { get; set; } = new List<EventRegistration>();
}
