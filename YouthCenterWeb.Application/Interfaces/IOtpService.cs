using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IOtpService
    {
        Task<BaseResponse<object>> SendOtpAsync(SendOtpDto dto);
        Task<BaseResponse<object>> VerifyOtpAsync(VerifyOtpDto dto);
    }
}