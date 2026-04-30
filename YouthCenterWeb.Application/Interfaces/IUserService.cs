// using YouthCenterWeb.DTOs;
// using YouthCenterWeb.Models;

// namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
// {
//     public interface IUserService
//     {
//         int UserId { get; }

//         string Role { get; }
//         int YouthCenterId { get; }

//         Task<UserDto?> GetByEmailAsync(string email);
//         Task<UserDto?> GetByIdAsync(int? id);

//         Task<List<UserDto>> GetAllAsync();

//         Task<UserDto?> UpdateAsync(UpdateUserDto dto);
//     }
// }

// Application/Interfaces/IUserService.cs

// using YouthCenterWeb.DTOs;
// using YouthCenterWeb.Models;

// namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
// {
//     public interface IUserService
//     {
//         int UserId { get; }

//         string Role { get; }
//         int YouthCenterId { get; }

//         Task<UserDto?> GetByEmailAsync(string email);
//         Task<UserDto?> GetByIdAsync(int? id);

//         Task<List<UserDto>> GetAllAsync();

//         Task<UserDto?> UpdateAsync(UpdateUserDto dto);
//     }
// }

// Application/Interfaces/IUserService.cs
using YouthCenterWeb.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IUserService
    {
        Task<List<UserDto>> GetAllAsync();
        Task<UserDto?> GetByIdAsync(int id);
        Task<UserDto?> GetByEmailAsync(string email);
        Task<UserDto?> UpdateAsync(int userId, UpdateUserDto dto);
    }
}