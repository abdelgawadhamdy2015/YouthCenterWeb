using AutoMapper;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Helpers;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class CupService(
        ICupRepo cupRepo,
        INotificationService notificationService,
        IYouthCenterActivityRepo youthCenterActivityRepo,
        IAvailabilityRepo availabilityRepo,
        IMapper mapper) : ICupService
    {
        private readonly ICupRepo _cupRepo = cupRepo;
        private readonly IYouthCenterActivityRepo _activityRepo = youthCenterActivityRepo;
        private readonly IAvailabilityRepo _availabilityRepo = availabilityRepo;
        private readonly IMapper _mapper = mapper;

        private static readonly TimeSpan MatchDuration = TimeSpan.FromHours(1);

        // ─────────────────────────────────────────────────────────────────────
        // AUTHORIZATION HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private static void RequireAdmin(UserRole callerRole)
        {
            if (callerRole != UserRole.Admin &&
                callerRole != UserRole.SuperAdmin)
                throw new UnauthorizedAccessException();
        }

        private async Task RequireSameCenter(Cup cup, int callerYouthCenterId)
        {
            var activity = await _activityRepo.GetByIdAsync(cup.YouthCenterActivityId);
            if (activity?.YouthCenterId != callerYouthCenterId)
                throw new UnauthorizedAccessException();
        }

        private async Task RequireAdminOfCup(Cup cup, UserRole callerRole, int callerYouthCenterId)
        {
            RequireAdmin(callerRole);
            await RequireSameCenter(cup, callerYouthCenterId);
        }

        // ─────────────────────────────────────────────────────────────────────
        // NOTIFICATION HELPERS
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Single entry point for every "broadcast to the activity's followers" notification.
        /// Always keys off cup.YouthCenterActivityId directly — never the navigation
        /// property, which isn't guaranteed to be loaded.
        /// </summary>
        private Task NotifyActivityTopicAsync(
            Cup cup,
            string title,
            string body,
            string notificationType)
        {
            return notificationService.SendToTopicAsync(
                topic: NotificationTopics.Activity(cup.YouthCenterActivity.ActivityId),
                title: title,
                body: body,
                data: new Dictionary<string, string>
                {
                    ["type"] = notificationType,
                    ["cupId"] = cup.Id.ToString()
                });
        }

        /// <summary>
        /// Broadcasts a topic notification for a cup's *current* status, called right
        /// after any successful status transition — whether triggered explicitly via
        /// UpdateAsync or automatically (cup fills up, group stage ends, final ends).
        /// Draft/unrecognized statuses are intentionally silent.
        /// </summary>
        private Task NotifyCupStatusChangeAsync(Cup cup)
        {
            var (body, type) = cup.Status switch
            {
                CupStatus.RegistrationOpen => (
                    $"Registration is now open for {cup.NameEn}. Join now!",
                    "cup_registration_open"),

                CupStatus.GroupStage => (
                    $"The group stage has begun for {cup.NameEn}. Check your matches!",
                    "cup_group_stage_started"),

                CupStatus.KnockoutStage => (
                    $"The knockout stage has begun for {cup.NameEn}. Good luck to all qualifiers!",
                    "cup_knockout_stage_started"),

                CupStatus.Completed => (
                    $"{cup.NameEn} has concluded. Thanks for a great tournament!",
                    "cup_completed"),

                CupStatus.Cancelled => (
                    $"{cup.NameEn} has been cancelled.",
                    "cup_cancelled"),

                _ => (null, null)
            };

            if (body is null) return Task.CompletedTask;

            return NotifyActivityTopicAsync(cup, cup.NameEn, body, type!);
        }

        // ─────────────────────────────────────────────────────────────────────
        #region  READ OPERATIONS
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupDto> GetByIdAsync(int id)
        {
            var cup = await _cupRepo.GetByIdAsync(id)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المطلوبة.",
                        messageEn: "Cup not found.");

            return _mapper.Map<CupDto>(cup);
        }

        public async Task<CupDetailDto> GetByIdWithDetailsAsync(int id)
        {
            var cup = await _cupRepo.GetByIdWithDetailsAsync(id)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المطلوبة.",
                        messageEn: "Cup not found.");

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
                ?? throw new NotFoundException(
                    messageAr: "لم يتم العثور على المجموعة المطلوبة.",
                    messageEn: "Group not found.");

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
        #endregion
        // ─────────────────────────────────────────────────────────────────────
        #region  CREATE
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupDto> CreateAsync(
            CreateCupDto dto,
            int adminId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            RequireAdmin(callerRole);

            var activity = await _activityRepo.GetByIdAsync(dto.YouthCenterActivityId)
                ?? throw new NotFoundException(
                    messageAr: "لم يتم العثور على نشاط مركز الشباب المحدد.",
                    messageEn: $"Youth center activity ({dto.YouthCenterActivityId}) was not found.");

            if (activity.YouthCenterId != callerYouthCenterId)
                throw new BusinessException(
                    messageAr: "يمكن للمسؤول إنشاء البطولة لمركز شبابه فقط.",
                    messageEn: "Admin can only create a cup for their own youth center.");

            var cup = _mapper.Map<Cup>(dto);
            cup.YouthCenterActivityId = dto.YouthCenterActivityId;
            cup.CreatedByAdminId = adminId;
            cup.Status = CupStatus.Draft;
            cup.CurrentTeamCount = 0;
            cup.CreatedAt = DateTime.UtcNow;

            await _cupRepo.CreateAsync(cup);

            // Cups are created as Draft — nobody's registration-eligible yet, so we
            // don't broadcast here. The "registration open" notification fires from
            // NotifyCupStatusChangeAsync once an admin actually flips the status
            // via UpdateAsync.

            return _mapper.Map<CupDto>(cup);
        }
        #endregion
        // ─────────────────────────────────────────────────────────────────────
        #region  UPDATE
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupDto> UpdateAsync(
            int cupId,
            UpdateCupDto dto,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: $"لم يتم العثور على البطولة بالمعرف ({cupId}).",
                        messageEn: $"Cup ({cupId}) was not found.");

            await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (cup.Status == CupStatus.Completed || cup.Status == CupStatus.Cancelled)
                throw new BusinessException(
                    messageAr: $"لا يمكن تعديل البطولة لأن حالتها الحالية هي '{cup.Status}'.",
                    messageEn: $"Cannot update a cup with status '{cup.Status}'.");

            // ── Description ───────────────────────────────────────────────────
            if (dto.Description != null)
                cup.Description = dto.Description.Trim().Length == 0
                    ? null
                    : dto.Description.Trim();

            // ── Status transition ─────────────────────────────────────────────
            bool statusChanged = false;
            if (dto.Status != null)
            {
                ValidateStatusTransition(cup.Status, (CupStatus)dto.Status!);
                statusChanged = cup.Status != (CupStatus)dto.Status!;
                cup.Status = (CupStatus)dto.Status!;
            }

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

                if (dto.MaxTeams != null && dto.MaxTeams > 0)
                {
                    if (dto.MaxTeams % dto.GroupSize != 0)
                        throw new BusinessException(
                            messageAr: $"عدد الفرق ({dto.MaxTeams}) يجب أن يكون قابلاً للقسمة على حجم المجموعة ({dto.GroupSize}).",
                            messageEn: $"MaxTeams ({dto.MaxTeams}) must be divisible by GroupSize ({dto.GroupSize}).");

                    cup.MaxTeams = (int)dto.MaxTeams!;
                }

                if (dto.GroupSize != null && dto.GroupSize > 1)
                {
                    if (cup.MaxTeams % dto.GroupSize != 0)
                        throw new BusinessException(
                            messageAr: $"حجم المجموعة ({dto.GroupSize}) لا يقسم عدد الفرق ({cup.MaxTeams}) بالتساوي.",
                            messageEn: $"GroupSize ({dto.GroupSize}) does not divide evenly into MaxTeams ({cup.MaxTeams}).");

                    cup.GroupSize = (int)dto.GroupSize!;
                }

                if (dto.TeamsAdvancingPerGroup != null && dto.TeamsAdvancingPerGroup > 0)
                {
                    if (dto.TeamsAdvancingPerGroup >= dto.GroupSize)
                        throw new BusinessException(
                            messageAr: $"عدد الفرق المتأهلة من كل مجموعة ({dto.TeamsAdvancingPerGroup}) يجب أن يكون أقل من حجم المجموعة ({dto.GroupSize}).",
                            messageEn: $"TeamsAdvancingPerGroup ({dto.TeamsAdvancingPerGroup}) must be less than GroupSize ({dto.GroupSize}).");

                    cup.TeamsAdvancingPerGroup = (int)dto.TeamsAdvancingPerGroup!;
                }

                if (dto.Format != null && dto.Format != default)
                    cup.Format = (CupFormat)dto.Format!;
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
                    throw new BusinessException(
                        messageAr: $"لا يمكن تغيير عدد الفرق أو حجم المجموعة أو عدد المتأهلين بعد فتح التسجيل (الحالة الحالية: '{cup.Status}').",
                        messageEn: $"MaxTeams, GroupSize and TeamsAdvancingPerGroup cannot be changed once registration has opened (status: '{cup.Status}').");
            }

            // ── Dates & fee (Draft + RegistrationOpen only) ───────────────────
            bool datesEditable = cup.Status == CupStatus.Draft
                            || cup.Status == CupStatus.RegistrationOpen;

            if (datesEditable)
            {
                if (dto.RegistrationDeadline != null && dto.RegistrationDeadline != default)
                {
                    if (dto.RegistrationDeadline < DateTime.UtcNow)
                        throw new BusinessException(
                            messageAr: "لا يمكن أن يكون الموعد النهائي للتسجيل في الماضي.",
                            messageEn: "Registration deadline cannot be in the past.");

                    cup.RegistrationDeadline = (DateTime)dto.RegistrationDeadline!;
                }

                if (dto.StartDate != default)
                {
                    if (dto.StartDate < DateTime.UtcNow)
                        throw new BusinessException(
                            messageAr: "لا يمكن أن يكون تاريخ البداية في الماضي.",
                            messageEn: "Start date cannot be in the past.");

                    if (cup.RegistrationDeadline != default &&
                        dto.StartDate <= cup.RegistrationDeadline)
                        throw new BusinessException(
                            messageAr: $"تاريخ البداية ({dto.StartDate:yyyy-MM-dd}) يجب أن يكون بعد الموعد النهائي للتسجيل ({cup.RegistrationDeadline:yyyy-MM-dd}).",
                            messageEn: $"Start date ({dto.StartDate:yyyy-MM-dd}) must be after registration deadline ({cup.RegistrationDeadline:yyyy-MM-dd}).");

                    cup.StartDate = (DateTime)dto.StartDate!;
                }

                if (dto.EntryFee.HasValue)
                {
                    if (dto.EntryFee.Value < 0)
                        throw new BusinessException(
                            messageAr: "لا يمكن أن تكون رسوم الاشتراك بالسالب.",
                            messageEn: "Entry fee cannot be negative.");

                    cup.EntryFee = dto.EntryFee.Value == 0 ? null : dto.EntryFee.Value;
                }
            }

            await _cupRepo.UpdateAsync(cup);

            if (statusChanged)
                await NotifyCupStatusChangeAsync(cup);

            return _mapper.Map<CupDto>(cup);
        }
        #endregion
        #region  DELETE  // ─────────────────────────────────────────────────────────────────────

        public async Task DeleteAsync(
            int cupId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: $"لم يتم العثور على البطولة بالمعرف ({cupId}).",
                        messageEn: $"Cup ({cupId}) was not found.");

            await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (cup.Status != CupStatus.Draft && cup.Status != CupStatus.Cancelled)
                throw new BusinessException(
                    messageAr: $"لا يمكن حذف البطولة لأن حالتها الحالية هي '{cup.Status}'. يمكن حذف الأكواب في مرحلة المسودة أو الملغاة فقط.",
                    messageEn: $"Cannot delete a cup with status '{cup.Status}'. Only Draft or Cancelled cups can be deleted.");

            await _cupRepo.DeleteAsync(cupId);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region JOIN REQUESTS  // ─────────────────────────────────────────────────────────────────────

        public async Task<CupTeamDto> SubmitJoinRequestAsync(
            int cupId,
            int userId,
            string teamName)
        {
            Cup cup = await _cupRepo.GetByIdAsync(cupId)
                   ?? throw new NotFoundException(
                       messageAr: "لم يتم العثور على البطولة المطلوبة.",
                       messageEn: "Cup not found.");

            if (cup.Status != CupStatus.RegistrationOpen)
                throw new BusinessException(
                    messageAr: $"هذا البطولة لا يقبل التسجيل حالياً (الحالة الحالية: '{cup.Status}').",
                    messageEn: $"This cup is not accepting registrations (current status: '{cup.Status}').");

            if (cup.CurrentTeamCount >= cup.MaxTeams)
                throw new BusinessException(
                    messageAr: $"هذا البطولة ممتلئة ({cup.MaxTeams}/{cup.MaxTeams} فريق).",
                    messageEn: $"This cup is full ({cup.MaxTeams}/{cup.MaxTeams} teams).");

            var existing = await _cupRepo.GetTeamByCupAndUserAsync(cupId, userId);
            if (existing != null)
            {
                bool isPending = existing.Status == CupTeamStatus.Pending;
                throw new BusinessException(
                    messageAr: isPending
                        ? "لديك طلب انضمام معلق بالفعل لهذا البطولة."
                        : "أنت مسجل بالفعل في هذا البطولة.",
                    messageEn: isPending
                        ? "You already have a pending request for this cup."
                        : "You already have an active registration for this cup.");
            }

            var team = new CupTeam
            {
                CupId = cupId,
                CaptainUserId = userId,
                TeamName = teamName.Trim(),
                Status = CupTeamStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            await _cupRepo.CreateTeamAsync(team);

            // Direct notification to the admin who owns the cup — not a topic
            // broadcast, since only that admin needs to act on it.
            await notificationService.SendToUserAsync(
                userId: cup.CreatedByAdminId,
                title: cup.NameEn,
                body: $"A new team has requested to join the \"{cup.NameEn}\" tournament. Review the request to approve or reject it.");

            return _mapper.Map<CupTeamDto>(team);
        }
        #endregion

        #region Accept And REject
        public async Task AcceptJoinRequestAsync(
            int teamId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var team = await _cupRepo.GetTeamByIdAsync(teamId)
                    ?? throw new NotFoundException(
                        messageAr: $"لم يتم العثور على الفريق بالمعرف ({teamId}).",
                        messageEn: $"Team ({teamId}) was not found.");

            var cup = await _cupRepo.GetByIdAsync(team.CupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المرتبطة بهذا الفريق.",
                        messageEn: "Cup associated with this team was not found.");

            await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (team.Status != CupTeamStatus.Pending)
                throw new BusinessException(
                    messageAr: $"لا يمكن قبول هذا الطلب لأن حالته الحالية هي '{team.Status}'.",
                    messageEn: $"This request is already '{team.Status}' and cannot be accepted again.");

            if (cup.Status != CupStatus.RegistrationOpen)
                throw new BusinessException(
                    messageAr: $"لا يمكن قبول الطلبات عندما تكون حالة البطولة '{cup.Status}'.",
                    messageEn: $"Cannot accept requests when cup status is '{cup.Status}'.");

            if (cup.CurrentTeamCount >= cup.MaxTeams)
                throw new BusinessException(
                    messageAr: "البطولة ممتلئة بالفعل. يرجى رفض هذا الطلب بدلاً من ذلك.",
                    messageEn: "The cup is already full. Reject this request instead.");

            var updatedCup = await _cupRepo.AcceptTeamAndIncrementCountAsync(teamId);

            await notificationService.SendToUserAsync(
                userId: team.CaptainUserId,
                title: cup.NameEn,
                body: $"Your team's request to join the \"{cup.NameEn}\" tournament has been approved. Good luck!");

            if (updatedCup.CurrentTeamCount == updatedCup.MaxTeams)
            {
                await GenerateGroupsAsync(updatedCup.Id);
                await GenerateGroupMatchesAsync(updatedCup.Id);

                updatedCup.Status = CupStatus.GroupStage;
                await _cupRepo.UpdateAsync(updatedCup);
                await NotifyCupStatusChangeAsync(updatedCup);
            }
        }

        public async Task RejectJoinRequestAsync(
            int teamId,
            UserRole callerRole,
            int callerYouthCenterId)
        {
            var team = await _cupRepo.GetTeamByIdAsync(teamId)
                    ?? throw new NotFoundException(
                        messageAr: $"لم يتم العثور على الفريق بالمعرف ({teamId}).",
                        messageEn: $"Team ({teamId}) was not found.");

            var cup = await _cupRepo.GetByIdAsync(team.CupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المرتبطة بهذا الفريق.",
                        messageEn: "Cup associated with this team was not found.");

            await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (team.Status != CupTeamStatus.Pending)
                throw new BusinessException(
                    messageAr: $"يمكن رفض الطلبات المعلقة فقط. الحالة الحالية: '{team.Status}'.",
                    messageEn: $"Only pending requests can be rejected. Current status: '{team.Status}'.");

            await _cupRepo.RejectTeamAsync(teamId);

            await notificationService.SendToUserAsync(
                userId: team.CaptainUserId,
                title: cup.NameEn,
                body: $"Your team's request to join the \"{cup.NameEn}\" tournament has been declined.");
        }
        #endregion

        // ─────────────────────────────────────────────────────────────────────
        #region  MATCH MANAGEMENT
        // ─────────────────────────────────────────────────────────────────────

        public async Task<CupMatchDto> EnterMatchResultAsync(
    int matchId,
    EnterMatchResultDto dto,
    UserRole callerRole,
    int callerYouthCenterId)
        {
            var match = await _cupRepo.GetMatchByIdAsync(matchId)
                ?? throw new NotFoundException(
                    messageAr: $"لم يتم العثور على المباراة بالمعرف ({matchId}).",
                    messageEn: $"Match ({matchId}) was not found.");

            var cup = await _cupRepo.GetByIdAsync(match.CupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المرتبطة بهذه المباراة.",
                        messageEn: "Cup associated with this match was not found.");

            await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (match.Status == CupMatchStatus.Completed)
                throw new BusinessException(
                    messageAr: "تم إدخال نتيجة هذه المباراة بالفعل.",
                    messageEn: "This match result has already been entered.");

            if (match.TeamAId == null || match.TeamBId == null)
                throw new BusinessException(
                    messageAr: "لا يمكن إدخال نتيجة لمباراة لم يتم تحديد فرقها بعد. انتظر اكتمال الجولة السابقة.",
                    messageEn: "Cannot enter a result for a match that has no assigned teams yet. Wait for the previous round to complete.");

            if (dto.ScoreA < 0 || dto.ScoreB < 0)
                throw new BusinessException(
                    messageAr: "لا يمكن أن تكون النتيجة بالسالب.",
                    messageEn: "Scores cannot be negative.");

            int? winnerId = null;

            // ── Group stage ───────────────────────────────────────────────────
            if (match.Stage == CupMatchStage.GroupStage)
            {
                if (dto.ScoreA > dto.ScoreB)
                    winnerId = match.TeamAId;
                else if (dto.ScoreB > dto.ScoreA)
                    winnerId = match.TeamBId;
                // else: draw, winnerId stays null

                await _cupRepo.EnterMatchResultAsync(matchId, dto.ScoreA, dto.ScoreB, winnerId);
                await UpdateStandings(match, dto);

                var allDone = await _cupRepo.AllGroupMatchesCompletedAsync(match.CupId);
                if (allDone)
                {
                    await _cupRepo.SeedKnockoutMatchesAsync(match.CupId);
                    cup.Status = CupStatus.KnockoutStage;
                    await _cupRepo.UpdateAsync(cup);
                    await NotifyCupStatusChangeAsync(cup);
                }
            }
            // ── Knockout stage ────────────────────────────────────────────────
            else
            {
                if (dto.ScoreA == dto.ScoreB)
                {
                    if (dto.PenaltyScoreA == null || dto.PenaltyScoreB == null)
                        throw new BusinessException(
                            messageAr: "المباراة انتهت بالتعادل. يجب إدخال نتيجة ركلات الترجيح لتحديد الفائز.",
                            messageEn: "The match ended in a draw. Penalty shootout scores are required to determine a winner.");

                    if (dto.PenaltyScoreA < 0 || dto.PenaltyScoreB < 0)
                        throw new BusinessException(
                            messageAr: "لا يمكن أن تكون نتيجة ركلات الترجيح بالسالب.",
                            messageEn: "Penalty shootout scores cannot be negative.");

                    if (dto.PenaltyScoreA == dto.PenaltyScoreB)
                        throw new BusinessException(
                            messageAr: "لا يمكن أن تنتهي ركلات الترجيح بالتعادل.",
                            messageEn: "Penalty shootout cannot end in a draw.");

                    winnerId = dto.PenaltyScoreA > dto.PenaltyScoreB ? match.TeamAId : match.TeamBId;
                }
                else
                {
                    winnerId = dto.ScoreA > dto.ScoreB ? match.TeamAId : match.TeamBId;
                }

                await _cupRepo.EnterMatchResultAsync(
                    matchId, dto.ScoreA, dto.ScoreB, winnerId,
                    dto.PenaltyScoreA, dto.PenaltyScoreB);

                await _cupRepo.AdvanceWinnerToNextRoundAsync(
                    match.CupId, match.Round, match.MatchNumber, winnerId!.Value);

                if (match.Stage == CupMatchStage.SemiFinal)
                {
                    int loserId = winnerId == match.TeamAId
                        ? match.TeamBId!.Value
                        : match.TeamAId!.Value;

                    await _cupRepo.AssignThirdPlaceParticipantAsync(match.CupId, loserId);
                }

                if (match.Stage == CupMatchStage.FinalMatch)
                {
                    cup.Status = CupStatus.Completed;
                    await _cupRepo.UpdateAsync(cup);
                    await NotifyCupStatusChangeAsync(cup);
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
                ?? throw new NotFoundException(
                    messageAr: $"لم يتم العثور على المباراة بالمعرف ({matchId}).",
                    messageEn: $"Match ({matchId}) was not found.");

            var cup = await _cupRepo.GetByIdAsync(match.CupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المرتبطة بهذه المباراة.",
                        messageEn: "Cup associated with this match was not found.");

            await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

            if (scheduledAt < DateTime.UtcNow)
                throw new BusinessException(
                    messageAr: "لا يمكن أن يكون موعد المباراة في الماضي.",
                    messageEn: "Scheduled date cannot be in the past.");

            if (match.Status == CupMatchStatus.Completed)
                throw new BusinessException(
                    messageAr: "لا يمكن إعادة جدولة مباراة منتهية.",
                    messageEn: "Cannot reschedule a completed match.");

            var start = TimeOnly.FromDateTime(scheduledAt);
            var end = start.Add(MatchDuration);

            var conflict = await _availabilityRepo.HasConflictAsync(
                cup.YouthCenterActivityId,
                scheduledAt.Date,
                start,
                end,
                excludeMatchId: matchId);

            if (conflict)
                throw new BusinessException(
                    messageAr: "هذا الموعد محجوز بالفعل أو به مباراة أخرى مجدولة على نفس النشاط.",
                    messageEn: "This time slot is already reserved or has another match scheduled on this activity.");

            match.ScheduledAt = scheduledAt;
            await _cupRepo.UpdateMatchAsync(match);

            // Scheduling a single match is a per-match detail, not a cup-wide
            // event, so it stays without a topic broadcast. If you want to notify
            // both teams' captains here, that's a SendToUserAsync x2, not a topic.

            return _mapper.Map<CupMatchDto>(match);
        }
        #endregion
        // ─────────────────────────────────────────────────────────────────────
        #region  PRIVATE HELPERS
        // ─────────────────────────────────────────────────────────────────────

        private async Task UpdateStandings(CupMatch match, EnterMatchResultDto dto)
        {
            if (dto.ScoreA > dto.ScoreB)
            {
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value, 3, 1, 0, 0, dto.ScoreA, dto.ScoreB);
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value, 0, 0, 0, 1, dto.ScoreB, dto.ScoreA);
            }
            else if (dto.ScoreB > dto.ScoreA)
            {
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value, 3, 1, 0, 0, dto.ScoreB, dto.ScoreA);
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value, 0, 0, 0, 1, dto.ScoreA, dto.ScoreB);
            }
            else
            {
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamAId!.Value, 1, 0, 1, 0, dto.ScoreA, dto.ScoreB);
                await _cupRepo.UpdateTeamStandingsAsync(
                    match.TeamBId!.Value, 1, 0, 1, 0, dto.ScoreB, dto.ScoreA);
            }
        }

        private async Task GenerateGroupsAsync(int cupId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المطلوبة.",
                        messageEn: "Cup not found.");

            var teams = (await _cupRepo.GetAcceptedTeamsByCupAsync(cupId))
                .OrderBy(_ => Guid.NewGuid())
                .ToList();

            if (teams.Count != cup.MaxTeams)
                throw new BusinessException(
                    messageAr: $"لا يمكن توليد المجموعات: المتوقع {cup.MaxTeams} فرق ولكن تم العثور على {teams.Count} فقط.",
                    messageEn: $"Cannot generate groups: expected {cup.MaxTeams} teams but found {teams.Count}.");

            int groupCount = cup.MaxTeams / cup.GroupSize;

            var groups = Enumerable.Range(1, groupCount)
                .Select(i => new CupGroup
                {
                    CupId = cupId,
                    GroupNumber = i,
                    Name = $"Group {(char)('A' + i - 1)}"
                })
                .ToList();

            await _cupRepo.CreateGroupsAsync(groups);

            var savedGroups = (await _cupRepo.GetGroupsByCupAsync(cupId))
                .OrderBy(g => g.GroupNumber)
                .ToList();

            var assignments = teams
                .Select((team, i) => (
                    TeamId: team.Id,
                    GroupId: savedGroups[i % groupCount].Id))
                .ToList();

            await _cupRepo.AssignGroupsToTeamsAsync(assignments);
        }

        private async Task GenerateGroupMatchesAsync(int cupId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المطلوبة.",
                        messageEn: "Cup not found.");

            var groups = (await _cupRepo.GetGroupsByCupAsync(cupId)).ToList();
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

            CreateKnockoutBracket(cupId, groups.Count, cup.TeamsAdvancingPerGroup, matches);
            await _cupRepo.CreateMatchesAsync(matches);
        }

        private static void CreateKnockoutBracket(
    int cupId,
    int groupCount,
    int teamsAdvancingPerGroup,
    List<CupMatch> matches)
        {
            int knockoutTeams = groupCount * teamsAdvancingPerGroup;

            bool isPowerOfTwo = knockoutTeams >= 2 && (knockoutTeams & (knockoutTeams - 1)) == 0;
            if (!isPowerOfTwo)
                throw new BusinessException(
                    messageAr: $"عدد الفرق المتأهلة للأدوار الإقصائية ({knockoutTeams}) يجب أن يكون قوة للعدد 2.",
                    messageEn: $"Number of knockout-stage teams ({knockoutTeams}) must be a power of two.");

            int currentCount = knockoutTeams / 2;
            int round = 2;
            int finalRound = round + (int)Math.Log2(knockoutTeams) - 1;

            while (currentCount >= 1)
            {
                CupMatchStage stage = currentCount switch
                {
                    1 => CupMatchStage.FinalMatch,
                    2 => CupMatchStage.SemiFinal,
                    4 => CupMatchStage.QuarterFinal,
                    8 => CupMatchStage.RoundOf16,
                    _ => CupMatchStage.RoundOf32
                };

                for (int i = 1; i <= currentCount; i++)
                {
                    matches.Add(new CupMatch
                    {
                        CupId = cupId,
                        Stage = stage,
                        Round = round,
                        MatchNumber = i,
                        Status = CupMatchStatus.Pending
                    });
                }

                if (currentCount == 2)
                {
                    matches.Add(new CupMatch
                    {
                        CupId = cupId,
                        Stage = CupMatchStage.ThirdPlace,
                        Round = finalRound,
                        MatchNumber = 2,
                        Status = CupMatchStatus.Pending
                    });
                }

                currentCount /= 2;
                round++;
            }
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
                throw new BusinessException(
                    messageAr: $"انتقال الحالة غير مسموح به: '{current}' ← '{next}'. الانتقالات المسموح بها من '{current}': {GetAllowedTransitions(current)}.",
                    messageEn: $"Invalid status transition: '{current}' → '{next}'. Allowed from '{current}': {GetAllowedTransitions(current)}.");
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
    #endregion
}