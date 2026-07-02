using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class CupRepo(AppDbContext context) : ICupRepo
{
    private readonly AppDbContext _context = context;

    // ─── Cup CRUD ─────────────────────────────────────────────────────────────
    private IQueryable<Cup> WithRelation() => _context.Cups
            .Include(c => c.YouthCenterActivity)
            .ThenInclude(y => y.Activity).Include(c => c.YouthCenterActivity).ThenInclude(y => y.YouthCenter)
            .Include(c => c.CreatedByAdmin);

    public async Task<Cup?> GetByIdAsync(int id)
    {
        return await _context.Cups
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Cup?> GetByIdWithDetailsAsync(int id)
    {
        return await _context.Cups
            .Include(c => c.YouthCenterActivity)
            .Include(c => c.CreatedByAdmin)
            .Include(c => c.Groups)
                .ThenInclude(g => g.Teams)
            .Include(c => c.Groups)
                .ThenInclude(g => g.Matches)
                    .ThenInclude(m => m.TeamA)
            .Include(c => c.Groups)
                .ThenInclude(g => g.Matches)
                    .ThenInclude(m => m.TeamB)
            .Include(c => c.Matches.Where(m => m.GroupId == null)) // knockout only
                .ThenInclude(m => m.TeamA)
            .Include(c => c.Matches.Where(m => m.GroupId == null))
                .ThenInclude(m => m.TeamB)
            .Include(c => c.Matches.Where(m => m.GroupId == null))
                .ThenInclude(m => m.Winner)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<IEnumerable<Cup>> GetAllAsync()
    {
        return await WithRelation()
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cup>> GetByStatusAsync(CupStatus status)
    {
        return await WithRelation()
            .Where(c => c.Status == status)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cup>> GetByYouthCenterAsync(int youthCenterId)
    {
        return await WithRelation()
            .Where(c => c.YouthCenterActivity.YouthCenterId == youthCenterId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Cup>> GetByActivityAsync(int activityId)
    {
        return await WithRelation()
            .Where(c => c.YouthCenterActivity.ActivityId == activityId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<Cup> CreateAsync(Cup cup)
    {
        await _context.Cups.AddAsync(cup);
        await _context.SaveChangesAsync();
        return cup;
    }

    public async Task<Cup> UpdateAsync(Cup cup)
    {
        _context.Cups.Update(cup);
        await _context.SaveChangesAsync();
        return cup;
    }

    public async Task DeleteAsync(int id)
    {
        var cup = await GetByIdAsync(id);
        if (cup is null) return;

        _context.Cups.Remove(cup);
        await _context.SaveChangesAsync();
    }

    // ─── CupGroup ─────────────────────────────────────────────────────────────

    public async Task<CupGroup?> GetGroupByIdAsync(int groupId)
    {
        return await _context.CupGroups
            .Include(g => g.Teams)
            .Include(g => g.Matches)
                .ThenInclude(m => m.TeamA)
            .Include(g => g.Matches)
                .ThenInclude(m => m.TeamB)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    public async Task<IEnumerable<CupGroup>> GetGroupsByCupAsync(int cupId)
    {
        return await _context.CupGroups
            .Include(g => g.Teams)
            .Include(g => g.Matches)
                .ThenInclude(m => m.TeamA)
            .Include(g => g.Matches)
                .ThenInclude(m => m.TeamB)
            .Where(g => g.CupId == cupId)
            .OrderBy(g => g.GroupNumber)
            .ToListAsync();
    }

    public async Task CreateGroupsAsync(IEnumerable<CupGroup> groups)
    {
        await _context.CupGroups.AddRangeAsync(groups);
        await _context.SaveChangesAsync();
    }

    // ─── CupTeam ──────────────────────────────────────────────────────────────

    public async Task<CupTeam?> GetTeamByIdAsync(int teamId)
    {
        return await _context.CupTeams
            .Include(t => t.CaptainUser)
            .Include(t => t.Group)
            .FirstOrDefaultAsync(t => t.Id == teamId);
    }

    public async Task<CupTeam?> GetTeamByCupAndUserAsync(int cupId, int userId)
    {
        return await _context.CupTeams
            .FirstOrDefaultAsync(t =>
                t.CupId == cupId &&
                t.CaptainUserId == userId &&
                t.Status != CupTeamStatus.Rejected);
    }

    public async Task<IEnumerable<CupTeam>> GetTeamsByCupAsync(int cupId)
    {
        return await _context.CupTeams
            .Include(t => t.CaptainUser)
            .Include(t => t.Group)
            .Where(t => t.CupId == cupId)
            .OrderByDescending(t => t.RegisteredAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CupTeam>> GetPendingTeamsByCupAsync(int cupId)
    {
        return await _context.CupTeams
            .Include(t => t.CaptainUser)
            .Where(t => t.CupId == cupId && t.Status == CupTeamStatus.Pending)
            .OrderBy(t => t.RegisteredAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<CupTeam>> GetAcceptedTeamsByCupAsync(int cupId)
    {
        return await _context.CupTeams
            .Where(t => t.CupId == cupId && t.Status == CupTeamStatus.Accepted)
            .OrderBy(t => t.RegisteredAt)
            .ToListAsync();
    }

    public async Task<CupTeam> CreateTeamAsync(CupTeam team)
    {
        await _context.CupTeams.AddAsync(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task<CupTeam> UpdateTeamAsync(CupTeam team)
    {
        _context.CupTeams.Update(team);
        await _context.SaveChangesAsync();
        return team;
    }

    public async Task<Cup> AcceptTeamAndIncrementCountAsync(int teamId)
    {
        var team = await _context.CupTeams
            .Include(t => t.Cup)
            .FirstOrDefaultAsync(t => t.Id == teamId)
            ?? throw new KeyNotFoundException($"CupTeam {teamId} not found");

        team.Status = CupTeamStatus.Accepted;
        team.ReviewedAt = DateTime.UtcNow;
        team.Cup.CurrentTeamCount++;

        await _context.SaveChangesAsync();
        return team.Cup;
    }

    public async Task RejectTeamAsync(int teamId)
    {
        var team = await _context.CupTeams
            .FirstOrDefaultAsync(t => t.Id == teamId)
            ?? throw new KeyNotFoundException($"CupTeam {teamId} not found");

        team.Status = CupTeamStatus.Rejected;
        team.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task AssignGroupsToTeamsAsync(IEnumerable<(int TeamId, int GroupId)> assignments)
    {
        var teamIds = assignments.Select(a => a.TeamId).ToList();
        var teams = await _context.CupTeams
            .Where(t => teamIds.Contains(t.Id))
            .ToListAsync();

        var map = assignments.ToDictionary(a => a.TeamId, a => a.GroupId);
        foreach (var team in teams)
        {
            if (map.TryGetValue(team.Id, out var groupId))
                team.GroupId = groupId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task UpdateTeamStandingsAsync(
        int teamId,
        int pointsDelta,
        int winsDelta,
        int drawsDelta,
        int lossesDelta,
        int goalsForDelta,
        int goalsAgainstDelta)
    {
        var team = await _context.CupTeams
            .FirstOrDefaultAsync(t => t.Id == teamId)
            ?? throw new KeyNotFoundException($"CupTeam {teamId} not found");

        team.Points += pointsDelta;
        team.Wins += winsDelta;
        team.Draws += drawsDelta;
        team.Losses += lossesDelta;
        team.GoalsFor += goalsForDelta;
        team.GoalsAgainst += goalsAgainstDelta;

        await _context.SaveChangesAsync();
    }

    // ─── CupMatch ─────────────────────────────────────────────────────────────

    public async Task<CupMatch?> GetMatchByIdAsync(int matchId)
    {
        return await _context.CupMatches
            .Include(m => m.TeamA)
            .Include(m => m.TeamB)
            .Include(m => m.Winner)
            .Include(m => m.Group)
            .FirstOrDefaultAsync(m => m.Id == matchId);
    }

    public async Task<IEnumerable<CupMatch>> GetMatchesByCupAsync(int cupId)
    {
        return await _context.CupMatches
            .Include(m => m.TeamA)
            .Include(m => m.TeamB)
            .Include(m => m.Winner)
            .Include(m => m.Group)
            .Where(m => m.CupId == cupId)
            .OrderBy(m => m.Stage)
            .ThenBy(m => m.Round)
            .ThenBy(m => m.MatchNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<CupMatch>> GetMatchesByGroupAsync(int groupId)
    {
        return await _context.CupMatches
            .Include(m => m.TeamA)
            .Include(m => m.TeamB)
            .Include(m => m.Winner)
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.MatchNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<CupMatch>> GetKnockoutMatchesByCupAsync(int cupId)
    {
        return await _context.CupMatches
            .Include(m => m.TeamA)
            .Include(m => m.TeamB)
            .Include(m => m.Winner)
            .Where(m => m.CupId == cupId && m.GroupId == null)
            .OrderBy(m => m.Stage)
            .ThenBy(m => m.MatchNumber)
            .ToListAsync();
    }

    public async Task<IEnumerable<CupMatch>> GetMatchesByStageAsync(int cupId, CupMatchStage stage)
    {
        return await _context.CupMatches
            .Include(m => m.TeamA)
            .Include(m => m.TeamB)
            .Include(m => m.Winner)
            .Where(m => m.CupId == cupId && m.Stage == stage)
            .OrderBy(m => m.MatchNumber)
            .ToListAsync();
    }

    public async Task<bool> AllGroupMatchesCompletedAsync(int cupId)
    {
        return !await _context.CupMatches
            .AnyAsync(m =>
                m.CupId == cupId &&
                m.Stage == CupMatchStage.GroupStage &&
                m.Status != CupMatchStatus.Completed);
    }

    public async Task CreateMatchesAsync(IEnumerable<CupMatch> matches)
    {
        await _context.CupMatches.AddRangeAsync(matches);
        await _context.SaveChangesAsync();
    }

    public async Task<CupMatch> UpdateMatchAsync(CupMatch match)
    {
        _context.CupMatches.Update(match);
        await _context.SaveChangesAsync();
        return match;
    }

    public async Task<CupMatch> EnterMatchResultAsync(
     int matchId,
     int scoreA,
     int scoreB,
     int? penaltyScoreA,
     int? penaltyScoreB,
     int? winnerId)
    {
        var match = await _context.CupMatches
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new KeyNotFoundException();

        match.ScoreA = scoreA;
        match.ScoreB = scoreB;
        match.WinnerId = winnerId;
        match.PenaltyScoreA = penaltyScoreA;
        match.PenaltyScoreB = penaltyScoreB;
        match.Status = CupMatchStatus.Completed;

        await _context.SaveChangesAsync();

        return match;
    }

    public async Task AdvanceWinnerToNextRoundAsync(
        int cupId,
        int currentRound,
        int currentMatchNumber,
        int winnerTeamId)
    {
        var nextMatchNumber = (int)Math.Ceiling(currentMatchNumber / 2.0);
        var nextRound = currentRound + 1;

        var nextMatch = await _context.CupMatches
            .FirstOrDefaultAsync(m =>
                m.CupId == cupId &&
                m.Round == nextRound &&
                m.MatchNumber == nextMatchNumber &&
                m.GroupId == null);

        if (nextMatch is null) return; // final had no next match

        // Odd MatchNumber → TeamA slot, Even → TeamB slot
        if (currentMatchNumber % 2 != 0)
            nextMatch.TeamAId = winnerTeamId;
        else
            nextMatch.TeamBId = winnerTeamId;

        await _context.SaveChangesAsync();
    }
    public async Task SeedKnockoutMatchesAsync(int cupId)
    {
        var groups = await _context.CupGroups
            .Include(g => g.Teams)
            .Where(g => g.CupId == cupId)
            .OrderBy(g => g.GroupNumber)
            .ToListAsync();

        var cup = await _context.Cups.FirstOrDefaultAsync(c => c.Id == cupId)
            ?? throw new NotFoundException(
                messageAr: "لم يتم العثور على البطولة المطلوبة.",
                messageEn: "Cup not found.");

        var firstPlaces = groups
            .Select(g => g.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalsFor - t.GoalsAgainst)
                .First())
            .ToList();

        var secondPlaces = groups
            .Select(g => g.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalsFor - t.GoalsAgainst)
                .Skip(1)
                .First())
            .ToList();

        // Determine which stage is the FIRST knockout round for this bracket
        // size — must match the same logic used in CreateKnockoutBracket.
        int knockoutTeams = groups.Count * cup.TeamsAdvancingPerGroup;
        var firstRoundStage = knockoutTeams switch
        {
            2 => CupMatchStage.FinalMatch,
            4 => CupMatchStage.SemiFinal,
            8 => CupMatchStage.QuarterFinal,
            16 => CupMatchStage.RoundOf16,
            _ => CupMatchStage.RoundOf32
        };

        // Only pull the first-round placeholder slots — this excludes
        // ThirdPlace and FinalMatch, which must stay empty until winners
        // are advanced into them later.
        var firstRoundMatches = await _context.CupMatches
            .Where(m => m.CupId == cupId
                     && m.GroupId == null
                     && m.Stage == firstRoundStage)
            .OrderBy(m => m.MatchNumber)
            .ToListAsync();

        for (int i = 0; i < firstPlaces.Count && i * 2 + 1 < firstRoundMatches.Count; i++)
        {
            var matchA = firstRoundMatches[i * 2];
            var matchB = firstRoundMatches[i * 2 + 1];

            matchA.TeamAId = firstPlaces[i].Id;
            matchA.TeamBId = secondPlaces[(i + 1) % secondPlaces.Count].Id;

            matchB.TeamAId = firstPlaces[(i + 1) % firstPlaces.Count].Id;
            matchB.TeamBId = secondPlaces[i].Id;
        }

        await _context.SaveChangesAsync();
    }

    public async Task AssignThirdPlaceParticipantAsync(int cupId, int teamId)
    {
        var thirdPlaceMatch = await _context.CupMatches
            .FirstOrDefaultAsync(m =>
                m.CupId == cupId &&
                m.Stage == CupMatchStage.ThirdPlace);

        if (thirdPlaceMatch == null)
            throw new NotFoundException(
                messageAr: "لم يتم العثور على مباراة تحديد المركز الثالث لهذه البطولة.",
                messageEn: "Third-place match was not found for this cup.");

        if (thirdPlaceMatch.TeamAId == null)
            thirdPlaceMatch.TeamAId = teamId;
        else if (thirdPlaceMatch.TeamBId == null)
            thirdPlaceMatch.TeamBId = teamId;
        else
            throw new BusinessException(
                messageAr: "مباراة تحديد المركز الثالث ممتلئة بالفعل.",
                messageEn: "The third-place match is already full.");

        await _context.SaveChangesAsync();
    }
}