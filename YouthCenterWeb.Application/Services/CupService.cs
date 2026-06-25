using AutoMapper;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class CupService(
        ICupRepo cupRepo,
        IMapper mapper) : ICupService
    {
        private readonly ICupRepo _cupRepo = cupRepo;
        private readonly IMapper _mapper = mapper;



        public async Task<CupDto> GetByIdAsync(int id)
        {
            var cup = await _cupRepo.GetByIdAsync(id)
                      ?? throw new KeyNotFoundException("Cup not found");

            return _mapper.Map<CupDto>(cup);
        }

        public async Task<CupDetailDto> GetByIdWithDetailsAsync(int id)
        {
            var cup = await _cupRepo.GetByIdWithDetailsAsync(id)
                      ?? throw new KeyNotFoundException("Cup not found");

            return _mapper.Map<CupDetailDto>(cup);
        }

        public async Task<IEnumerable<CupDto>> GetAllAsync()
        {
            var cups = await _cupRepo.GetAllAsync();

            return _mapper.Map<IEnumerable<CupDto>>(cups);
        }

        public async Task<IEnumerable<CupDto>> GetByStatusAsync(CupStatus status)
        {
            var cups = await _cupRepo.GetByStatusAsync(status);

            return _mapper.Map<IEnumerable<CupDto>>(cups);
        }

        public async Task<IEnumerable<CupDto>> GetByYouthCenterAsync(int youthCenterId)
        {
            var cups = await _cupRepo.GetByYouthCenterAsync(youthCenterId);

            return _mapper.Map<IEnumerable<CupDto>>(cups);
        }

        public async Task<IEnumerable<CupDto>> GetByActivityAsync(int activityId)
        {
            var cups = await _cupRepo.GetByActivityAsync(activityId);

            return _mapper.Map<IEnumerable<CupDto>>(cups);
        }


        public async Task<CupDto> CreateAsync(
    CreateCupDto dto,
    int adminId)
        {
            var cup = _mapper.Map<Cup>(dto);

            cup.CreatedByAdminId = adminId;
            cup.Status = CupStatus.Draft;
            cup.CurrentTeamCount = 0;
            cup.CreatedAt = DateTime.UtcNow;

            await _cupRepo.CreateAsync(cup);

            return _mapper.Map<CupDto>(cup);
        }

        public async Task<CupDto> UpdateAsync(
            int cupId,
            UpdateCupDto dto)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                      ?? throw new KeyNotFoundException();

            cup.NameAr = dto.NameAr;
            cup.NameEn = dto.NameEn;
            cup.StartDate = dto.StartDate;
            cup.Status = dto.Status;

            await _cupRepo.UpdateAsync(cup);

            return _mapper.Map<CupDto>(cup);
        }

        public async Task DeleteAsync(int cupId)
        {
            await _cupRepo.DeleteAsync(cupId);
        }

        public async Task<CupTeamDto> SubmitJoinRequestAsync(int cupId, int userId, string teamName)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                      ?? throw new KeyNotFoundException();

            if (cup.Status != CupStatus.RegistrationOpen)
                throw new Exception("Registration closed");

            if (cup.CurrentTeamCount >= cup.MaxTeams)
                throw new Exception("Cup is full");

            var existing =
                await _cupRepo.GetTeamByCupAndUserAsync(cupId, userId);

            if (existing != null)
                throw new Exception("Already registered");

            var team = new CupTeam
            {
                CupId = cupId,
                CaptainUserId = userId,
                TeamName = teamName,
                Status = CupTeamStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            await _cupRepo.CreateTeamAsync(team);

            return _mapper.Map<CupTeamDto>(team);
        }

        public async Task<IEnumerable<CupTeamDto>> GetPendingRequestsAsync(int cupId)
        {
            var teams =
                await _cupRepo.GetPendingTeamsByCupAsync(cupId);

            return _mapper.Map<IEnumerable<CupTeamDto>>(teams);
        }

        public async Task<IEnumerable<CupTeamDto>> GetAllTeamsAsync(int cupId)
        {
            var teams =
                await _cupRepo.GetTeamsByCupAsync(cupId);

            return _mapper.Map<IEnumerable<CupTeamDto>>(teams);
        }

        public async Task AcceptJoinRequestAsync(
    int teamId,
    int adminId)
        {
            var cup =
                await _cupRepo.AcceptTeamAndIncrementCountAsync(teamId);

            if (cup.CurrentTeamCount != cup.MaxTeams)
                return;

            await GenerateGroupsAsync(cup.Id);

            await GenerateGroupMatchesAsync(cup.Id);

            cup.Status = CupStatus.GroupStage;

            await _cupRepo.UpdateAsync(cup);
        }

        public async Task RejectJoinRequestAsync(
    int teamId,
    int adminId)
        {
            await _cupRepo.RejectTeamAsync(teamId);
        }

        public async Task<IEnumerable<CupGroupDto>>
    GetGroupsAsync(int cupId)
        {
            var groups =
                await _cupRepo.GetGroupsByCupAsync(cupId);

            return _mapper.Map<IEnumerable<CupGroupDto>>(groups);
        }

        public async Task<IEnumerable<CupStandingDto>>
    GetGroupStandingsAsync(int groupId)
        {
            var group =
                await _cupRepo.GetGroupByIdAsync(groupId)
                ?? throw new KeyNotFoundException();

            var standings = group.Teams
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalsFor - t.GoalsAgainst);

            return _mapper.Map<IEnumerable<CupStandingDto>>(standings);
        }

        public async Task<CupBracketDto> GetBracketAsync(int cupId)
        {
            var matches = await _cupRepo.GetMatchesByCupAsync(cupId);

            return new CupBracketDto
            {
                GroupMatches = matches
                    .Where(x => x.GroupId != null)
                    .Select(_mapper.Map<CupMatchDto>)
                    .ToList(),

                KnockoutMatches = matches
                    .Where(x => x.GroupId == null)
                    .Select(_mapper.Map<CupMatchDto>)
                    .ToList()
            };
        }

        public async Task<CupMatchDto> EnterMatchResultAsync(
     int matchId,
     EnterMatchResultDto dto,
     int adminId)
        {
            var match = await _cupRepo.GetMatchByIdAsync(matchId)
                ?? throw new KeyNotFoundException("Match not found");

            if (match.Status == CupMatchStatus.Completed)
                throw new Exception("Match already completed");

            int? winnerId = null;

            // Group Stage
            if (match.Stage == CupMatchStage.GroupStage)
            {
                if (dto.ScoreA > dto.ScoreB)
                    winnerId = match.TeamAId;
                else if (dto.ScoreB > dto.ScoreA)
                    winnerId = match.TeamBId;

                await _cupRepo.EnterMatchResultAsync(
                    matchId,
                    dto.ScoreA,
                    dto.ScoreB,
                    winnerId ?? 0);

                await UpdateStandings(match, dto);

                var allDone = await _cupRepo
                    .AllGroupMatchesCompletedAsync(match.CupId);

                if (allDone)
                {
                    await _cupRepo.SeedKnockoutMatchesAsync(match.CupId);

                    var cup = await _cupRepo.GetByIdAsync(match.CupId);

                    if (cup is not null)
                    {
                        cup.Status = CupStatus.KnockoutStage;
                        await _cupRepo.UpdateAsync(cup);
                    }
                }
            }
            // Knockout
            else
            {
                if (dto.ScoreA == dto.ScoreB)
                    throw new Exception(
                        "Knockout matches cannot end in a draw");

                winnerId = dto.ScoreA > dto.ScoreB
                    ? match.TeamAId
                    : match.TeamBId;

                await _cupRepo.EnterMatchResultAsync(
                    matchId,
                    dto.ScoreA,
                    dto.ScoreB,
                    winnerId!.Value);

                await _cupRepo.AdvanceWinnerToNextRoundAsync(
                    match.CupId,
                    match.Round,
                    match.MatchNumber,
                    winnerId.Value);

                // Check if final completed
                if (match.Stage == CupMatchStage.Final)
                {
                    var cup = await _cupRepo.GetByIdAsync(match.CupId);

                    if (cup is not null)
                    {
                        cup.Status = CupStatus.Completed;
                        await _cupRepo.UpdateAsync(cup);
                    }
                }
            }

            var updated = await _cupRepo.GetMatchByIdAsync(matchId);

            return _mapper.Map<CupMatchDto>(updated);
        }

        public async Task<CupMatchDto> ScheduleMatchAsync(
        int matchId,
        DateTime scheduledAt,
        int adminId)
        {
            var match =
                await _cupRepo.GetMatchByIdAsync(matchId)
                ?? throw new KeyNotFoundException();

            match.ScheduledAt = scheduledAt;

            await _cupRepo.UpdateMatchAsync(match);

            return _mapper.Map<CupMatchDto>(match);
        }

        private async Task UpdateStandings(CupMatch match, EnterMatchResultDto dto)
        {
            if (dto.ScoreA > dto.ScoreB)
            {
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value,
                    3, 1, 0, 0,
                    dto.ScoreA, dto.ScoreB);

                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value,
                    0, 0, 0, 1,
                    dto.ScoreB, dto.ScoreA);
            }
            else if (dto.ScoreB > dto.ScoreA)
            {
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value,
                    3, 1, 0, 0,
                    dto.ScoreB, dto.ScoreA);

                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value,
                    0, 0, 0, 1,
                    dto.ScoreA, dto.ScoreB);
            }
            else
            {
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value,
                    1, 0, 1, 0,
                    dto.ScoreA, dto.ScoreB);

                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value,
                    1, 0, 1, 0,
                    dto.ScoreB, dto.ScoreA);
            }
        }

        private async Task GenerateGroupsAsync(int cupId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                      ?? throw new KeyNotFoundException("Cup not found");

            var teams = (await _cupRepo.GetAcceptedTeamsByCupAsync(cupId))
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            if (teams.Count != cup.MaxTeams)
                throw new Exception("Cup is not full yet");

            var groups = Enumerable.Range(1, cup.Groups.Count)
                .Select(i => new CupGroup
                {
                    CupId = cupId,
                    GroupNumber = i
                })
                .ToList();

            await _cupRepo.CreateGroupsAsync(groups);

            var savedGroups =
                (await _cupRepo.GetGroupsByCupAsync(cupId))
                .OrderBy(g => g.GroupNumber)
                .ToList();

            var assignments = new List<(int TeamId, int GroupId)>();

            for (int i = 0; i < teams.Count; i++)
            {
                var group = savedGroups[i % cup.Groups.Count];

                assignments.Add(
                    (
                        teams[i].Id,
                        group.Id
                    ));
            }

            await _cupRepo.AssignGroupsToTeamsAsync(assignments);
        }

        private async Task GenerateGroupMatchesAsync(int cupId)
        {
            var groups =
                (await _cupRepo.GetGroupsByCupAsync(cupId))
                .ToList();

            var matches = new List<CupMatch>();

            int matchNumber = 1;

            foreach (var group in groups)
            {
                var teams = group.Teams.ToList();

                for (int i = 0; i < teams.Count; i++)
                {
                    for (int j = i + 1; j < teams.Count; j++)
                    {
                        matches.Add(new CupMatch
                        {
                            CupId = cupId,
                            GroupId = group.Id,

                            TeamAId = teams[i].Id,
                            TeamBId = teams[j].Id,

                            Stage = CupMatchStage.GroupStage,
                            Round = 1,

                            MatchNumber = matchNumber++,

                            Status = CupMatchStatus.Pending
                        });
                    }
                }
            }

            await CreateKnockoutBracket(
                cupId,
                groups.Count,
                matches);

            await _cupRepo.CreateMatchesAsync(matches);
        }

        private async Task CreateKnockoutBracket(
    int cupId,
    int groupCount,
    List<CupMatch> matches)
        {
            int knockoutTeams = groupCount * 2;

            int round = 2;

            int firstRoundMatches = knockoutTeams / 2;

            CupMatchStage firstStage;

            if (firstRoundMatches == 2)
                firstStage = CupMatchStage.SemiFinal;
            else if (firstRoundMatches == 4)
                firstStage = CupMatchStage.QuarterFinal;
            else
                firstStage = CupMatchStage.RoundOf16;

            for (int i = 1; i <= firstRoundMatches; i++)
            {
                matches.Add(new CupMatch
                {
                    CupId = cupId,

                    Stage = firstStage,
                    Round = round,

                    MatchNumber = i,

                    Status = CupMatchStatus.Pending
                });
            }

            int currentMatches = firstRoundMatches;
            int currentRound = round + 1;

            while (currentMatches > 1)
            {
                currentMatches /= 2;

                CupMatchStage stage =
                    currentMatches switch
                    {
                        1 => CupMatchStage.Final,
                        2 => CupMatchStage.SemiFinal,
                        4 => CupMatchStage.QuarterFinal,
                        _ => CupMatchStage.RoundOf16
                    };

                for (int i = 1; i <= currentMatches; i++)
                {
                    matches.Add(new CupMatch
                    {
                        CupId = cupId,

                        Stage = stage,
                        Round = currentRound,

                        MatchNumber = i,

                        Status = CupMatchStatus.Pending
                    });
                }

                currentRound++;
            }
        }
    }
}
