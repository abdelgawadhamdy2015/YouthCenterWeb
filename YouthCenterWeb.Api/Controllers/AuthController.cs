using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IAuthService authService,
    IOtpService otpService) : ControllerBase
{
    // ── POST api/auth/send-otp ─────────────────────────────────────────────
    [HttpPost("send-otp")]
    [AllowAnonymous]
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
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        return Ok(result);
    }

    // ── POST api/auth/login ────────────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(result);
    }

    // ── POST api/auth/create-admin — SuperAdmin only ───────────────────────
    [HttpPost("create-admin")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
    {
        var result = await authService.CreateAdminAsync(dto);
        return Ok(result);
    }
}