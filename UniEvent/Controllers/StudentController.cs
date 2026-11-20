
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniEvent.Models;
using UniEvent.Repositories.Interfaces;
using UniEvent.ViewModels;

namespace UniEvent.Controllers;

[Authorize(Roles = "Student")]
public class StudentController : Controller
{
    private readonly IEventRepository _eventRepository;
    private readonly IEventRegistrationRepository _registrationRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public StudentController(
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
        var student = await _userManager.GetUserAsync(User);

        if (student == null || !student.IsActive)
            return Forbid();

        var now = DateTime.Now;

        var availableEvents =
            await _eventRepository.GetUpcomingApprovedAsync(now);

        var registrations =
            await _registrationRepository.GetByStudentAsync(student.Id);

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

        var eventItem = await _eventRepository.GetByIdAsync(eventId);

        if (eventItem == null ||
            eventItem.Status != EventStatus.Approved ||
            eventItem.Date < DateTime.Now)
        {
            return Json(new
            {
                success = false,
                message = "This event is no longer available for registration."
            });
        }

        var alreadyRegistered =
            await _registrationRepository.ExistsAsync(eventId, student.Id);

        if (alreadyRegistered)
        {
            return Json(new
            {
                success = false,
                message = "You are already registered for this event."
            });
        }

        var registration = new EventRegistration
        {
            EventId = eventId,
            StudentId = student.Id,
            RegistrationDate = DateTime.UtcNow
        };

        try
        {
            await _registrationRepository.AddAsync(registration);

            return Json(new
            {
                success = true,
                message = "You have successfully registered for the event."
            });
        }
        catch (DbUpdateException)
        {
            return Json(new
            {
                success = false,
                message = "Registration could not be completed. You may already be registered."
            });
        }
    }
}
