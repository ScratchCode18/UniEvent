
using Microsoft.AspNetCore.Mvc;
using UniEvent.Models;
using UniEvent.Repositories.Interfaces;
using UniEvent.ViewModels;

namespace UniEvent.Controllers;

public class HomeController : Controller
{
    private readonly IEventRepository _eventRepository;

    public HomeController(IEventRepository eventRepository)
    {
        _eventRepository = eventRepository;
    }

    public async Task<IActionResult> Index()
    {
        var upcomingEvents =
            await _eventRepository.GetUpcomingApprovedAsync(DateTime.Now);

        var viewModel = new HomeViewModel
        {
            UpcomingEvents = upcomingEvents
        };

        return View(viewModel);
    }

[HttpGet]
public async Task<IActionResult> SearchEvents(string? searchTerm)
{
    var events = await _eventRepository
        .GetUpcomingApprovedAsync(DateTime.Now);

    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        searchTerm = searchTerm.Trim();

        events = events.Where(e =>
            e.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            (e.Category != null &&
             e.Category.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
            e.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();
    }

    return PartialView("_EventCards", events);
}


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
