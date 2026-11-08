using AutoMapper;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.ICupScheduler;
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
        ICupScheduler scheduler,
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
        private async Task NotifyActivityTopicAsync(
    Cup cup, string title, string body, string eventName)
        {
            var ya = await _activityRepo.GetByIdAsync(cup.YouthCenterActivityId);
            if (ya is null) return;

            await notificationService.BroadcastToActivityAsync(
                activityId: ya.ActivityId,
                topic: NotificationTopics.Activity(ya.ActivityId),
                title: title,
                body: body,
                type: "cup",
                targetId: cup.Id,
                data: new Dictionary<string, string> { ["event"] = eventName });
        }

        /// <summary>
        /// Broadcasts a topic notification for a cup's *current* status, called right
        /// after any successful status transition — whether triggered explicitly via
        /// UpdateAsync or automatically (cup fills up, group stage ends, final ends).
        /// Draft/unrecognized statuses are intentionally silent.
        /// </summary>
        private async Task NotifyCupStatusChangeAsync(Cup cup)
        {
            var (body, eventName) = cup.Status switch
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

            if (body is null) return;

            await NotifyActivityTopicAsync(cup, cup.NameEn, body, eventName!);
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

        public async Task<IEnumerable<CupTeamDto>> GetPendingRequestsAsync(int cupId, UserRole role, int centerId)
        {
            var cup = await _cupRepo.GetByIdAsync(cupId)
       ?? throw new NotFoundException("Cup", cupId);
            await RequireAdminOfCup(cup, role, centerId);
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

            ValidateCupStructure(
        dto.Format,
        dto.MaxTeams,
        dto.GroupSize,
        dto.TeamsAdvancingPerGroup);

            if (dto.RegistrationDeadline <= DateTime.UtcNow)
                throw new BusinessException(
                    messageAr: "الموعد النهائي للتسجيل يجب أن يكون في المستقبل.",
                    messageEn: "Registration deadline must be in the future.");

            if (dto.StartDate <= DateTime.UtcNow)
                throw new BusinessException(
                    messageAr: "تاريخ بداية البطولة يجب أن يكون في المستقبل.",
                    messageEn: "Cup start date must be in the future.");

            if (dto.StartDate <= dto.RegistrationDeadline)
                throw new BusinessException(
                    messageAr: "تاريخ بداية البطولة يجب أن يكون بعد الموعد النهائي للتسجيل.",
                    messageEn: "Start date must be after the registration deadline.");

            if (dto.EntryFee < 0)
                throw new BusinessException(
                    messageAr: "رسوم الاشتراك لا يمكن أن تكون سالبة.",
                    messageEn: "Entry fee cannot be negative.");
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

            bool registrationHasStarted =
     cup.Status == CupStatus.RegistrationOpen ||
     cup.Status == CupStatus.GroupStage ||
     cup.Status == CupStatus.KnockoutStage;

            if (!string.IsNullOrWhiteSpace(dto.NameAr))
                cup.NameAr = dto.NameAr.Trim();

            if (!string.IsNullOrWhiteSpace(dto.NameEn))
                cup.NameEn = dto.NameEn.Trim();

            if (!registrationHasStarted)
            {
                var finalMaxTeams = dto.MaxTeams ?? cup.MaxTeams;
                var finalGroupSize = dto.GroupSize ?? cup.GroupSize;
                var finalAdvancing =
                    dto.TeamsAdvancingPerGroup ?? cup.TeamsAdvancingPerGroup;
                var finalFormat = dto.Format ?? cup.Format;

                ValidateCupStructure(
                    finalFormat,
                    finalMaxTeams,
                    finalGroupSize,
                    finalAdvancing);

                cup.MaxTeams = finalMaxTeams;
                cup.GroupSize = finalGroupSize;
                cup.TeamsAdvancingPerGroup = finalAdvancing;
                cup.Format = finalFormat;
            }
            else
            {
                bool structureChanged =
                    (dto.MaxTeams.HasValue && dto.MaxTeams.Value != cup.MaxTeams) ||
                    (dto.GroupSize.HasValue && dto.GroupSize.Value != cup.GroupSize) ||
                    (dto.TeamsAdvancingPerGroup.HasValue &&
                     dto.TeamsAdvancingPerGroup.Value != cup.TeamsAdvancingPerGroup) ||
                    (dto.Format.HasValue && dto.Format.Value != cup.Format);

                if (structureChanged)
                    throw new BusinessException(
                        messageAr: "لا يمكن تغيير إعدادات نظام البطولة أو عدد الفرق أو حجم المجموعات بعد بدء التسجيل.",
                        messageEn: "Cup format, team count and group settings cannot be changed after registration starts.");
            }
            bool scheduleEditable = cup.Status is CupStatus.Draft
    or CupStatus.RegistrationOpen or CupStatus.GroupStage;

            if (scheduleEditable)
            {
                cup.ScheduleMode = dto.ScheduleMode ?? cup.ScheduleMode;
                cup.DailyStartTime = dto.DailyStartTime ?? cup.DailyStartTime;
                cup.DailyEndTime = dto.DailyEndTime ?? cup.DailyEndTime;
                cup.MatchDurationMinutes = dto.MatchDurationMinutes ?? cup.MatchDurationMinutes;
                cup.BreakMinutes = dto.BreakMinutes ?? cup.BreakMinutes;
                ValidateSchedule(cup.ScheduleMode, cup.DailyStartTime, cup.DailyEndTime,
                                 cup.MatchDurationMinutes, cup.BreakMinutes);
            }
            bool datesEditable =
     cup.Status == CupStatus.Draft ||
     cup.Status == CupStatus.RegistrationOpen;

            if (datesEditable)
            {
                var finalDeadline =
                    dto.RegistrationDeadline ?? cup.RegistrationDeadline;

                var finalStartDate =
                    dto.StartDate ?? cup.StartDate;

                if (dto.RegistrationDeadline.HasValue &&
                    finalDeadline <= DateTime.UtcNow)
                {
                    throw new BusinessException(
                        messageAr: "الموعد النهائي للتسجيل يجب أن يكون في المستقبل.",
                        messageEn: "Registration deadline must be in the future.");
                }

                if (dto.StartDate.HasValue &&
                    finalStartDate <= DateTime.UtcNow)
                {
                    throw new BusinessException(
                        messageAr: "تاريخ البداية يجب أن يكون في المستقبل.",
                        messageEn: "Start date must be in the future.");
                }

                if (finalDeadline != default &&
                    finalStartDate != default &&
                    finalStartDate <= finalDeadline)
                {
                    throw new BusinessException(
                        messageAr: "تاريخ البداية يجب أن يكون بعد الموعد النهائي للتسجيل.",
                        messageEn: "Start date must be after the registration deadline.");
                }

                if (dto.RegistrationDeadline.HasValue)
                    cup.RegistrationDeadline = finalDeadline;

                if (dto.StartDate.HasValue)
                    cup.StartDate = finalStartDate;

                if (dto.EntryFee.HasValue)
                {
                    if (dto.EntryFee.Value < 0)
                        throw new BusinessException(
                            messageAr: "رسوم الاشتراك لا يمكن أن تكون سالبة.",
                            messageEn: "Entry fee cannot be negative.");

                    cup.EntryFee = dto.EntryFee.Value == 0
                        ? null
                        : dto.EntryFee.Value;
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
            if (cup.RegistrationDeadline <= DateTime.UtcNow)
                throw new BusinessException(
                    messageAr: "انتهت فترة التسجيل في هذه البطولة.",
                    messageEn: "Registration deadline has passed.");

            if (string.IsNullOrWhiteSpace(teamName))
                throw new BusinessException(
                    messageAr: "يجب إدخال اسم الفريق.",
                    messageEn: "Team name is required.");

            if (teamName.Length > 100)
                throw new BusinessException(
                    messageAr: "اسم الفريق يجب ألا يتجاوز 100 حرف.",
                    messageEn: "Team name cannot exceed 100 characters.");
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
                TeamName = teamName,
                Status = CupTeamStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            await _cupRepo.CreateTeamAsync(team);

            // Direct notification to the admin who owns the cup — not a topic
            // broadcast, since only that admin needs to act on it.
            await notificationService.SendToUserAsync(
     userId: cup.CreatedByAdminId,
     title: cup.NameEn,
     body: $"A new team has requested to join the \"{cup.NameEn}\" tournament. Review the request to approve or reject it.",
     type: "cup", targetId: cup.Id);
            return _mapper.Map<CupTeamDto>(team);
        }
        #endregion


        #region  Add Teams by admin 
        public async Task<CupTeamDto> AddTeamByAdminAsync(
    int cupId, string teamName, int? captainUserId,
    UserRole callerRole, int callerYouthCenterId)
        {
            CupTeam? created = null;
            Cup? updatedCup = null;
            bool filled = false;

            await _cupRepo.ExecuteInTransactionAsync(async () =>
            {
                var cup = await _cupRepo.GetByIdForUpdateAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة.",
                        messageEn: "Cup not found.");

                await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

                if (cup.Status != CupStatus.RegistrationOpen && cup.Status != CupStatus.Draft)
                    throw new BusinessException(
                        "لا يمكن إضافة فرق في هذه المرحلة.",
                        "Teams can only be added during Draft or RegistrationOpen.");

                if (cup.CurrentTeamCount >= cup.MaxTeams)
                    throw new BusinessException("البطولة ممتلئة.", "The cup is full.");

                if (string.IsNullOrWhiteSpace(teamName) || teamName.Length > 100)
                    throw new BusinessException("اسم الفريق غير صالح.", "Invalid team name.");

                // الكابتن اختياري: لو مش محدد نستخدم الأدمن نفسه كمالك مؤقت
                var team = new CupTeam
                {
                    CupId = cupId,
                    CaptainUserId = captainUserId,
                    TeamName = teamName.Trim(),
                    Status = CupTeamStatus.Pending,
                    RegisteredAt = DateTime.UtcNow
                };
                created = await _cupRepo.CreateTeamAsync(team);

                updatedCup = await _cupRepo.AcceptTeamAndIncrementCountAsync(created.Id);

                if (updatedCup.CurrentTeamCount == updatedCup.MaxTeams)
                {
                    await GenerateGroupsAsync(updatedCup.Id);
                    await GenerateGroupMatchesAsync(updatedCup.Id);
                    updatedCup.Status = CupStatus.GroupStage;
                    await _cupRepo.UpdateAsync(updatedCup);
                    filled = true;
                }
            });

            if (filled) await NotifyCupStatusChangeAsync(updatedCup!);
            return _mapper.Map<CupTeamDto>(created);
        }
        #endregion


        #region Accept And REject
        public async Task AcceptJoinRequestAsync(
       int teamId,
       UserRole callerRole,
       int callerYouthCenterId)
        {
            // نحتاج CupId فقط لتحديد السجل الذي سنحجزه.
            var cupId = await _cupRepo.GetCupIdByTeamIdAsync(teamId)
                ?? throw new NotFoundException(
                    messageAr: $"لم يتم العثور على الفريق بالمعرف ({teamId}).",
                    messageEn: $"Team ({teamId}) was not found.");

            Cup? updatedCup = null;
            CupTeam? acceptedTeam = null;
            bool cupCompleted = false;

            await _cupRepo.ExecuteInTransactionAsync(async () =>
            {
                // يجب أن يكون أول قراءة داخل المعاملة هي قفل البطولة.
                var cup = await _cupRepo.GetByIdForUpdateAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المطلوبة.",
                        messageEn: "Cup not found.");

                await RequireAdminOfCup(
                    cup,
                    callerRole,
                    callerYouthCenterId);

                // نقرأ الفريق بعد الحصول على القفل، وليس قبله.
                var team = await _cupRepo.GetTeamByIdAsync(teamId)
                    ?? throw new NotFoundException(
                        messageAr: $"لم يتم العثور على الفريق بالمعرف ({teamId}).",
                        messageEn: $"Team ({teamId}) was not found.");

                if (team.CupId != cupId)
                    throw new BusinessException(
                        messageAr: "الفريق لا ينتمي إلى هذه البطولة.",
                        messageEn: "The team does not belong to this cup.");

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

                updatedCup = await _cupRepo.AcceptTeamAndIncrementCountAsync(teamId);
                acceptedTeam = team;

                if (updatedCup.CurrentTeamCount == updatedCup.MaxTeams)
                {
                    await GenerateGroupsAsync(updatedCup.Id);
                    await GenerateGroupMatchesAsync(updatedCup.Id);

                    updatedCup.Status = CupStatus.GroupStage;
                    await _cupRepo.UpdateAsync(updatedCup);

                    cupCompleted = true;
                }
            });

            if (acceptedTeam?.CaptainUserId != null) await notificationService.SendToUserAsync(
                 userId: acceptedTeam?.CaptainUserId ?? 0,
                 title: updatedCup!.NameEn,
                 body: $"Your team's request to join the \"{updatedCup.NameEn}\" tournament has been approved. Good luck!",
                 type: "cup",
                 targetId: updatedCup.Id);

            if (cupCompleted)
            {
                await NotifyCupStatusChangeAsync(updatedCup!);
            }
        }

        public async Task RejectJoinRequestAsync(
     int teamId,
     UserRole callerRole,
     int callerYouthCenterId)
        {
            var cupId = await _cupRepo.GetCupIdByTeamIdAsync(teamId)
                ?? throw new NotFoundException(
                    messageAr: $"لم يتم العثور على الفريق بالمعرف ({teamId}).",
                    messageEn: $"Team ({teamId}) was not found.");

            Cup? cupForNotification = null;
            CupTeam? rejectedTeam = null;

            await _cupRepo.ExecuteInTransactionAsync(async () =>
            {
                var cup = await _cupRepo.GetByIdForUpdateAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة المطلوبة.",
                        messageEn: "Cup not found.");

                await RequireAdminOfCup(
                    cup,
                    callerRole,
                    callerYouthCenterId);

                var team = await _cupRepo.GetTeamByIdAsync(teamId)
                    ?? throw new NotFoundException(
                        messageAr: $"لم يتم العثور على الفريق بالمعرف ({teamId}).",
                        messageEn: $"Team ({teamId}) was not found.");

                if (team.CupId != cupId)
                    throw new BusinessException(
                        messageAr: "الفريق لا ينتمي إلى هذه البطولة.",
                        messageEn: "The team does not belong to this cup.");

                if (team.Status != CupTeamStatus.Pending)
                    throw new BusinessException(
                        messageAr: $"يمكن رفض الطلبات المعلقة فقط. الحالة الحالية: '{team.Status}'.",
                        messageEn: $"Only pending requests can be rejected. Current status: '{team.Status}'.");

                await _cupRepo.RejectTeamAsync(teamId);

                cupForNotification = cup;
                rejectedTeam = team;
            });

            if (rejectedTeam?.CaptainUserId != null) await notificationService.SendToUserAsync(
                     userId: rejectedTeam?.CaptainUserId ?? 0,
                     title: cupForNotification!.NameEn,
                     body: $"Your team's request to join the \"{cupForNotification.NameEn}\" tournament has been declined.",
                     type: "cup",
                     targetId: cupForNotification.Id);
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

                await _cupRepo.EnterMatchResultAsync(
                    matchId: matchId,
                    scoreA: dto.ScoreA,
                    scoreB: dto.ScoreB,
                    winnerId: winnerId);
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
      matchId: matchId,
      scoreA: dto.ScoreA,
      scoreB: dto.ScoreB,
      penaltyScoreA: dto.PenaltyScoreA,
      penaltyScoreB: dto.PenaltyScoreB,
      winnerId: winnerId);

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
            var end = start.AddMinutes(cup.MatchDurationMinutes);
            if (end <= start)   // عدّى منتصف الليل
                throw new BusinessException("المباراة لا يمكن أن تتخطى منتصف الليل.", "A match cannot run past midnight.");

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
            match.IsManuallyScheduled = true;
            match.Status = CupMatchStatus.Scheduled;
            await _cupRepo.UpdateMatchAsync(match);



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
            if (cup.ScheduleMode == CupScheduleMode.Automatic)
                await scheduler.ScheduleAsync(cup);
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

        private static void ValidateCupStructure(
CupFormat format,
int maxTeams,
int groupSize,
int teamsAdvancingPerGroup)
        {
            if (maxTeams < 2)
                throw new BusinessException(
                    messageAr: "يجب أن يكون عدد الفرق فريقين على الأقل.",
                    messageEn: "MaxTeams must be at least 2.");

            if (format == CupFormat.SingleElimination)
            {
                // Current generation logic always creates groups.
                // Do not allow this format until its dedicated bracket generation is implemented.
                throw new BusinessException(
                    messageAr: "نظام خروج المغلوب المباشر غير مدعوم حاليًا.",
                    messageEn: "Single elimination format is not supported yet.");
            }

            if (format != CupFormat.RoundRobin)
                throw new BusinessException(
                    messageAr: "نظام البطولة غير صالح.",
                    messageEn: "Invalid cup format.");

            if (groupSize < 2 || groupSize > maxTeams)
                throw new BusinessException(
                    messageAr: "حجم المجموعة يجب أن يكون بين 2 وعدد الفرق الكلي.",
                    messageEn: "GroupSize must be between 2 and MaxTeams.");

            if (maxTeams % groupSize != 0)
                throw new BusinessException(
                    messageAr: "عدد الفرق يجب أن يقبل القسمة على حجم المجموعة دون باقٍ.",
                    messageEn: "MaxTeams must be divisible by GroupSize.");

            if (teamsAdvancingPerGroup < 1 ||
                teamsAdvancingPerGroup >= groupSize)
                throw new BusinessException(
                    messageAr: "عدد المتأهلين يجب أن يكون واحدًا على الأقل وأقل من حجم المجموعة.",
                    messageEn: "TeamsAdvancingPerGroup must be at least 1 and less than GroupSize.");

            int groupCount = maxTeams / groupSize;
            int knockoutTeams = groupCount * teamsAdvancingPerGroup;

            bool isPowerOfTwo =
                knockoutTeams >= 2 &&
                (knockoutTeams & (knockoutTeams - 1)) == 0;

            if (!isPowerOfTwo)
                throw new BusinessException(
                    messageAr: $"عدد المتأهلين للأدوار الإقصائية ({knockoutTeams}) يجب أن يكون قوة للعدد 2.",
                    messageEn: $"The number of knockout teams ({knockoutTeams}) must be a power of 2.");
        }

        public Task CancelMyJoinRequestAsync(int cupId, int userId)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region  Start Cup
        public async Task StartCupAsync(int cupId, UserRole callerRole, int callerYouthCenterId)
        {
            Cup? started = null;

            await _cupRepo.ExecuteInTransactionAsync(async () =>
            {
                var cup = await _cupRepo.GetByIdForUpdateAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة.", messageEn: "Cup not found.");

                await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

                if (cup.Status != CupStatus.RegistrationOpen)
                    throw new BusinessException(
                        messageAr: "يمكن بدء البطولة فقط أثناء فترة التسجيل.",
                        messageEn: "A cup can only be started while registration is open.");

                var accepted = (await _cupRepo.GetAcceptedTeamsByCupAsync(cupId)).ToList();

                // يتحقق: القسمة على حجم المجموعة + المتأهلين قوة 2
                ValidateCupStructure(cup.Format, accepted.Count, cup.GroupSize, cup.TeamsAdvancingPerGroup);

                // الطلبات المعلقة تتقفل
                foreach (var p in (await _cupRepo.GetPendingTeamsByCupAsync(cupId)).ToList())
                    await _cupRepo.RejectTeamAsync(p.Id);

                cup.MaxTeams = accepted.Count;
                cup.CurrentTeamCount = accepted.Count;
                cup.Status = CupStatus.GroupStage;
                await _cupRepo.UpdateAsync(cup);

                await GenerateGroupsAsync(cupId);
                await GenerateGroupMatchesAsync(cupId);
                started = cup;
            });

            await NotifyCupStatusChangeAsync(started!);
        }
        #endregion

        public async Task RemoveTeamAsync(int cupId, int teamId, UserRole callerRole, int callerYouthCenterId)
        {
            CupTeam? removed = null;
            Cup? cupRef = null;

            await _cupRepo.ExecuteInTransactionAsync(async () =>
            {
                var cup = await _cupRepo.GetByIdForUpdateAsync(cupId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على البطولة.", messageEn: "Cup not found.");

                await RequireAdminOfCup(cup, callerRole, callerYouthCenterId);

                if (cup.Status != CupStatus.Draft && cup.Status != CupStatus.RegistrationOpen)
                    throw new BusinessException(
                        messageAr: "لا يمكن حذف فريق بعد بدء البطولة.",
                        messageEn: "Teams cannot be removed after the cup has started.");

                var team = await _cupRepo.GetTeamByIdAsync(teamId)
                    ?? throw new NotFoundException(
                        messageAr: "لم يتم العثور على الفريق.", messageEn: "Team not found.");

                if (team.CupId != cupId)
                    throw new BusinessException(
                        messageAr: "الفريق لا ينتمي لهذه البطولة.",
                        messageEn: "The team does not belong to this cup.");

                if (team.Status == CupTeamStatus.Accepted && cup.CurrentTeamCount > 0)
                {
                    cup.CurrentTeamCount--;
                    await _cupRepo.UpdateAsync(cup);
                }

                await _cupRepo.DeleteTeamAsync(teamId);
                removed = team;
                cupRef = cup;
            });

            if (removed!.CaptainUserId != cupRef!.CreatedByAdminId)
                await notificationService.SendToUserAsync(
                    userId: removed.CaptainUserId ?? 0,
                    title: cupRef.NameEn,
                    body: $"Your team \"{removed.TeamName}\" was removed from \"{cupRef.NameEn}\".",
                    type: "cup", targetId: cupRef.Id);
        }

        private static void ValidateSchedule(
    CupScheduleMode mode, TimeOnly? start, TimeOnly? end, int duration, int brk)
        {
            if (duration < 10 || duration > 240)
                throw new BusinessException("مدة المباراة غير صالحة.", "Match duration must be between 10 and 240 minutes.");
            if (brk < 0 || brk > 120)
                throw new BusinessException("مدة الاستراحة غير صالحة.", "Break must be between 0 and 120 minutes.");

            if (mode != CupScheduleMode.Automatic) return;

            if (start is null || end is null)
                throw new BusinessException("حدد وقت البداية والنهاية للجدولة التلقائية.", "Start and end times are required for automatic scheduling.");
            if (end <= start)
                throw new BusinessException("وقت النهاية يجب أن يكون بعد وقت البداية.", "End time must be after start time.");
            if ((end.Value - start.Value).TotalMinutes < duration)
                throw new BusinessException("الفترة اليومية أقصر من مدة المباراة.", "The daily window is shorter than a single match.");
        }

        public async Task<int> RegenerateScheduleAsync(
    int cupId, bool overwriteManual, UserRole role, int centerId)
        {
            int count = 0;
            await _cupRepo.ExecuteInTransactionAsync(async () =>
            {
                var cup = await _cupRepo.GetByIdForUpdateAsync(cupId)
                    ?? throw new NotFoundException("Cup", cupId);
                await RequireAdminOfCup(cup, role, centerId);

                if (cup.Status is not (CupStatus.GroupStage or CupStatus.KnockoutStage))
                    throw new BusinessException(
                        "الجدول بيتولد بعد بدء البطولة فقط.",
                        "The schedule can only be generated after the cup has started.");

                cup.ScheduleMode = CupScheduleMode.Automatic;   // يسمح بالتحويل من Manual
                ValidateSchedule(cup.ScheduleMode, cup.DailyStartTime, cup.DailyEndTime,
                                 cup.MatchDurationMinutes, cup.BreakMinutes);
                await _cupRepo.UpdateAsync(cup);

                count = await scheduler.ScheduleAsync(cup, overwriteManual);
            });
            return count;
        }
    }



}