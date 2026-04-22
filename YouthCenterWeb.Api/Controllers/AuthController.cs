using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.Data.DTOs;

namespace YouthCenterWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(CreateUserDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        return Ok(result);
    }
}