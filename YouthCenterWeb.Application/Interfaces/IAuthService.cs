using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.Application.Interfaces;

public interface IAuthService
{
    Task<BaseResponse<LoginData>> LoginAsync(LoginDto dto);
    Task<BaseResponse<LoginData>> RegisterAsync(RegisterDto dto);
    Task<BaseResponse<LoginData>> CreateAdminAsync(CreateAdminDto dto);



}