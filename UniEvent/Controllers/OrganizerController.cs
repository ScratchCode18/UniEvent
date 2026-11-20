
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UniEvent.Models;
using UniEvent.Repositories.Interfaces;
using UniEvent.ViewModels;

namespace UniEvent.Controllers;

[Authorize(Roles = "Organizer")]
public class OrganizerController : Controller
{
    private readonly IEventRepository _eventRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEventRegistrationRepository _registrationRepository;

    public OrganizerController(
        IEventRepository eventRepository,
        IEventRegistrationRepository registrationRepository,
        UserManager<ApplicationUser> userManager)
    {
        _eventRepository = eventRepository;
        _registrationRepository = registrationRepository;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var organizer = await _userManager.GetUserAsync(User);

        if (organizer == null || !organizer.IsActive)
            return Forbid();

        var events = await _eventRepository.GetByOrganizerAsync(organizer.Id);

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
        if (!ModelState.IsValid)
            return View(model);

        var organizer = await _userManager.GetUserAsync(User);

        if (organizer == null || !organizer.IsActive)
            return Forbid();

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

        await _eventRepository.AddAsync(newEvent);

        TempData["SuccessMessage"] =
            "Your event has been submitted for admin approval.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var organizer = await _userManager.GetUserAsync(User);

        if (organizer == null || !organizer.IsActive)
            return Forbid();

        var eventItem = await _eventRepository
            .GetByIdForOrganizerAsync(id, organizer.Id);

        if (eventItem == null)
            return NotFound();

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
        if (!ModelState.IsValid)
            return View(model);

        var organizer = await _userManager.GetUserAsync(User);

        if (organizer == null || !organizer.IsActive)
            return Forbid();

        var eventItem = await _eventRepository
            .GetByIdForOrganizerAsync(id, organizer.Id);

        if (eventItem == null)
            return NotFound();

        eventItem.Title = model.Title.Trim();
        eventItem.Description = model.Description.Trim();
        eventItem.Date = model.Date;
        eventItem.Venue = model.Venue.Trim();
        eventItem.Category = model.Category?.Trim();

        await _eventRepository.UpdateAsync(eventItem);

        TempData["SuccessMessage"] = "Event updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var organizer = await _userManager.GetUserAsync(User);

        if (organizer == null || !organizer.IsActive)
            return Forbid();

        var eventItem = await _eventRepository
            .GetByIdForOrganizerAsync(id, organizer.Id);

        if (eventItem == null)
            return NotFound();

        var hasRegistrations =
            await _registrationRepository.HasRegistrationsAsync(id);

        if (hasRegistrations)
        {
            TempData["ErrorMessage"] =
                "This event has registrations and cannot be deleted.";

            return RedirectToAction(nameof(Index));
        }

        await _eventRepository.DeleteAsync(eventItem);

        TempData["SuccessMessage"] = "Event deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}
