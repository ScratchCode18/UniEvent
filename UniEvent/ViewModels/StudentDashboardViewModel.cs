using UniEvent.Models;

namespace UniEvent.ViewModels;

public class StudentDashboardViewModel
{
    public int AvailableEventsCount { get; set; }
    public int RegisteredEventsCount { get; set; }
    public int UpcomingRegistrationsCount { get; set; }

    public List<Event> AvailableEvents { get; set; } = new();
    public List<EventRegistration> MyRegistrations { get; set; } = new();

    public HashSet<int> RegisteredEventIds { get; set; } = new();
}
