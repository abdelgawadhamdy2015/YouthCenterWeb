using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class YouthCenterActivityController(IYouthCenterActivityService service) : ControllerBase
    {
        private readonly IYouthCenterActivityService youthCenterActivityService = service;

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await youthCenterActivityService.GetAllAsync(y => y.Activity, y => y.YouthCenter);
            var validResult = result != null && result.Count > 0;
            return Ok(new BaseResponse<List<YouthCenterActivityDto>>
            {
                Result = validResult ? 1 : 0,
                Data = result,
                DataCount = result?.Count ?? 0,
                Alert = new Alert
                {
                    MessageEn = validResult ? Messages.Data.FoundEn : Messages.Data.NoDataEn,
                    MessageAr = validResult ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {

            var result = await youthCenterActivityService.GetByIdAsync(id);
            if (result == null)
                return NotFound(
                    new BaseResponse<YouthCenterActivityDto>
                    {
                        Result = 0,
                        Data = null,
                        Alert = new Alert
                        {
                            MessageEn = Messages.Data.NoDataEn,
                            MessageAr = Messages.Data.NoDataAr,
                        }
                    }
                );

            return Ok(new BaseResponse<YouthCenterActivityDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.FoundEn,
                    MessageAr = Messages.Data.FoundAr,
                }
            });
        }


        [HttpGet("activity/{ActivityId}")]
        public async Task<IActionResult> GetByActivityId(int ActivityId)
        {

            var result = await youthCenterActivityService.GetYouthCenterActivitiesByActivityIdAsync(ActivityId);
            if (result == null)
                return NotFound(
                    new BaseResponse<YouthCenterActivityDto>
                    {
                        Result = 0,
                        Data = null,
                        Alert = new Alert
                        {
                            MessageEn = Messages.Data.NoDataEn,
                            MessageAr = Messages.Data.NoDataAr,
                        }
                    }
                );

            return Ok(new BaseResponse<List<YouthCenterActivityDto>>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.FoundEn,
                    MessageAr = Messages.Data.FoundAr,
                }
            });
        }

        [HttpGet("youthCenter/{YouthCenterId}")]
        public async Task<IActionResult> GetByYouthCenterId(int YouthCenterId)
        {

            var result = await youthCenterActivityService.GetYouthCenterActivitiesByYouthCenterIdAsync(YouthCenterId);
            if (result == null)
                return NotFound(
                    new BaseResponse<YouthCenterActivityDto>
                    {
                        Result = 0,
                        Data = null,
                        Alert = new Alert
                        {
                            MessageEn = Messages.Data.NoDataEn,
                            MessageAr = Messages.Data.NoDataAr,
                        }
                    }
                );

            return Ok(new BaseResponse<List<YouthCenterActivityDto>>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.FoundEn,
                    MessageAr = Messages.Data.FoundAr,
                }
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(CreateYouthCenterActivityDto dto)
        {
            var result = await youthCenterActivityService.CreateAsync(dto);
            if (result == null)
                return BadRequest(new BaseResponse<YouthCenterActivityDto>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageEn = Messages.Data.CreationFailedEn,
                        MessageAr = Messages.Data.CreationFailedAr,
                    }
                });
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, new BaseResponse<YouthCenterActivityDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.CreatedEn,
                    MessageAr = Messages.Data.CreatedAr,
                }
            });
        }

    }
}