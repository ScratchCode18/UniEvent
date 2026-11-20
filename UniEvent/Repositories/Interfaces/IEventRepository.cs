
using UniEvent.Models;

namespace UniEvent.Repositories.Interfaces;

public interface IEventRepository
{
    Task<List<Event>> GetAllAsync();

    Task<List<Event>> GetUpcomingApprovedAsync(DateTime currentDate);

    Task<List<Event>> GetByOrganizerAsync(string organizerId);

    Task<Event?> GetByIdAsync(int id);

    Task<Event?> GetByIdForOrganizerAsync(int id, string organizerId);

    Task<List<Event>> GetPendingWithOrganizerAsync();

    Task<int> GetTotalCountAsync();

    Task<int> GetCountByStatusAsync(EventStatus status);

    Task AddAsync(Event eventItem);

    Task UpdateAsync(Event eventItem);

    Task DeleteAsync(Event eventItem);
}
