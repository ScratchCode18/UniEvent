using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniEvent.Data;
using UniEvent.Models;
using UniEvent.ViewModels;

namespace UniEvent.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var upcomingEvents = await _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Approved && e.Date >= DateTime.Now)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var viewModel = new HomeViewModel
        {
            UpcomingEvents = upcomingEvents
        };

        return View(viewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
