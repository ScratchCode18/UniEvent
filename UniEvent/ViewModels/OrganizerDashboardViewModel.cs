using UniEvent.Models;

namespace UniEvent.ViewModels;

public class OrganizerDashboardViewModel
{
    public List<Event> MyEvents { get; set; } = new();

    public int TotalEvents { get; set; }
    public int PendingEvents { get; set; }
    public int ApprovedEvents { get; set; }
    public int RejectedEvents { get; set; }
}
