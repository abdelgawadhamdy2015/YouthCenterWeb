// Api/Controllers/AuthController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    // ── POST api/auth/register — public ───────────────────────────────────────
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var result = await authService.RegisterAsync(dto);
        return Ok(result);
    }

    // ── POST api/auth/login — public ──────────────────────────────────────────
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await authService.LoginAsync(dto);
        return Ok(result);
    }

    // ── POST api/auth/create-admin — SuperAdmin only ──────────────────────────
    [HttpPost("create-admin")]
    [Authorize(Policy = Policies.RequireSuperAdmin)]
    public async Task<IActionResult> CreateAdmin(CreateAdminDto dto)
    {
        var result = await authService.CreateAdminAsync(dto);
        return Ok(result);
    }
}

