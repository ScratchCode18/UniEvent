using UniEvent.Models;

namespace UniEvent.ViewModels;

public class AdminDashboardViewModel
{
    public int TotalUsers { get; set; }
    public int TotalEvents { get; set; }
    public int PendingEvents { get; set; }
    public int ApprovedEvents { get; set; }
    public int RejectedEvents { get; set; }

    public List<Event> PendingEventList { get; set; } = new();
}