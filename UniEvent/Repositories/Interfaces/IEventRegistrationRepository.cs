
using UniEvent.Models;

namespace UniEvent.Repositories.Interfaces;

public interface IEventRegistrationRepository
{
    Task<List<EventRegistration>> GetByStudentAsync(string studentId);

    Task<bool> ExistsAsync(int eventId, string studentId);

    Task<bool> HasRegistrationsAsync(int eventId);

    Task AddAsync(EventRegistration registration);
}
