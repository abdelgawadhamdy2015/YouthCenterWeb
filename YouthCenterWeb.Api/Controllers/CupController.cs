using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CupController(ICupService cupService) : ControllerBase
{
    private readonly ICupService service = cupService;

    [HttpPost]
    [Authorize(policy: Policies.RequireAdmin)]
    public async Task<IActionResult> CreateCup(CreateCupDto dto)
    {
        var created = await service.CreateAsync(dto);


        return CreatedAtAction(nameof(CreateCup), new { id = created.Id }, new BaseResponse<CupDto>
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAllCups()
    {
        var cups = await service.GetAllAsync();
        return Ok(new BaseResponse<List<CupDto>>
        {
            Result = 1,
            Data = cups,
            Alert = new Alert
            {
                MessageAr = Messages.Data.FoundAr,
                MessageEn = Messages.Data.FoundEn
            }
        });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetCupById(int id)
    {
        var cup = await service.GetByIdAsync(id);
        return Ok(new BaseResponse<CupDto>
        {
            Result = 1,
            Data = cup,
            Alert = new Alert
            {
                MessageAr = Messages.Data.FoundAr,
                MessageEn = Messages.Data.FoundEn
            }
        });
    }

    [HttpDelete]
    [Authorize(policy: Policies.RequireAdmin)]
    public async Task<IActionResult> DeleteCup(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }



}