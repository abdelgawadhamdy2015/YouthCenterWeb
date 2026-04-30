using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;

public interface IAuthRepo
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    Task SaveChangesAsync();
}