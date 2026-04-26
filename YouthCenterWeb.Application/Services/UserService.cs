using System.Security.Claims;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class UserService(IUserRepo userRepository, IHttpContextAccessor httpContextAccessor, IMapper<User, UserDto, CreateUserDto> userMapper) : IUserService
    {
        private readonly IUserRepo _userRepository = userRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly IMapper<User, UserDto, CreateUserDto> _userMapper = userMapper;

        public int UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    return userId;
                }
                throw new Exception("User ID claim not found or invalid.");
            }
        }

        public int RoleId
        {
            get
            {
                var roleIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role);
                if (roleIdClaim != null && int.TryParse(roleIdClaim.Value, out int roleId))
                {
                    return roleId;
                }
                throw new Exception("Role ID claim not found or invalid.");
            }
        }

        public async Task<List<UserDto>> GetAllAsync()
        {
            var users = await _userRepository.GetAllAsync(u => u.Reservations);
            return users.Select(u => _userMapper.ToDto(u)).ToList();
        }

        public async Task<UserDto?> GetByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email, u => u.Reservations);
            return user != null ? _userMapper.ToDto(user) : null;
        }

        public async Task<UserDto?> GetByIdAsync(int? id)
        {
            var role = RoleId;
            if (role == 1 || id == null)
            {

                id = UserId;
            }
            var user = await _userRepository.GetByIdAsync(id, u => u.Reservations);
            return user != null ? _userMapper.ToDto(user) : null;
        }
    }


}