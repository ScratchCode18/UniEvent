
using Microsoft.EntityFrameworkCore;
using UniEvent.Data;
using UniEvent.Models;
using UniEvent.Repositories.Interfaces;

namespace UniEvent.Repositories;

public class EventRegistrationRepository : IEventRegistrationRepository
{
    private readonly ApplicationDbContext _context;

    public EventRegistrationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<EventRegistration>> GetByStudentAsync(string studentId)
    {
        return await _context.EventRegistrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();
    }

    public async Task<bool> ExistsAsync(int eventId, string studentId)
    {
        return await _context.EventRegistrations
            .AnyAsync(r =>
                r.EventId == eventId &&
                r.StudentId == studentId);
    }

    public async Task<bool> HasRegistrationsAsync(int eventId)
    {
        return await _context.EventRegistrations
            .AnyAsync(r => r.EventId == eventId);
    }

    public async Task AddAsync(EventRegistration registration)
    {
        await _context.EventRegistrations.AddAsync(registration);
        await _context.SaveChangesAsync();
    }
}
