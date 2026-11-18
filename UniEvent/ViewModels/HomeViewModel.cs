using UniEvent.Models;

namespace UniEvent.ViewModels;

public class HomeViewModel
{
    public List<Event> UpcomingEvents { get; set; } = new();
}
