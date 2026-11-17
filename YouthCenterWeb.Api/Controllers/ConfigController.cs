using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers;

[ApiController]
[Route("api/config")]
[AllowAnonymous]
public class ConfigController(IConfiguration cfg) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new BaseResponse<AppConfigDto>
    {
        Result = 1,
        Data = new AppConfigDto
        {
            PaymentsEnabled = cfg.GetValue<bool>("Payments:Enabled"),
            CancelCutoffHours = cfg.GetValue("Reservations:CancelCutoffHours", 2),
            MinAppVersion = cfg["App:MinVersion"] ?? "0.0.0",
            LatestAppVersion = cfg["App:LatestVersion"] ?? "0.0.0",
            StoreUrlAndroid = cfg["App:AndroidUrl"],
            StoreUrlIos = cfg["App:IosUrl"]
        }
    });
}