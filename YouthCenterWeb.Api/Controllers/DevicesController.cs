// WebApi/Controllers/DevicesController.cs
namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers;

using global::YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly INotificationService _notificationService;
    public DevicesController(INotificationService notificationService)
        => _notificationService = notificationService;

    public record RegisterDeviceRequest(string Token, string Platform);

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDeviceRequest req)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.RegisterDeviceAsync(userId, req.Token, req.Platform);
        return Ok();
    }

    [HttpPost("unregister")]
    public async Task<IActionResult> Unregister()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        await _notificationService.UnregisterDeviceAsync(userId);
        return Ok();
    }
}