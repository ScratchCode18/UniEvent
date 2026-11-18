using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniEvent.Data;
using UniEvent.Models;
using UniEvent.ViewModels;

namespace UniEvent.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var student = await _userManager.GetUserAsync(User);

        if (student == null || !student.IsActive)
            return Forbid();

        var now = DateTime.Now;

        var availableEvents = await _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Approved && e.Date >= now)
            .OrderBy(e => e.Date)
            .ToListAsync();

        var registrations = await _context.EventRegistrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Where(r => r.StudentId == student.Id)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();

        var registeredEventIds = registrations
            .Select(r => r.EventId)
            .ToHashSet();

        var viewModel = new StudentDashboardViewModel
        {
            AvailableEvents = availableEvents,
            MyRegistrations = registrations,
            RegisteredEventIds = registeredEventIds,
            AvailableEventsCount = availableEvents.Count,
            RegisteredEventsCount = registrations.Count,
            UpcomingRegistrationsCount = registrations.Count(r =>
                r.Event != null &&
                r.Event.Date >= now &&
                r.Event.Status == EventStatus.Approved)
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(int eventId)
    {
        var student = await _userManager.GetUserAsync(User);

        if (student == null || !student.IsActive)
            return Forbid();

        var eventItem = await _context.Events
            .FirstOrDefaultAsync(e =>
                e.Id == eventId &&
                e.Status == EventStatus.Approved &&
                e.Date >= DateTime.Now);

        if (eventItem == null)
        {
            TempData["ErrorMessage"] = "This event is no longer available for registration.";
            return RedirectToAction(nameof(Index));
        }

        var alreadyRegistered = await _context.EventRegistrations
            .AnyAsync(r => r.EventId == eventId && r.StudentId == student.Id);

        if (alreadyRegistered)
        {
            TempData["ErrorMessage"] = "You are already registered for this event.";
            return RedirectToAction(nameof(Index));
        }

        var registration = new EventRegistration
        {
            EventId = eventId,
            StudentId = student.Id,
            RegistrationDate = DateTime.UtcNow
        };

        _context.EventRegistrations.Add(registration);

        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "You have successfully registered for the event.";
        }
        catch (DbUpdateException)
        {
            _context.Entry(registration).State = EntityState.Detached;
            TempData["ErrorMessage"] = "Registration could not be completed. You may already be registered.";
        }

        return RedirectToAction(nameof(Index));
    }
}
