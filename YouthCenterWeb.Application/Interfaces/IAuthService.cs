using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.Application.Interfaces;

public interface IAuthService
{
    Task<BaseResponse<LoginData>> LoginAsync(LoginDto dto);
    Task<BaseResponse<LoginData>> RegisterAsync(CreateUserDto dto);
}