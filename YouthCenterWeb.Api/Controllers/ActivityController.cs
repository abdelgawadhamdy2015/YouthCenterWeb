using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityController(IGenericService<Activity, ActivityDto, CreateActivityDto> genericService) : ControllerBase
    {
        private readonly IGenericService<Activity, ActivityDto, CreateActivityDto> service = genericService;



        [HttpGet]
        public async Task<IActionResult> GetActivities()
        {

            var activities = await service.GetAllAsync(

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateActivity(CreateActivityDto dto)
        {
            var created = await service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetActivities), new { id = created.Id }, new BaseResponse<ActivityDto>
            {
                Result = 1,
                Data = created,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.CreatedAr,
                    MessageEn = Messages.Data.CreatedEn
                }
            });

        }
    }
}