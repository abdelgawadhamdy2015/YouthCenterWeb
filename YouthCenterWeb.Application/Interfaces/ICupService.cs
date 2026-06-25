

using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

public interface ICupService
{
    // ─── Cup CRUD ─────────────────────────────────────────────────────────────

    Task<CupDto> GetByIdAsync(int id);
    Task<CupDetailDto> GetByIdWithDetailsAsync(int id);
    Task<IEnumerable<CupDto>> GetAllAsync();
    Task<IEnumerable<CupDto>> GetByStatusAsync(CupStatus status);
    Task<IEnumerable<CupDto>> GetByYouthCenterAsync(int youthCenterId);
    Task<IEnumerable<CupDto>> GetByActivityAsync(int activityId);
    Task<CupDto> CreateAsync(CreateCupDto dto, int adminId);
    Task<CupDto> UpdateAsync(int cupId, UpdateCupDto dto);
    Task DeleteAsync(int cupId);

    // ─── Join Requests ────────────────────────────────────────────────────────

    /// <summary>
    /// User submits a join request for a cup.
    /// Validates: cup is RegistrationOpen, user has no active request,
    /// cup is not already full.
    /// </summary>
    Task<CupTeamDto> SubmitJoinRequestAsync(int cupId, int userId, string teamName);

    /// <summary>
    /// Returns all pending join requests for a cup.
    /// Admin only.
    /// </summary>
    Task<IEnumerable<CupTeamDto>> GetPendingRequestsAsync(int cupId);

    /// <summary>
    /// Returns all teams for a cup (all statuses).
    /// </summary>
    Task<IEnumerable<CupTeamDto>> GetAllTeamsAsync(int cupId);

    /// <summary>
    /// Admin accepts a join request.
    /// - Sets team status to Accepted
    /// - Increments Cup.CurrentTeamCount
    /// - If cup is now full: triggers group draw + group match generation
    /// </summary>
    Task AcceptJoinRequestAsync(int teamId, int adminId);

    /// <summary>
    /// Admin rejects a join request.
    /// </summary>
    Task RejectJoinRequestAsync(int teamId, int adminId);

    // ─── Group Stage ──────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all groups with their teams and matches.
    /// </summary>
    Task<IEnumerable<CupGroupDto>> GetGroupsAsync(int cupId);

    /// <summary>
    /// Returns standings for all teams inside a specific group,
    /// sorted by Points desc, then GoalDifference desc.
    /// </summary>
    Task<IEnumerable<CupStandingDto>> GetGroupStandingsAsync(int groupId);

    // ─── Match Management ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns full bracket: all group matches + all knockout matches.
    /// </summary>
    Task<CupBracketDto> GetBracketAsync(int cupId);

    /// <summary>
    /// Admin enters a match result.
    /// - Validates winnerId is teamA or teamB
    /// - Updates match (ScoreA, ScoreB, WinnerId, Status = Completed)
    /// - Updates standings for both teams (group stage only)
    /// - If group stage: checks if all group matches done → triggers knockout seeding
    /// - If knockout: advances winner to next bracket slot
    /// </summary>
    Task<CupMatchDto> EnterMatchResultAsync(int matchId, EnterMatchResultDto dto, int adminId);

    /// <summary>
    /// Updates the scheduled date/time of a match.
    /// Admin only.
    /// </summary>
    Task<CupMatchDto> ScheduleMatchAsync(int matchId, DateTime scheduledAt, int adminId);
}