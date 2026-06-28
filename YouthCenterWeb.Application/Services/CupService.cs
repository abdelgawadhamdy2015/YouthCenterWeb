using AutoMapper;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class CupService(
        ICupRepo cupRepo,
        IYouthCenterActivityRepo youthCenterActivityRepo,
        IMapper mapper) : ICupService
    {
        private readonly ICupRepo _cupRepo = cupRepo;
        private readonly IYouthCenterActivityRepo _activityRepo = youthCenterActivityRepo;
        private readonly IMapper _mapper = mapper;

        // ─────────────────────────────────────────────────────────────────────
        // AUTHORIZATION HELPERS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Throws if the caller is not an admin.
        /// Call this on every write operation before touching data.
        /// </summary>
        private static void RequireAdmin(UserRole callerRole)
        {
            // Adjust the role value to match your UserRole enum
            // e.g. if UserRole.Admin == 1 use: if (callerRole != 1)
            if (callerRole != UserRole.Admin &&
                callerRole != UserRole.SuperAdmin)
                throw new UnauthorizedAccessException(
                    "Only admins can perform this action.");
        }

        /// <summary>
        /// Throws if the admin does not belong to the same youth center as the cup.
        /// Must be called AFTER the cup is loaded from the database.
        /// </summary>
        private static void RequireSameCenter(
     Cup cup,
     int callerYouthCenterId)
        {
            if (cup.YouthCenterActivity.YouthCenterId != callerYouthCenterId)
                throw new UnauthorizedAccessException(
                    "You can only manage cups that belong to your youth center.");
        }

        /// <summary>
        /// Combines both checks in one call for convenience.
        /// </summary>
        private static void RequireAdminOfCup(Cup cup, UserRole callerRole, int callerYouthCenterId)
        {
            RequireAdmin(callerRole);
            RequireSameCenter(cup, callerYouthCenterId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // READ OPERATIONS  (no auth — any authenticated user can read)
        // ─────────────────────────────────────────────────────────────────────

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

        public async Task<IEnumerable<CupGroupDto>> GetGroupsAsync(int cupId)
        {
            var groups = await _cupRepo.GetGroupsByCupAsync(cupId);
            return _mapper.Map<IEnumerable<CupGroupDto>>(groups);
        }

        public async Task<IEnumerable<CupStandingDto>> GetGroupStandingsAsync(int groupId)
        {
            var group = await _cupRepo.GetGroupByIdAsync(groupId)
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

        public async Task<IEnumerable<CupTeamDto>> GetPendingRequestsAsync(int cupId)
        {
            var teams = await _cupRepo.GetPendingTeamsByCupAsync(cupId);
            return _mapper.Map<IEnumerable<CupTeamDto>>(teams);
        }

        public async Task<IEnumerable<CupTeamDto>> GetAllTeamsAsync(int cupId)
        {
            var teams = await _cupRepo.GetTeamsByCupAsync(cupId);
            return _mapper.Map<IEnumerable<CupTeamDto>>(teams);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CREATE  (admin of that center only)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupDto> CreateAsync(
            CreateCupDto dto,
            int adminId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            // 1. Must be an admin
            RequireAdmin(callerRole);
            var activity = await _activityRepo.GetByIdAsync(dto.YouthCenterActivityId) ?? throw new NotFoundException("youthCenterActivity", dto.YouthCenterActivityId);
            if (activity.YouthCenterId != callerYouthCenterId) throw new BusinessException(" Admin can only create cup for his YouthCenter");
            var cup = _mapper.Map<Cup>(dto);
            // 2. The cup's youth center must match the caller's center
            cup.YouthCenterActivityId = dto.YouthCenterActivityId;
            cup.CreatedByAdminId = adminId;
            cup.Status = CupStatus.Draft;
            cup.CurrentTeamCount = 0;
            cup.CreatedAt = DateTime.UtcNow;

            await _cupRepo.CreateAsync(cup);

            return _mapper.Map<CupDto>(cup);
        }

        // ─────────────────────────────────────────────────────────────────────
        // UPDATE  (admin of same center only + status-based field rules)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupDto> UpdateAsync(
            int cupId,
            UpdateCupDto dto,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                      ?? throw new KeyNotFoundException($"Cup {cupId} not found");

            // Auth: must be admin of the same center
            RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            // Status guard: completed/cancelled cups are frozen
            if (cup.Status == CupStatus.Completed || cup.Status == CupStatus.Cancelled)
                throw new InvalidOperationException(
                    $"Cannot update a cup with status '{cup.Status}'.");

            // ── Description (nullable patch) ──────────────────────────────────
            if (dto.Description != null)
                cup.Description = dto.Description.Trim().Length == 0
                    ? null
                    : dto.Description.Trim();

            // ── Status transition ─────────────────────────────────────────────
            ValidateStatusTransition(cup.Status, dto.Status);
            cup.Status = dto.Status;

            // ── Structural fields (Draft only) ────────────────────────────────
            bool registrationHasStarted = cup.Status == CupStatus.RegistrationOpen
                                       || cup.Status == CupStatus.GroupStage
                                       || cup.Status == CupStatus.KnockoutStage;

            if (!registrationHasStarted)
            {
                if (!string.IsNullOrWhiteSpace(dto.NameAr))
                    cup.NameAr = dto.NameAr.Trim();

                if (!string.IsNullOrWhiteSpace(dto.NameEn))
                    cup.NameEn = dto.NameEn.Trim();

                if (dto.MaxTeams > 0)
                {
                    if (dto.MaxTeams % dto.GroupSize != 0)
                        throw new InvalidOperationException(
                            $"MaxTeams ({dto.MaxTeams}) must be divisible by GroupSize ({dto.GroupSize}).");

                    cup.MaxTeams = dto.MaxTeams;
                }

                if (dto.GroupSize > 1)
                {
                    if (cup.MaxTeams % dto.GroupSize != 0)
                        throw new InvalidOperationException(
                            $"GroupSize ({dto.GroupSize}) does not divide evenly into MaxTeams ({cup.MaxTeams}).");

                    cup.GroupSize = dto.GroupSize;
                }

                if (dto.TeamsAdvancingPerGroup > 0)
                {
                    if (dto.TeamsAdvancingPerGroup >= dto.GroupSize)
                        throw new InvalidOperationException(
                            $"TeamsAdvancingPerGroup ({dto.TeamsAdvancingPerGroup}) must be " +
                            $"less than GroupSize ({dto.GroupSize}).");

                    cup.TeamsAdvancingPerGroup = dto.TeamsAdvancingPerGroup;
                }

                if (dto.Format != default)
                    cup.Format = dto.Format;
            }
            else
            {
                // Names still editable post-registration (typo fixes)
                if (!string.IsNullOrWhiteSpace(dto.NameAr))
                    cup.NameAr = dto.NameAr.Trim();

                if (!string.IsNullOrWhiteSpace(dto.NameEn))
                    cup.NameEn = dto.NameEn.Trim();

                bool structureChanged =
                    (dto.MaxTeams > 0 && dto.MaxTeams != cup.MaxTeams) ||
                    (dto.GroupSize > 1 && dto.GroupSize != cup.GroupSize) ||
                    (dto.TeamsAdvancingPerGroup > 0 &&
                     dto.TeamsAdvancingPerGroup != cup.TeamsAdvancingPerGroup);

                if (structureChanged)
                    throw new InvalidOperationException(
                        "MaxTeams, GroupSize and TeamsAdvancingPerGroup cannot be changed " +
                        $"once registration has opened (status: '{cup.Status}').");
            }

            // ── Dates & fee (Draft + RegistrationOpen only) ───────────────────
            bool datesEditable = cup.Status == CupStatus.Draft
                               || cup.Status == CupStatus.RegistrationOpen;

            if (datesEditable)
            {
                if (dto.RegistrationDeadline != default)
                {
                    if (dto.RegistrationDeadline < DateTime.UtcNow)
                        throw new InvalidOperationException(
                            "Registration deadline cannot be in the past.");

                    cup.RegistrationDeadline = dto.RegistrationDeadline;
                }

                if (dto.StartDate != default)
                {
                    if (dto.StartDate < DateTime.UtcNow)
                        throw new InvalidOperationException(
                            "Start date cannot be in the past.");

                    if (cup.RegistrationDeadline != default &&
                        dto.StartDate <= cup.RegistrationDeadline)
                        throw new InvalidOperationException(
                            $"Start date ({dto.StartDate:yyyy-MM-dd}) must be after " +
                            $"registration deadline ({cup.RegistrationDeadline:yyyy-MM-dd}).");

                    cup.StartDate = dto.StartDate;
                }

                if (dto.EntryFee.HasValue)
                {
                    if (dto.EntryFee.Value < 0)
                        throw new InvalidOperationException(
                            "Entry fee cannot be negative.");

                    cup.EntryFee = dto.EntryFee.Value == 0 ? null : dto.EntryFee.Value;
                }
            }

            await _cupRepo.UpdateAsync(cup);
            return _mapper.Map<CupDto>(cup);
        }

        // ─────────────────────────────────────────────────────────────────────
        // DELETE  (admin of same center + Draft only)
        // ─────────────────────────────────────────────────────────────────────

        public async Task DeleteAsync(
            int cupId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                      ?? throw new KeyNotFoundException($"Cup {cupId} not found");

            RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            // Only allow deletion while the cup has never started
            if (cup.Status != CupStatus.Draft && cup.Status != CupStatus.Cancelled)
                throw new InvalidOperationException(
                    $"Cannot delete a cup with status '{cup.Status}'. " +
                    "Only Draft or Cancelled cups can be deleted. " +
                    "Set status to Cancelled first if you need to stop an active cup.");

            await _cupRepo.DeleteAsync(cupId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // JOIN REQUESTS  (any user can submit; accept/reject = admin same center)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupTeamDto> SubmitJoinRequestAsync(
            int cupId,
            int userId,
            string teamName)
        {
            // No admin check — any authenticated user can submit a join request
            var cup = await _cupRepo.GetByIdAsync(cupId)
                      ?? throw new KeyNotFoundException("Cup not found");

            if (cup.Status != CupStatus.RegistrationOpen)
                throw new InvalidOperationException(
                    "This cup is not accepting registrations " +
                    $"(current status: '{cup.Status}').");

            if (cup.CurrentTeamCount >= cup.MaxTeams)
                throw new InvalidOperationException(
                    $"This cup is full ({cup.MaxTeams}/{cup.MaxTeams} teams).");

            var existing = await _cupRepo.GetTeamByCupAndUserAsync(cupId, userId);
            if (existing != null)
                throw new InvalidOperationException(
                    existing.Status == CupTeamStatus.Pending
                        ? "You already have a pending request for this cup."
                        : "You already have an active registration for this cup.");

            var team = new CupTeam
            {
                CupId = cupId,
                CaptainUserId = userId,
                TeamName = teamName.Trim(),
                Status = CupTeamStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            await _cupRepo.CreateTeamAsync(team);
            return _mapper.Map<CupTeamDto>(team);
        }

        public async Task AcceptJoinRequestAsync(
            int teamId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            // Load the team first so we can get the cup for the center check
            var team = await _cupRepo.GetTeamByIdAsync(teamId)
                       ?? throw new KeyNotFoundException($"Team {teamId} not found");

            var cup = await _cupRepo.GetByIdAsync(team.CupId)
                      ?? throw new KeyNotFoundException("Cup not found");

            RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (team.Status != CupTeamStatus.Pending)
                throw new InvalidOperationException(
                    $"This request is already '{team.Status}' and cannot be accepted again.");

            if (cup.Status != CupStatus.RegistrationOpen)
                throw new InvalidOperationException(
                    $"Cannot accept requests when cup status is '{cup.Status}'.");

            if (cup.CurrentTeamCount >= cup.MaxTeams)
                throw new InvalidOperationException(
                    "The cup is already full. Reject this request instead.");

            var updatedCup = await _cupRepo.AcceptTeamAndIncrementCountAsync(teamId);

            // If the cup is now full → generate groups and matches automatically
            if (updatedCup.CurrentTeamCount == updatedCup.MaxTeams)
            {
                await GenerateGroupsAsync(updatedCup.Id);
                await GenerateGroupMatchesAsync(updatedCup.Id);

                updatedCup.Status = CupStatus.GroupStage;
                await _cupRepo.UpdateAsync(updatedCup);
            }
        }

        public async Task RejectJoinRequestAsync(
            int teamId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var team = await _cupRepo.GetTeamByIdAsync(teamId)
                       ?? throw new KeyNotFoundException($"Team {teamId} not found");

            var cup = await _cupRepo.GetByIdAsync(team.CupId)
                      ?? throw new KeyNotFoundException("Cup not found");

            RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (team.Status != CupTeamStatus.Pending)
                throw new InvalidOperationException(
                    $"Only pending requests can be rejected. " +
                    $"Current status: '{team.Status}'.");

            await _cupRepo.RejectTeamAsync(teamId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // MATCH MANAGEMENT  (admin of same center only)
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupMatchDto> EnterMatchResultAsync(
            int matchId,
            EnterMatchResultDto dto,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var match = await _cupRepo.GetMatchByIdAsync(matchId)
                ?? throw new KeyNotFoundException("Match not found");

            var cup = await _cupRepo.GetByIdAsync(match.CupId)
                      ?? throw new KeyNotFoundException("Cup not found");

            RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (match.Status == CupMatchStatus.Completed)
                throw new InvalidOperationException("This match result has already been entered.");

            if (match.TeamAId == null || match.TeamBId == null)
                throw new InvalidOperationException(
                    "Cannot enter a result for a match that has no assigned teams yet. " +
                    "Wait for the previous round to complete.");

            if (dto.ScoreA < 0 || dto.ScoreB < 0)
                throw new InvalidOperationException("Scores cannot be negative.");

            int? winnerId = null;

            // ── Group stage ───────────────────────────────────────────────────
            if (match.Stage == CupMatchStage.GroupStage)
            {
                // Draws are allowed in group stage
                if (dto.ScoreA > dto.ScoreB)
                    winnerId = match.TeamAId;
                else if (dto.ScoreB > dto.ScoreA)
                    winnerId = match.TeamBId;
                // draw → winnerId stays null

                await _cupRepo.EnterMatchResultAsync(
                    matchId,
                    dto.ScoreA,
                    dto.ScoreB,
                    winnerId ?? 0);

                await UpdateStandings(match, dto);

                // If all group matches done → seed knockout
                var allDone = await _cupRepo.AllGroupMatchesCompletedAsync(match.CupId);
                if (allDone)
                {
                    await _cupRepo.SeedKnockoutMatchesAsync(match.CupId);
                    cup.Status = CupStatus.KnockoutStage;
                    await _cupRepo.UpdateAsync(cup);
                }
            }
            // ── Knockout stage ────────────────────────────────────────────────
            else
            {
                // No draws in knockout
                if (dto.ScoreA == dto.ScoreB)
                    throw new InvalidOperationException(
                        "Knockout matches cannot end in a draw. " +
                        "Use extra time / penalty scores if needed.");

                winnerId = dto.ScoreA > dto.ScoreB
                    ? match.TeamAId
                    : match.TeamBId;

                await _cupRepo.EnterMatchResultAsync(
                    matchId,
                    dto.ScoreA,
                    dto.ScoreB,
                    winnerId!.Value);

                // Advance the winner to the next bracket slot
                await _cupRepo.AdvanceWinnerToNextRoundAsync(
                    match.CupId,
                    match.Round,
                    match.MatchNumber,
                    winnerId.Value);

                // If the final is done → cup is completed
                if (match.Stage == CupMatchStage.Final)
                {
                    cup.Status = CupStatus.Completed;
                    await _cupRepo.UpdateAsync(cup);
                }
            }

            var updated = await _cupRepo.GetMatchByIdAsync(matchId);
            return _mapper.Map<CupMatchDto>(updated);
        }

        public async Task<CupMatchDto> ScheduleMatchAsync(
            int matchId,
            DateTime scheduledAt,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var match = await _cupRepo.GetMatchByIdAsync(matchId)
                ?? throw new KeyNotFoundException("Match not found");

            var cup = await _cupRepo.GetByIdAsync(match.CupId)
                      ?? throw new KeyNotFoundException("Cup not found");

            RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (scheduledAt < DateTime.UtcNow)
                throw new InvalidOperationException(
                    "Scheduled date cannot be in the past.");

            if (match.Status == CupMatchStatus.Completed)
                throw new InvalidOperationException(
                    "Cannot reschedule a completed match.");

            match.ScheduledAt = scheduledAt;
            await _cupRepo.UpdateMatchAsync(match);

            return _mapper.Map<CupMatchDto>(match);
        }

        // ─────────────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private async Task UpdateStandings(CupMatch match, EnterMatchResultDto dto)
        {
            if (dto.ScoreA > dto.ScoreB)
            {
                // A wins
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value, 3, 1, 0, 0, dto.ScoreA, dto.ScoreB);
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value, 0, 0, 0, 1, dto.ScoreB, dto.ScoreA);
            }
            else if (dto.ScoreB > dto.ScoreA)
            {
                // B wins
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value, 3, 1, 0, 0, dto.ScoreB, dto.ScoreA);
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value, 0, 0, 0, 1, dto.ScoreA, dto.ScoreB);
            }
            else
            {
                // Draw
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value, 1, 0, 1, 0, dto.ScoreA, dto.ScoreB);
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value, 1, 0, 1, 0, dto.ScoreB, dto.ScoreA);
            }
        }

        private async Task GenerateGroupsAsync(int cupId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                      ?? throw new KeyNotFoundException("Cup not found");

            var teams = (await _cupRepo.GetAcceptedTeamsByCupAsync(cupId))
                .OrderBy(_ => Guid.NewGuid()) // random shuffle
                .ToList();

            if (teams.Count != cup.MaxTeams)
                throw new InvalidOperationException(
                    $"Cannot generate groups: expected {cup.MaxTeams} teams but found {teams.Count}.");

            int groupCount = cup.MaxTeams / cup.GroupSize;

            // Create the group rows (A, B, C …)
            var groups = Enumerable.Range(1, groupCount)
                .Select(i => new CupGroup
                {
                    CupId = cupId,
                    GroupNumber = i,
                    Name = $"Group {(char)('A' + i - 1)}"
                })
                .ToList();

            await _cupRepo.CreateGroupsAsync(groups);

            // Reload to get the auto-generated IDs
            var savedGroups = (await _cupRepo.GetGroupsByCupAsync(cupId))
                .OrderBy(g => g.GroupNumber)
                .ToList();

            // Round-robin assignment: team[0] → group[0], team[1] → group[1] … wrap around
            var assignments = teams
                .Select((team, i) => (
                    TeamId: team.Id,
                    GroupId: savedGroups[i % groupCount].Id))
                .ToList();

            await _cupRepo.AssignGroupsToTeamsAsync(assignments);
        }

        private async Task GenerateGroupMatchesAsync(int cupId)
        {
            var groups = (await _cupRepo.GetGroupsByCupAsync(cupId)).ToList();
            var matches = new List<CupMatch>();
            int matchNumber = 1;

            // Round-robin within each group: every team vs every other team once
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

            // Pre-create empty knockout bracket slots
            await CreateKnockoutBracket(cupId, groups.Count, matches);

            await _cupRepo.CreateMatchesAsync(matches);
        }

        private static Task CreateKnockoutBracket(
            int cupId,
            int groupCount,
            List<CupMatch> matches)
        {
            // e.g. 2 groups × 2 advancing = 4 knockout teams → 2 semi-final slots
            //      4 groups × 2 advancing = 8 knockout teams → 4 QF slots
            int knockoutTeams = groupCount * 2;
            int firstRoundMatchCount = knockoutTeams / 2;
            int round = 2;

            CupMatchStage FirstStage(int count) => count switch
            {
                2 => CupMatchStage.SemiFinal,
                4 => CupMatchStage.QuarterFinal,
                _ => CupMatchStage.RoundOf16
            };

            var firstStage = FirstStage(firstRoundMatchCount);

            for (int i = 1; i <= firstRoundMatchCount; i++)
            {
                matches.Add(new CupMatch
                {
                    CupId = cupId,
                    Stage = firstStage,
                    Round = round,
                    MatchNumber = i,
                    Status = CupMatchStatus.Pending
                    // TeamAId / TeamBId deliberately null — filled by SeedKnockoutMatchesAsync
                });
            }

            int currentCount = firstRoundMatchCount;
            int currentRound = round + 1;

            while (currentCount > 1)
            {
                currentCount /= 2;

                CupMatchStage stage = currentCount switch
                {
                    1 => CupMatchStage.Final,
                    2 => CupMatchStage.SemiFinal,
                    4 => CupMatchStage.QuarterFinal,
                    _ => CupMatchStage.RoundOf16
                };

                for (int i = 1; i <= currentCount; i++)
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

            return Task.CompletedTask;
        }

        // ─── Status Transition Validator ──────────────────────────────────────

        private static void ValidateStatusTransition(CupStatus current, CupStatus next)
        {
            if (current == next) return;

            bool valid = (current, next) switch
            {
                (CupStatus.Draft, CupStatus.RegistrationOpen) => true,
                (CupStatus.Draft, CupStatus.Cancelled) => true,

                (CupStatus.RegistrationOpen, CupStatus.Draft) => true,
                (CupStatus.RegistrationOpen, CupStatus.GroupStage) => true,
                (CupStatus.RegistrationOpen, CupStatus.Cancelled) => true,

                (CupStatus.GroupStage, CupStatus.KnockoutStage) => true,
                (CupStatus.GroupStage, CupStatus.Cancelled) => true,

                (CupStatus.KnockoutStage, CupStatus.Completed) => true,
                (CupStatus.KnockoutStage, CupStatus.Cancelled) => true,

                _ => false
            };

            if (!valid)
                throw new InvalidOperationException(
                    $"Invalid status transition: '{current}' → '{next}'. " +
                    $"Allowed from '{current}': {GetAllowedTransitions(current)}.");
        }

        private static string GetAllowedTransitions(CupStatus current) =>
            current switch
            {
                CupStatus.Draft => "RegistrationOpen, Cancelled",
                CupStatus.RegistrationOpen => "Draft, GroupStage, Cancelled",
                CupStatus.GroupStage => "KnockoutStage, Cancelled",
                CupStatus.KnockoutStage => "Completed, Cancelled",
                _ => "none"
            };
    }
}