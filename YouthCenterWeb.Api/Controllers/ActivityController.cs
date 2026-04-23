using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ActivityController(IGenericService<Activity, ActivityDto> genericService) : ControllerBase
    {
        private readonly IGenericService<Activity, ActivityDto> service = genericService;



        [HttpGet]
        public async Task<IActionResult> GetActivities()
        {

            var activities = await service.GetAllAsync(
                a => a.YouthCenter,
                a => a.Reservations
            );

            if (activities.Count == 0) return NotFound(new BaseResponse<RoleDto>
            {
                Result = 0,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.NoDataAr,
                    MessageEn = Messages.Data.NoDataEn
                }
            });
            return Ok(new BaseResponse<List<ActivityDto>>
            {
                Result = 1,
                Data = activities,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }

            });
        }

    }
}