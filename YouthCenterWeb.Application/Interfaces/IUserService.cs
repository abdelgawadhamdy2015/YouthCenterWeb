using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IUserService
    {
        int UserId { get; }

        int RoleId { get; }

        Task<UserDto?> GetByEmailAsync(string email);
        Task<UserDto?> GetByIdAsync(int? id);

        Task<List<UserDto>> GetAllAsync();
    }
}