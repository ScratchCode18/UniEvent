using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UniEvent.Models;

namespace UniEvent.Data;

public static class DbSeeder
{
    public static readonly string[] Roles = ["Admin", "Organizer", "Student"];

    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var provider = scope.ServiceProvider;

        var context = provider.GetRequiredService<ApplicationDbContext>();
        var userManager = provider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = provider.GetRequiredService<RoleManager<IdentityRole>>();
        var configuration = provider.GetRequiredService<IConfiguration>();
        var environment = provider.GetRequiredService<IWebHostEnvironment>();
        var logger = provider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await context.Database.MigrateAsync();

        // 1. Seed Roles
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

                
        // 2. Seed Initial Admin from Configuration / User Secrets
        var adminEmail = configuration["AdminSeed:Email"];
        var adminPassword = configuration["AdminSeed:Password"];
        var adminFullName = configuration["AdminSeed:FullName"] ?? "System Administrator";

        if (!string.IsNullOrWhiteSpace(adminEmail) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName,
                    Department = "Administration",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var createResult = await userManager.CreateAsync(adminUser, adminPassword);

                if (!createResult.Succeeded)
                {
                    logger.LogWarning("Failed to seed Admin user: {Errors}",
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));

                    adminUser = null;
                }
                else
                {
                    logger.LogInformation("Seeded initial Admin user: {Email}", adminEmail);
                }
            }

            if (adminUser is not null)
            {
                if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
                {
                    var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                    if (!roleResult.Succeeded)
                    {
                        logger.LogWarning("Failed to assign Admin role: {Errors}",
                            string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    }
                }

                // One-time password reset, enabled only through User Secrets.
                if (configuration.GetValue<bool>("AdminSeed:ResetPassword"))
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                    var resetResult = await userManager.ResetPasswordAsync(
                        adminUser, token, adminPassword);

                    if (resetResult.Succeeded)
                    {
                        await userManager.SetLockoutEndDateAsync(adminUser, null);
                        await userManager.ResetAccessFailedCountAsync(adminUser);

                        logger.LogInformation(
                            "Admin password reset and lockout cleared for {Email}", adminEmail);
                    }
                    else
                    {
                        logger.LogWarning("Admin password reset failed: {Errors}",
                            string.Join(", ", resetResult.Errors.Select(e => e.Description)));
                    }
                }
            }
        }
        else
        {
            logger.LogWarning(
                "AdminSeed:Email or AdminSeed:Password not configured. Skipping initial Admin creation.");
        }

        // 3. Seed Demo Data in Development Environment
        if (!environment.IsDevelopment())
        {
            return;
        }

        var organizerEmail = "organizer@unievent.edu";
        var organizerUser = await userManager.FindByEmailAsync(organizerEmail);
        if (organizerUser is null)
        {
            organizerUser = new ApplicationUser
            {
                UserName = organizerEmail,
                Email = organizerEmail,
                FullName = "Sara Ahmed",
                Department = "Computer Science Society",
                EmailConfirmed = true,
                IsActive = true
            };
            if ((await userManager.CreateAsync(organizerUser, "Organizer@123!")).Succeeded)
            {
                await userManager.AddToRoleAsync(organizerUser, "Organizer");
            }
        }

        var studentEmail = "student@unievent.edu";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        if (studentUser is null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FullName = "Ali Khan",
                Department = "Software Engineering",
                EmailConfirmed = true,
                IsActive = true
            };
            if ((await userManager.CreateAsync(studentUser, "Student@123!")).Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }

        if (!await context.Events.AnyAsync() && organizerUser is not null)
        {
            var demoEvents = new List<Event>
            {
                new()
                {
                    Title = "Tech Workshop",
                    Description = "Hands-on session covering modern ASP.NET Core MVC web development, RESTful design, and cloud deployment.",
                    Date = DateTime.Today.AddDays(14).AddHours(10),
                    Venue = "Computer Lab 3",
                    Category = "Technology",
                    OrganizerId = organizerUser.Id,
                    Status = EventStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-5)
                },
                new()
                {
                    Title = "Sports Festival",
                    Description = "Annual inter-departmental university sports competitions featuring football, badminton, and athletics.",
                    Date = DateTime.Today.AddDays(21).AddHours(9),
                    Venue = "University Sports Complex",
                    Category = "Sports",
                    OrganizerId = organizerUser.Id,
                    Status = EventStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-4)
                },
                new()
                {
                    Title = "Career Seminar",
                    Description = "Explore software engineering internships and graduate career opportunities with industry professionals.",
                    Date = DateTime.Today.AddDays(28).AddHours(14),
                    Venue = "Main Auditorium",
                    Category = "Career",
                    OrganizerId = organizerUser.Id,
                    Status = EventStatus.Approved,
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new()
                {
                    Title = "AI & Machine Learning Symposium",
                    Description = "Student research showcase and guest talks on applied machine learning and generative AI.",
                    Date = DateTime.Today.AddDays(35).AddHours(11),
                    Venue = "Seminar Hall B",
                    Category = "Technology",
                    OrganizerId = organizerUser.Id,
                    Status = EventStatus.Pending,
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    Title = "Late Night Gaming Marathon",
                    Description = "Overnight campus LAN tournament (rejected due to venue curfew policy).",
                    Date = DateTime.Today.AddDays(18).AddHours(22),
                    Venue = "Student Center",
                    Category = "Cultural",
                    OrganizerId = organizerUser.Id,
                    Status = EventStatus.Rejected,
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                }
            };

            context.Events.AddRange(demoEvents);
            await context.SaveChangesAsync();
        }
    }
}
