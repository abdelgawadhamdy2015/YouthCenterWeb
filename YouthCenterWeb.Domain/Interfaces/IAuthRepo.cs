using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface IAuthRepo
    {
        Task<User?> GetByEmailAsync(string email);
        Task AddAsync(User user);
        Task SaveChangesAsync();
        Task UpdateSessionAsync(User user, Guid sessionId);
        Task<User?> GetByIdAsync(int id);
        Task ClearSessionAsync(User user);
        Task UpdatePasswordAsync(User user, string hashedPassword);
    }
}