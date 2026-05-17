using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(policy: Policies.RequireAdmin)]

    public class DashboardController(IDashboardService service) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetDashboardData()
        {

            var data = await service.GetDashboardOverviewDtoAsync(youthCenterId: User.GetYouthCenterId());
            return Ok(new BaseResponse<DashboardOverviewDto>
            {
                Result = 1,
                Data = data,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }

            });
        }
    }
}