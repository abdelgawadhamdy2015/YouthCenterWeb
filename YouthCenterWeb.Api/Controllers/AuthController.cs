using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IAuthService authService,
        IOtpService otpService) : ControllerBase
    {
        // ── POST api/auth/send-otp ─────────────────────────────────────────────
        [HttpPost("send-otp")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]

        public async Task<IActionResult> SendOtp(SendOtpDto dto)
        {
            var result = await otpService.SendOtpAsync(dto);
            return Ok(result);
        }

        // ── POST api/auth/verify-otp ───────────────────────────────────────────
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp(VerifyOtpDto dto)
        {
            var result = await otpService.VerifyOtpAsync(dto);
            return Ok(result);
        }

        // ── POST api/auth/register ─────────────────────────────────────────────
        [HttpPost("register")]
        [AllowAnonymous]
        [EnableRateLimiting("otp")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await authService.RegisterAsync(dto);
            return Ok(result);
        }

        // ── POST api/auth/login ────────────────────────────────────────────────
        [HttpPost("login")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await authService.LoginAsync(dto);
            return Ok(result);
        }

        // ── POST api/auth/create-admin — SuperAdmin only ───────────────────────
        [HttpPost("create-admin")]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
        {
            var result = await authService.CreateAdminAsync(dto);
            return Ok(result);
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            var result = await authService.ResetPasswordAsync(dto);
            return Ok(result);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        [EnableRateLimiting("refresh")]
        public async Task<IActionResult> Refresh(RefreshTokenDto dto)
        {
            var result = await authService.RefreshAsync(dto);
            return result.Result == 1 ? Ok(result) : Unauthorized(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var result = await authService.LogoutAsync(User.GetUserId());
            return Ok(result);
        }
    }
}