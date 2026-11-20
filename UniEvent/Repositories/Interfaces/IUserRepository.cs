
using UniEvent.Models;

namespace UniEvent.Repositories.Interfaces;

public interface IUserRepository
{
    Task<List<ApplicationUser>> GetAllAsync();

    Task<ApplicationUser?> GetByIdAsync(string id);

    Task<int> GetTotalCountAsync();
}
