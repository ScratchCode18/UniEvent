using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniEvent.Data;
using UniEvent.Models;
using UniEvent.ViewModels;

namespace UniEvent.Controllers;

[Authorize(Roles = "Organizer")]
public class OrganizerController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OrganizerController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var organizer = await _userManager.GetUserAsync(User);
        if (organizer == null || !organizer.IsActive) return Forbid();

        var events = await _context.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizer.Id)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        var model = new OrganizerDashboardViewModel
        {
            MyEvents = events,
            TotalEvents = events.Count,
            PendingEvents = events.Count(e => e.Status == EventStatus.Pending),
            ApprovedEvents = events.Count(e => e.Status == EventStatus.Approved),
            RejectedEvents = events.Count(e => e.Status == EventStatus.Rejected)
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new EventFormViewModel
        {
            Date = DateTime.Now.AddDays(1)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var organizer = await _userManager.GetUserAsync(User);
        if (organizer == null || !organizer.IsActive) return Forbid();

        var newEvent = new Event
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Date = model.Date,
            Venue = model.Venue.Trim(),
            Category = model.Category?.Trim(),
            OrganizerId = organizer.Id,
            Status = EventStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _context.Events.Add(newEvent);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your event has been submitted for admin approval.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var organizer = await _userManager.GetUserAsync(User);
        if (organizer == null || !organizer.IsActive) return Forbid();

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizer.Id);

        if (eventItem == null) return NotFound();

        var model = new EventFormViewModel
        {
            Title = eventItem.Title,
            Description = eventItem.Description,
            Date = eventItem.Date,
            Venue = eventItem.Venue,
            Category = eventItem.Category
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EventFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var organizer = await _userManager.GetUserAsync(User);
        if (organizer == null || !organizer.IsActive) return Forbid();

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizer.Id);

        if (eventItem == null) return NotFound();

        eventItem.Title = model.Title.Trim();
        eventItem.Description = model.Description.Trim();
        eventItem.Date = model.Date;
        eventItem.Venue = model.Venue.Trim();
        eventItem.Category = model.Category?.Trim();

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Event updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var organizer = await _userManager.GetUserAsync(User);
        if (organizer == null || !organizer.IsActive) return Forbid();

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id && e.OrganizerId == organizer.Id);

        if (eventItem == null) return NotFound();

        var hasRegistrations = await _context.EventRegistrations
            .AnyAsync(r => r.EventId == id);

        if (hasRegistrations)
        {
            TempData["ErrorMessage"] = "This event has registrations and cannot be deleted.";
            return RedirectToAction(nameof(Index));
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Event deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}
