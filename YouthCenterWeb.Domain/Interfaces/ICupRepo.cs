using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

public interface ICupRepo
{
    // ─── Cup CRUD ─────────────────────────────────────────────────────────────

    Task<Cup?> GetByIdAsync(int id);

    /// <summary>
    /// Full detail: includes Groups, Teams, Matches, Activity, YouthCenter.
    /// Used for the detail screen and bracket view.
    /// </summary>
    Task<Cup?> GetByIdWithDetailsAsync(int id);

    Task<IEnumerable<Cup>> GetAllAsync();

    Task<IEnumerable<Cup>> GetByStatusAsync(CupStatus status);

    Task<IEnumerable<Cup>> GetByYouthCenterAsync(int youthCenterId);

    Task<IEnumerable<Cup>> GetByActivityAsync(int activityId);

    Task<Cup> CreateAsync(Cup cup);

    Task<Cup> UpdateAsync(Cup cup);

    Task DeleteAsync(int id);

    // ─── CupGroup ─────────────────────────────────────────────────────────────

    Task<CupGroup?> GetGroupByIdAsync(int groupId);

    /// <summary>All groups for a cup, including their teams and matches.</summary>
    Task<IEnumerable<CupGroup>> GetGroupsByCupAsync(int cupId);

    Task CreateGroupsAsync(IEnumerable<CupGroup> groups);

    // ─── CupTeam — join requests ──────────────────────────────────────────────

    Task<CupTeam?> GetTeamByIdAsync(int teamId);

    /// <summary>
    /// Checks if a user already has a Pending or Accepted registration
    /// for this cup. Used to prevent duplicate requests.
    /// </summary>
    Task<CupTeam?> GetTeamByCupAndUserAsync(int cupId, int userId);

    /// <summary>All teams for a cup regardless of status.</summary>
    Task<IEnumerable<CupTeam>> GetTeamsByCupAsync(int cupId);

    /// <summary>Only Pending teams — shown to admin in the requests list.</summary>
    Task<IEnumerable<CupTeam>> GetPendingTeamsByCupAsync(int cupId);

    /// <summary>Only Accepted teams — used for group draw and standings.</summary>
    Task<IEnumerable<CupTeam>> GetAcceptedTeamsByCupAsync(int cupId);

    Task<CupTeam> CreateTeamAsync(CupTeam team);

    Task<CupTeam> UpdateTeamAsync(CupTeam team);

    /// <summary>
    /// Accepts a join request:
    ///   - Sets CupTeam.Status = Accepted, ReviewedAt = now
    ///   - Increments Cup.CurrentTeamCount
    /// Returns updated Cup so service can check if full.
    /// </summary>
    Task<Cup> AcceptTeamAndIncrementCountAsync(int teamId);

    /// <summary>
    /// Rejects a join request:
    ///   - Sets CupTeam.Status = Rejected, ReviewedAt = now
    /// </summary>
    Task RejectTeamAsync(int teamId);

    /// <summary>
    /// Assigns a group to each accepted team after the random draw.
    /// Bulk update for performance.
    /// </summary>
    Task AssignGroupsToTeamsAsync(IEnumerable<(int TeamId, int GroupId)> assignments);

    /// <summary>
    /// Updates standings for a team after a match result is entered.
    /// Increments Points / Wins|Draws|Losses / Goals atomically.
    /// </summary>
    Task UpdateTeamStandingsAsync(
        int teamId,
        int pointsDelta,
        int winsDelta,
        int drawsDelta,
        int lossesDelta,
        int goalsForDelta,
        int goalsAgainstDelta);

    // ─── CupMatch ─────────────────────────────────────────────────────────────

    Task<CupMatch?> GetMatchByIdAsync(int matchId);

    /// <summary>All matches for a cup — used to build the full bracket.</summary>
    Task<IEnumerable<CupMatch>> GetMatchesByCupAsync(int cupId);

    /// <summary>All group-stage matches for a specific group.</summary>
    Task<IEnumerable<CupMatch>> GetMatchesByGroupAsync(int groupId);

    /// <summary>All knockout matches for a cup (GroupId is null).</summary>
    Task<IEnumerable<CupMatch>> GetKnockoutMatchesByCupAsync(int cupId);

    /// <summary>Matches for a specific stage (e.g. SemiFinal, Final).</summary>
    Task<IEnumerable<CupMatch>> GetMatchesByStageAsync(int cupId, CupMatchStage stage);

    /// <summary>
    /// Checks whether all group-stage matches for a cup are Completed.
    /// Used to trigger knockout generation.
    /// </summary>
    Task<bool> AllGroupMatchesCompletedAsync(int cupId);

    Task CreateMatchesAsync(IEnumerable<CupMatch> matches);

    Task<CupMatch> UpdateMatchAsync(CupMatch match);

    /// <summary>
    /// Enters a match result:
    ///   - Sets ScoreA, ScoreB, WinnerId, Status = Completed
    /// Caller (service) handles standings update and bracket advancement.
    /// </summary>
    Task<CupMatch> EnterMatchResultAsync(
        int matchId,
        int scoreA,
        int scoreB,
        int? penaltyScoreA = null,
        int? penaltyScoreB = null,
        int? winnerId = null);

    /// <summary>
    /// Advances the winner into the next knockout slot.
    /// Finds the next match by (Round+1, ceil(MatchNumber/2))
    /// and sets TeamAId or TeamBId depending on whether MatchNumber is odd/even.
    /// </summary>
    Task AdvanceWinnerToNextRoundAsync(int cupId, int currentRound, int currentMatchNumber, int winnerTeamId);
    Task AssignThirdPlaceParticipantAsync(int cupId, int teamId);

    /// <summary>
    /// Seeds knockout matches after group stage ends.
    /// Sets TeamAId/TeamBId based on group standings (A1 vs B2, B1 vs A2 etc.)
    /// </summary>
    Task SeedKnockoutMatchesAsync(int cupId);
}