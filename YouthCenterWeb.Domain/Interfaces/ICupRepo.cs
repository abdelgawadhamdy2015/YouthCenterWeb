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


    Task<CupTeam?> GetTeamByCupAndUserAsync(int cupId, int userId);

    Task<IEnumerable<CupTeam>> GetTeamsByCupAsync(int cupId);

    Task<IEnumerable<CupTeam>> GetPendingTeamsByCupAsync(int cupId);

    Task<IEnumerable<CupTeam>> GetAcceptedTeamsByCupAsync(int cupId);

    Task<CupTeam> CreateTeamAsync(CupTeam team);

    Task<CupTeam> UpdateTeamAsync(CupTeam team);


    Task<Cup> AcceptTeamAndIncrementCountAsync(int teamId);


    Task RejectTeamAsync(int teamId);


    Task AssignGroupsToTeamsAsync(IEnumerable<(int TeamId, int GroupId)> assignments);
    Task UpdateTeamStandingsAsync(int teamId, int pointsDelta, int winsDelta, int drawsDelta,
       int lossesDelta, int goalsForDelta, int goalsAgainstDelta, int fairPlayDelta = 0);

    Task<CupMatch> EnterMatchResultAsync(int matchId, int scoreA, int scoreB,
        int? penaltyScoreA = null, int? penaltyScoreB = null, int? winnerId = null,
        int? yellowA = null, int? yellowB = null, int? redA = null, int? redB = null);

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


    Task AdvanceWinnerToNextRoundAsync(int cupId, int currentRound, int currentMatchNumber, int winnerTeamId);
    Task AssignThirdPlaceParticipantAsync(int cupId, int teamId);

    Task SeedKnockoutMatchesAsync(int cupId);
    Task<int?> GetCupIdByTeamIdAsync(int teamId);
    Task UpdateMatchesAsync(IEnumerable<CupMatch> matches);

    Task<Cup?> GetByIdForUpdateAsync(int cupId);
    Task DeleteTeamAsync(int teamId);
    Task ExecuteInTransactionAsync(Func<Task> operation);


}