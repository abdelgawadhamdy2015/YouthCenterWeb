// Api/Controllers/YouthCenterController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class YouthCenterController(IYouthCenterService service) : ControllerBase
    {
        // ── GET api/youthcenter ───────────────────────────────────────────────────
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var result = await service.GetAllAsync();

            return Ok(new BaseResponse<List<YouthCenterDto>>
            {
                Result = result.Count != 0 ? 1 : 0,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = result.Count != 0 ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
                    MessageEn = result.Count != 0 ? Messages.Data.FoundEn : Messages.Data.NoDataEn
                }
            });
        }

        // ── GET api/youthcenter/{id} ──────────────────────────────────────────────
        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await service.GetByIdAsync(id)
                ?? throw new NotFoundException("YouthCenter", id);

            return Ok(new BaseResponse<YouthCenterDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        // ── POST api/youthcenter ──────────────────────────────────────────────────
        [HttpPost]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        public async Task<IActionResult> Create(CreateYouthCenterDto dto)
        {
            var result = await service.CreateAsync(dto);

            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                new BaseResponse<YouthCenterDto>
                {
                    Result = 1,
                    Data = result,
                    Alert = new Alert
                    {
                        MessageAr = Messages.Data.CreatedAr,
                        MessageEn = Messages.Data.CreatedEn
                    }
                });
        }

        // ── PUT api/youthcenter/{id} ──────────────────────────────────────────────
        [HttpPut("{id:int}")]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        public async Task<IActionResult> Update(int id, UpdatedYouthCenterDto dto)
        {
            var result = await service.UpdateAsync(id, dto);

            return Ok(new BaseResponse<YouthCenterDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.UpdatedAr,
                    MessageEn = Messages.Data.UpdatedEn
                }
            });
        }

        // ── DELETE api/youthcenter/{id} ───────────────────────────────────────────
        [HttpDelete("{id:int}")]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(id);
            return NoContent();
        }
    }
}