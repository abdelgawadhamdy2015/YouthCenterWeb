using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class YouthCenterController : ControllerBase
    {
        private readonly IYouthCenterService _service;

        public YouthCenterController(IYouthCenterService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<List<YouthCenterDto>>> GetAll()
        {
            var centers = await _service.GetAllAsync();
            return Ok(new BaseResponse<List<YouthCenterDto>>
            {
                Result = 1,
                Data = centers,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<YouthCenterDto>> GetById(int id)
        {
            var center = await _service.GetByIdAsync(id);
            if (center == null) return NotFound();
            return Ok(new BaseResponse<YouthCenterDto>
            {
                Result = 1,
                Data = center,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<YouthCenterDto>> Create(CreateYouthCenterDto dto)
        {
            var created = await _service.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}