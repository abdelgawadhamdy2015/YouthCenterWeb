using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CupsController(ICupService cupService) : ControllerBase
{
    private readonly ICupService _cupService = cupService;

    #region Cup CRUD

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        IEnumerable<CupDto> data = await _cupService.GetAllAsync();
        return ApiResult(data);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var data = await _cupService.GetByIdWithDetailsAsync(id);
        return ApiResult(data);
    }

    [HttpGet("status/{status}")]
    public async Task<IActionResult> GetByStatus(CupStatus status)
    {
        var data = await _cupService.GetByStatusAsync(status);
        return ApiResult(data);
    }

    [HttpGet("activity/{activityId:int}")]
    public async Task<IActionResult> GetByActivity(int activityId)
    {
        var data = await _cupService.GetByActivityAsync(activityId);
        return ApiResult(data);
    }

    [HttpGet("youth-center/{youthCenterId:int}")]
    public async Task<IActionResult> GetByYouthCenter(int youthCenterId)
    {
        var data = await _cupService.GetByYouthCenterAsync(youthCenterId);
        return ApiResult(data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateCupDto dto)
    {
        var adminId = GetCurrentUserId();

        var result = await _cupService.CreateAsync(
            dto,
            adminId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Id },
            result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCupDto dto)
    {
        var data = await _cupService.UpdateAsync(id, dto);

        return ApiResult(data);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _cupService.DeleteAsync(id);

        return NoContent();
    }

    #endregion

    #region Join Requests

    [Authorize]
    [HttpPost("{cupId:int}/join")]
    public async Task<IActionResult> JoinCup(
        int cupId,
        [FromBody] JoinCupRequestDto dto)
    {
        var userId = GetCurrentUserId();

        var data =
            await _cupService.SubmitJoinRequestAsync(
                cupId,
                userId,
                dto.TeamName);

        return ApiResult(data);

    }

    [Authorize(Roles = "Admin")]
    [HttpGet("{cupId:int}/requests")]
    public async Task<IActionResult> GetPendingRequests(
        int cupId)
    {
        var data =
            await _cupService.GetPendingRequestsAsync(cupId);

        return ApiResult(data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("teams/{teamId:int}/accept")]
    public async Task<IActionResult> AcceptRequest(
        int teamId)
    {
        var adminId = GetCurrentUserId();

        await _cupService.AcceptJoinRequestAsync(
            teamId,
            adminId);

        return Ok();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("teams/{teamId:int}/reject")]
    public async Task<IActionResult> RejectRequest(
        int teamId)
    {
        var adminId = GetCurrentUserId();

        await _cupService.RejectJoinRequestAsync(
            teamId,
            adminId);

        return Ok();
    }

    [HttpGet("{cupId:int}/teams")]
    public async Task<IActionResult> GetTeams(
        int cupId)
    {
        var data =
            await _cupService.GetAllTeamsAsync(cupId);

        return ApiResult(data);
    }

    #endregion

    #region Groups

    [HttpGet("{cupId:int}/groups")]
    public async Task<IActionResult> GetGroups(
        int cupId)
    {
        var data =
            await _cupService.GetGroupsAsync(cupId);

        return ApiResult(data);
    }

    [HttpGet("groups/{groupId:int}/standings")]
    public async Task<IActionResult> GetStandings(
        int groupId)
    {
        var data =
            await _cupService.GetGroupStandingsAsync(groupId);

        return ApiResult(data);
    }

    #endregion

    #region Matches

    [HttpGet("{cupId:int}/bracket")]
    public async Task<IActionResult> GetBracket(
        int cupId)
    {
        var data =
            await _cupService.GetBracketAsync(cupId);

        return ApiResult(data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("matches/{matchId:int}/result")]
    public async Task<IActionResult> EnterResult(
        int matchId,
        [FromBody] EnterMatchResultDto dto)
    {
        var adminId = GetCurrentUserId();

        var data =
            await _cupService.EnterMatchResultAsync(
                matchId,
                dto,
                adminId);

        return ApiResult(data);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("matches/{matchId:int}/schedule")]
    public async Task<IActionResult> ScheduleMatch(
        int matchId,
        [FromBody] ScheduleMatchDto dto)
    {
        var adminId = GetCurrentUserId();

        var data =
            await _cupService.ScheduleMatchAsync(
                matchId,
                dto.ScheduledAt,
                adminId);

        return ApiResult(data);
    }

    #endregion

    private int GetCurrentUserId()
    {
        return
            User.GetUserId();

    }


    private IActionResult ApiResult<T>(T data, string messageAr = "", string messageEn = "")
    {
        return Ok(new BaseResponse<T>
        {
            Result = data == null ? 0 : 1,
            Data = data,
            Alert = new Alert
            {
                MessageAr = string.IsNullOrEmpty(messageAr)
                    ? (data == null ? Messages.Data.NoDataAr : Messages.Data.FoundAr)
                    : messageAr,

                MessageEn = string.IsNullOrEmpty(messageEn)
                    ? (data == null ? Messages.Data.NoDataEn : Messages.Data.FoundEn)
                    : messageEn
            }
        });
    }
}