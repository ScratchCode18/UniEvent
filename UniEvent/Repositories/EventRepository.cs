
using Microsoft.EntityFrameworkCore;
using UniEvent.Data;
using UniEvent.Models;
using UniEvent.Repositories.Interfaces;

namespace UniEvent.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Event>> GetAllAsync()
    {
        return await _context.Events
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<Event>> GetUpcomingApprovedAsync(DateTime currentDate)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.Status == EventStatus.Approved &&
                        e.Date >= currentDate)
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<List<Event>> GetByOrganizerAsync(string organizerId)
    {
        return await _context.Events
            .AsNoTracking()
            .Where(e => e.OrganizerId == organizerId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event?> GetByIdForOrganizerAsync(int id, string organizerId)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e =>
                e.Id == id && e.OrganizerId == organizerId);
    }

    public async Task<List<Event>> GetPendingWithOrganizerAsync()
    {
        return await _context.Events
            .Include(e => e.Organizer)
            .Where(e => e.Status == EventStatus.Pending)
            .OrderBy(e => e.Date)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Events.CountAsync();
    }

    public async Task<int> GetCountByStatusAsync(EventStatus status)
    {
        return await _context.Events
            .CountAsync(e => e.Status == status);
    }

    public async Task AddAsync(Event eventItem)
    {
        await _context.Events.AddAsync(eventItem);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event eventItem)
    {
        _context.Events.Update(eventItem);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Event eventItem)
    {
        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();
    }
}
