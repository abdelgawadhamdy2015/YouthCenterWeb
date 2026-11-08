using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.ICupScheduler;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class CupSchedulerService(
        ICupRepo cupRepo,
        IAvailabilityRepo availability) : ICupScheduler
    {
        private const int MaxDaysAhead = 180;

        private static readonly TimeZoneInfo CairoTz = ResolveCairoTz();
        private static TimeZoneInfo ResolveCairoTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); }
        }

        public async Task<int> ScheduleAsync(Cup cup, bool overwriteManual = false)
        {
            if (cup.DailyStartTime is null || cup.DailyEndTime is null)
                throw new BusinessException(
                    "يجب تحديد وقت بداية ونهاية اللعب اليومي.",
                    "Daily start and end times are required.");

            var all = (await cupRepo.GetMatchesByCupAsync(cup.Id)).ToList();

            var toSchedule = all
                .Where(m => m.Status != CupMatchStatus.Completed &&
                            (overwriteManual || !m.IsManuallyScheduled))
                .ToList();

            // فك حجز المواعيد القديمة أولاً، وإلا المباريات هتتعارض مع نفسها
            foreach (var m in toSchedule) m.ScheduledAt = null;
            await cupRepo.UpdateMatchesAsync(toSchedule);

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CairoTz);
            var cursor = cup.StartDate.Date > now.Date ? cup.StartDate.Date : now;

            // 1) مباريات المجموعات بترتيب يمنع تلعب نفس الفرق ورا بعض
            var groupOrdered = OrderGroupMatches(
                toSchedule.Where(m => m.Stage == CupMatchStage.GroupStage).ToList());

            foreach (var m in groupOrdered)
                cursor = await Assign(cup, m, cursor, now);

            // 2) الإقصائيات: كل دور يبدأ في يوم جديد
            int? lastBucket = null;
            foreach (var m in toSchedule
                         .Where(m => m.Stage != CupMatchStage.GroupStage)
                         .OrderBy(m => m.Stage).ThenBy(m => m.MatchNumber))
            {
                var bucket = m.Stage == CupMatchStage.ThirdPlace
                    ? (int)CupMatchStage.FinalMatch   // مركز تالت + نهائي نفس اليوم
                    : (int)m.Stage;

                if (lastBucket != null && bucket != lastBucket)
                    cursor = cursor.Date.AddDays(1);
                lastBucket = bucket;

                cursor = await Assign(cup, m, cursor, now);
            }

            await cupRepo.UpdateMatchesAsync(toSchedule);
            return toSchedule.Count;
        }

        private async Task<DateTime> Assign(Cup cup, CupMatch m, DateTime cursor, DateTime now)
        {
            var start = await FindSlotAsync(cup, cursor, now);
            m.ScheduledAt = start;
            m.Status = CupMatchStatus.Scheduled;
            m.IsManuallyScheduled = false;
            // المباراة اللي بعدها تبدأ بعد (المدة + الاستراحة)
            return start.AddMinutes(cup.MatchDurationMinutes + cup.BreakMinutes);
        }

        private async Task<DateTime> FindSlotAsync(Cup cup, DateTime cursor, DateTime now)
        {
            var duration = TimeSpan.FromMinutes(cup.MatchDurationMinutes);
            var step = TimeSpan.FromMinutes(cup.MatchDurationMinutes + cup.BreakMinutes);
            var dayStart = cup.DailyStartTime!.Value.ToTimeSpan();
            var dayEnd = cup.DailyEndTime!.Value.ToTimeSpan();

            var day = cursor.Date;
            for (int i = 0; i < MaxDaysAhead; i++, day = day.AddDays(1))
            {
                for (var s = day + dayStart; s + duration <= day + dayEnd; s += step)
                {
                    if (s < cursor || s < now) continue;

                    var conflict = await availability.HasConflictAsync(
                        cup.YouthCenterActivityId, day,
                        TimeOnly.FromDateTime(s),
                        TimeOnly.FromDateTime(s + duration));

                    if (!conflict) return s;
                }
            }

            throw new BusinessException(
                "تعذر إيجاد مواعيد متاحة كافية خلال الفترة القادمة.",
                "Could not find enough free time slots in the coming days.");
        }

        /// Greedy: اختار مباراة فرقها ما لعبتش في الـ slot اللي قبله،
        /// ويفضّل مجموعة مختلفة عن اللي قبلها
        private static List<CupMatch> OrderGroupMatches(List<CupMatch> matches)
        {
            var remaining = matches
                .OrderBy(m => m.GroupId).ThenBy(m => m.Round).ThenBy(m => m.MatchNumber)
                .ToList();
            var result = new List<CupMatch>(matches.Count);
            var lastTeams = new HashSet<int>();
            int? lastGroup = null;

            while (remaining.Count > 0)
            {
                var rested = remaining
                    .Where(m => !lastTeams.Contains(m.TeamAId!.Value) &&
                                !lastTeams.Contains(m.TeamBId!.Value))
                    .ToList();

                var pick = rested.FirstOrDefault(m => m.GroupId != lastGroup)
                           ?? rested.FirstOrDefault()
                           ?? remaining[0];

                remaining.Remove(pick);
                result.Add(pick);
                lastTeams = new HashSet<int> { pick.TeamAId!.Value, pick.TeamBId!.Value };
                lastGroup = pick.GroupId;
            }
            return result;
        }
    }
}