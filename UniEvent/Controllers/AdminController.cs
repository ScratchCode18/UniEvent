
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniEvent.Data;
using UniEvent.Models;
using UniEvent.ViewModels;

namespace UniEvent.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AdminDashboardViewModel
        {
            TotalUsers = await _userManager.Users.CountAsync(),

            TotalEvents = await _context.Events.CountAsync(),

            PendingEvents = await _context.Events
                .CountAsync(e => e.Status == EventStatus.Pending),

            ApprovedEvents = await _context.Events
                .CountAsync(e => e.Status == EventStatus.Approved),

            RejectedEvents = await _context.Events
                .CountAsync(e => e.Status == EventStatus.Rejected),

            PendingEventList = await _context.Events
                .Include(e => e.Organizer)
                .Where(e => e.Status == EventStatus.Pending)
                .OrderBy(e => e.Date)
                .ToListAsync()
        };

        return View(model);
    }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ApproveEvent(int id)
{
    var eventItem = await _context.Events.FindAsync(id);

    if (eventItem == null)
    {
        TempData["ErrorMessage"] = "Event not found.";
        return RedirectToAction(nameof(Index));
    }

    if (eventItem.Status != EventStatus.Pending)
    {
        TempData["ErrorMessage"] = "This event is no longer pending.";
        return RedirectToAction(nameof(Index));
    }

    eventItem.Status = EventStatus.Approved;

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Event approved successfully.";

    return RedirectToAction(nameof(Index));
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> RejectEvent(int id)
{
    var eventItem = await _context.Events.FindAsync(id);

    if (eventItem == null)
    {
        TempData["ErrorMessage"] = "Event not found.";
        return RedirectToAction(nameof(Index));
    }

    if (eventItem.Status != EventStatus.Pending)
    {
        TempData["ErrorMessage"] = "This event is no longer pending.";
        return RedirectToAction(nameof(Index));
    }

    eventItem.Status = EventStatus.Rejected;

    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = "Event rejected successfully.";

    return RedirectToAction(nameof(Index));
}

[HttpGet]
public async Task<IActionResult> ManageUsers()
{
    var users = await _userManager.Users
        .OrderBy(u => u.FullName)
        .ToListAsync();

    var model = new List<AdminUserViewModel>();

    foreach (var user in users)
    {
        var roles = await _userManager.GetRolesAsync(user);

        model.Add(new AdminUserViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? "",
            Role = roles.FirstOrDefault() ?? "No Role",
            IsActive = user.IsActive
        });
    }

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> PromoteToOrganizer(string id)
{
    var user = await _userManager.FindByIdAsync(id);

    if (user == null)
    {
        TempData["ErrorMessage"] = "User not found.";
        return RedirectToAction(nameof(ManageUsers));
    }

    if (!user.IsActive)
    {
        TempData["ErrorMessage"] = "Activate this account before promoting it.";
        return RedirectToAction(nameof(ManageUsers));
    }

    if (!await _userManager.IsInRoleAsync(user, "Student"))
    {
        TempData["ErrorMessage"] = "Only Students can be promoted.";
        return RedirectToAction(nameof(ManageUsers));
    }

    var result = await _userManager.AddToRoleAsync(user, "Organizer");

    if (!result.Succeeded)
    {
        TempData["ErrorMessage"] = "Could not assign Organizer role.";
        return RedirectToAction(nameof(ManageUsers));
    }

    var removeStudentResult =
        await _userManager.RemoveFromRoleAsync(user, "Student");

    if (!removeStudentResult.Succeeded)
    {
        await _userManager.RemoveFromRoleAsync(user, "Organizer");

        TempData["ErrorMessage"] =
            "Promotion failed. The original Student role was preserved.";

        return RedirectToAction(nameof(ManageUsers));
    }

    await _userManager.UpdateSecurityStampAsync(user);

    TempData["SuccessMessage"] =
        $"{user.FullName} has been promoted to Organizer.";

    return RedirectToAction(nameof(ManageUsers));
}


[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleUserStatus(string id)
{
    var user = await _userManager.FindByIdAsync(id);

    if (user == null)
    {
        TempData["ErrorMessage"] = "User not found.";
        return RedirectToAction(nameof(ManageUsers));
    }

    var currentUserId = _userManager.GetUserId(User);

    if (user.Id == currentUserId && user.IsActive)
    {
        TempData["ErrorMessage"] = "You cannot deactivate your own account.";
        return RedirectToAction(nameof(ManageUsers));
    }

    if (user.IsActive)
    {
        var roles = await _userManager.GetRolesAsync(user);

        if (roles.Contains("Admin"))
        {
            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            var activeAdmins = admins.Count(a => a.IsActive);

            if (activeAdmins <= 1)
            {
                TempData["ErrorMessage"] =
                    "You cannot deactivate the last active Admin.";
                return RedirectToAction(nameof(ManageUsers));
            }
        }
    }

    user.IsActive = !user.IsActive;

    var result = await _userManager.UpdateAsync(user);
    if (result.Succeeded)
    {
        await _userManager.UpdateSecurityStampAsync(user);
    }

    TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
        result.Succeeded
            ? $"Account {(user.IsActive ? "activated" : "deactivated")} successfully."
            : "Could not update account status.";

    return RedirectToAction(nameof(ManageUsers));
}

}
