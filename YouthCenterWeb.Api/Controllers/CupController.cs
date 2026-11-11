using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Api.Controllers;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CupsController(ICupService cupService) : ApiControllerBase
{
    private readonly ICupService _cupService = cupService;

    #region Cup CRUD

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (GetCurrentUserRole() == UserRole.Admin)
        {
            var centerId = GetCurrentYouthCenter()
                ?? throw new BusinessException(
                    messageAr: "لا يمكن تحديد مركز الشباب الخاص بك.",
                    messageEn: "Your youth center could not be determined.");

            return await GetByYouthCenter(centerId);
        }

        var data = await _cupService.GetAllAsync();
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

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCupDto dto)
    {
        var (userId, role, youthCenterId) = GetAdminContext();
        var result = await _cupService.CreateAsync(dto, userId, role, youthCenterId);
        return CreatedResult(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateCupDto dto)
    {
        var (_, role, youthCenterId) = GetAdminContext();
        var data = await _cupService.UpdateAsync(id, dto, role, youthCenterId);
        return ApiResult(data);
    }

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var (_, role, youthCenterId) = GetAdminContext();
        await _cupService.DeleteAsync(id, role, youthCenterId);
        return Deleted();
    }

    #endregion

    #region Teams (admin)

    public record AddTeamDto(string TeamName, int? CaptainUserId);

    /// الأدمن يضيف فريق، والكابتن اختياري (يتربط بالـ id بعد البحث بالـ username)
    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPost("{cupId:int}/teams")]
    public async Task<IActionResult> AddTeam(int cupId, [FromBody] AddTeamDto dto)
    {
        var (_, role, centerId) = GetAdminContext();
        var data = await _cupService.AddTeamByAdminAsync(
            cupId, dto.TeamName, dto.CaptainUserId, role, centerId);
        return ApiResult(data);
    }

    /// بحث عن لاعب بالـ username/email لربطه ككابتن (3 حروف على الأقل)
    [Authorize(policy: Policies.RequireAdmin)]
    [HttpGet("captains/search")]
    public async Task<IActionResult> SearchCaptains([FromQuery] string email)
    {
        var data = await _cupService.SearchCaptainsAsync(email);
        return ApiResult(data);
    }

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpDelete("{cupId:int}/teams/{teamId:int}")]
    public async Task<IActionResult> RemoveTeam(int cupId, int teamId)
    {
        var (_, role, centerId) = GetAdminContext();
        await _cupService.RemoveTeamAsync(cupId, teamId, role, centerId);
        return Ok(ApiResultRaw("تم حذف الفريق.", "Team removed."));
    }

    [HttpGet("{cupId:int}/teams")]
    public async Task<IActionResult> GetTeams(int cupId)
    {
        var data = await _cupService.GetAllTeamsAsync(cupId);
        return ApiResult(data);
    }

    #endregion

    #region Join Requests

    [HttpPost("{cupId:int}/join")]
    public async Task<IActionResult> JoinCup(int cupId, [FromBody] JoinCupRequestDto dto)
    {
        var userId = GetCurrentUserId();
        var data = await _cupService.SubmitJoinRequestAsync(cupId, userId, dto.TeamName);
        return ApiResult(data);
    }

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpGet("{cupId:int}/requests")]
    public async Task<IActionResult> GetPendingRequests(int cupId)
    {
        var role = GetCurrentUserRole();
        var centerId = GetCurrentYouthCenter();
        var data = await _cupService.GetPendingRequestsAsync(cupId, role, centerId ?? 0);
        return ApiResult(data);
    }

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPost("teams/{teamId:int}/accept")]
    public async Task<IActionResult> AcceptRequest(int teamId)
    {
        var (_, role, youthCenterId) = GetAdminContext();
        await _cupService.AcceptJoinRequestAsync(teamId, role, youthCenterId);

        return Ok(ApiResultRaw(
            messageAr: "تم قبول طلب الانضمام بنجاح.",
            messageEn: "Join request accepted successfully."));
    }

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPost("teams/{teamId:int}/reject")]
    public async Task<IActionResult> RejectRequest(int teamId)
    {
        var (_, role, youthCenterId) = GetAdminContext();
        await _cupService.RejectJoinRequestAsync(teamId, role, youthCenterId);

        return Ok(ApiResultRaw(
            messageAr: "تم رفض طلب الانضمام.",
            messageEn: "Join request rejected."));
    }

    #endregion

    #region Groups

    [HttpGet("{cupId:int}/groups")]
    public async Task<IActionResult> GetGroups(int cupId)
    {
        var data = await _cupService.GetGroupsAsync(cupId);
        return ApiResult(data);
    }

    [HttpGet("groups/{groupId:int}/standings")]
    public async Task<IActionResult> GetStandings(int groupId)
    {
        var data = await _cupService.GetGroupStandingsAsync(groupId);
        return ApiResult(data);
    }

    #endregion

    #region Matches & Schedule

    [HttpGet("{cupId:int}/bracket")]
    public async Task<IActionResult> GetBracket(int cupId)
    {
        var data = await _cupService.GetBracketAsync(cupId);
        return ApiResult(data);
    }

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPost("matches/{matchId:int}/result")]
    public async Task<IActionResult> EnterResult(int matchId, [FromBody] EnterMatchResultDto dto)
    {
        var (_, role, youthCenterId) = GetAdminContext();
        var data = await _cupService.EnterMatchResultAsync(matchId, dto, role, youthCenterId);
        return ApiResult(data);
    }

    /// تعديل ميعاد مباراة يدوي (بيتعلّم IsManuallyScheduled)
    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPut("matches/{matchId:int}/schedule")]
    public async Task<IActionResult> ScheduleMatch(int matchId, [FromBody] ScheduleMatchDto dto)
    {
        var (_, role, youthCenterId) = GetAdminContext();
        var data = await _cupService.ScheduleMatchAsync(matchId, dto.ScheduledAt, role, youthCenterId);
        return ApiResult(data);
    }

    /// إعادة توليد الجدول تلقائياً
    /// OverwriteManual = false → يحافظ على المباريات اللي الأدمن عدّلها يدوي
    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPost("{cupId:int}/schedule/regenerate")]
    public async Task<IActionResult> RegenerateSchedule(
        int cupId, [FromBody] RegenerateScheduleDto dto)
    {
        var (_, role, centerId) = GetAdminContext();
        var count = await _cupService.RegenerateScheduleAsync(
            cupId, dto.OverwriteManual, role, centerId);

        return Ok(ApiResultRaw(
            messageAr: $"تم جدولة {count} مباراة.",
            messageEn: $"{count} matches scheduled."));
    }

    #endregion

    #region Start Cup

    [Authorize(policy: Policies.RequireAdmin)]
    [HttpPost("{cupId:int}/start")]
    public async Task<IActionResult> Start(int cupId)
    {
        var (_, role, centerId) = GetAdminContext();
        await _cupService.StartCupAsync(cupId, role, centerId);
        return Ok(ApiResultRaw("تم بدء البطولة.", "Cup started."));
    }

    #endregion

    // ─────────────────────────────────────────────────────────────────────
    // PRIVATE HELPERS
    // ─────────────────────────────────────────────────────────────────────

    private int GetCurrentUserId() => User.GetUserId();

    private UserRole GetCurrentUserRole() => User.GetRole();

    private int? GetCurrentYouthCenter() => User.GetYouthCenterId();

    private (int userId, UserRole role, int youthCenterId) GetAdminContext()
    {
        var youthCenterId = GetCurrentYouthCenter()
            ?? throw new BusinessException(
                messageAr: "لا يمكن تحديد مركز الشباب الخاص بك. تأكد من صلاحيات حسابك.",
                messageEn: "Your youth center could not be determined. Please verify your account permissions.");

        return (GetCurrentUserId(), GetCurrentUserRole(), youthCenterId);
    }

    private OkObjectResult ApiResult<T>(T data, string messageAr = "", string messageEn = "")
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

    private static BaseResponse<object> ApiResultRaw(string messageAr, string messageEn) =>
        new()
        {
            Result = 1,
            Data = null,
            Alert = new Alert { MessageAr = messageAr, MessageEn = messageEn }
        };
}