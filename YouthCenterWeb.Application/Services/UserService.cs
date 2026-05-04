
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class UserService(
        IUserRepo userRepository,
        IMapper<User, UserDto, RegisterDto> userMapper) : IUserService
    {

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await userRepository.GetAllAsync(
                u => u.Reservations,
                u => u.Role!);

            return users.Select(userMapper.ToDto).ToList();
        }

        public async Task<UserDto?> GetByIdAsync(int id)
        {
            var user = await userRepository.GetByIdAsync(
                id,
                u => u.Reservations
                );

            return user != null ? userMapper.ToDto(user) : null;
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await userRepository.GetByEmailAsync(
                email,
                u => u.Reservations,
                u => u.Role!,
                u => u.YouthCenter!);

            return user != null ? userMapper.ToDto(user) : null;
        }

        public async Task<UserDto?> UpdateAsync(int userId, UpdateUserDto dto)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User", userId);

            user.Name = dto.Name;
            user.Mobile = dto.Mobile;
            user.ImageUrl = dto.ImageUrl;

            userRepository.Update(user);
            await userRepository.SaveChangesAsync();
            return userMapper.ToDto(user);
        }

        public async Task DeleteAsync(int userId)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User", userId);

            userRepository.Delete(user);          // ← entity passed directly
            await userRepository.SaveChangesAsync();
        }
    }
}