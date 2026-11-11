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

        /// PlayDaysMask: Sunday = bit0 ... Saturday = bit6
        public static bool IsPlayDay(Cup cup, DateTime day) =>
            ((cup.PlayDaysMask >> (int)day.DayOfWeek) & 1) == 1;

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

            if (toSchedule.Count == 0) return 0;

            // فك حجز المواعيد القديمة أولاً، وإلا المباريات هتتعارض مع نفسها
            var stale = toSchedule.Where(m => m.ScheduledAt != null).ToList();
            if (stale.Count > 0)
            {
                foreach (var m in stale) m.ScheduledAt = null;
                await cupRepo.UpdateMatchesAsync(stale);
            }

            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CairoTz);
            var cursor = cup.StartDate.Date > now.Date ? cup.StartDate.Date : now;

            // الخطة بتتبني في الذاكرة وتتطبق مرة واحدة في الآخر،
            // فلو فشلت في النص ما يتحفظش جدول ناقص
            var plan = new List<(CupMatch Match, DateTime At)>();

            async Task Place(CupMatch m)
            {
                var at = await FindSlotAsync(cup, cursor, now, plan);
                plan.Add((m, at));
                cursor = at.AddMinutes(
                    cup.MatchDurationMinutes +
                    cup.HalftimeMinutes +
                    cup.BreakBetweenMatchesMinutes);
            }

            // 1) دور المجموعات
            var groupOrdered = OrderGroupMatches(
                toSchedule.Where(m => m.Stage == CupMatchStage.GroupStage).ToList());

            foreach (var m in groupOrdered)
                await Place(m);

            // 2) الإقصائيات: كل دور في يوم جديد + أيام الراحة
            int? lastBucket = null;
            foreach (var m in toSchedule
                         .Where(m => m.Stage != CupMatchStage.GroupStage)
                         .OrderBy(m => m.Stage)
                         .ThenBy(m => m.MatchNumber))
            {
                // المركز الثالث والنهائي في نفس اليوم
                var bucket = m.Stage == CupMatchStage.ThirdPlace
                    ? (int)CupMatchStage.FinalMatch
                    : (int)m.Stage;

                bool newRound = lastBucket != null
                    ? bucket != lastBucket
                    : groupOrdered.Count > 0;

                if (newRound)
                    cursor = cursor.Date.AddDays(1 + cup.RestDaysBetweenRounds);

                lastBucket = bucket;
                await Place(m);
            }

            foreach (var (m, at) in plan)
            {
                m.ScheduledAt = at;
                m.Status = CupMatchStatus.Scheduled;
                m.IsManuallyScheduled = false;
            }

            await cupRepo.UpdateMatchesAsync(plan.Select(p => p.Match).ToList());
            return plan.Count;
        }

        // ─────────────────────────────────────────────────────────────────────
        // إيجاد أول slot فاضي بعد الـ cursor
        // ─────────────────────────────────────────────────────────────────────
        private async Task<DateTime> FindSlotAsync(
            Cup cup,
            DateTime cursor,
            DateTime now,
            List<(CupMatch Match, DateTime At)> plan)
        {
            var slot = TimeSpan.FromMinutes(cup.MatchDurationMinutes + cup.HalftimeMinutes);
            var step = slot + TimeSpan.FromMinutes(cup.BreakBetweenMatchesMinutes);
            var dayStart = cup.DailyStartTime!.Value.ToTimeSpan();
            var dayEnd = cup.DailyEndTime!.Value.ToTimeSpan();

            var day = cursor.Date;
            for (int i = 0; i < MaxDaysAhead; i++, day = day.AddDays(1))
            {
                if (!IsPlayDay(cup, day)) continue;

                for (var s = day + dayStart; s + slot <= day + dayEnd; s += step)
                {
                    if (s < cursor || s < now) continue;

                    // تعارض مع مباريات الخطة اللي لسه ما اتحفظتش
                    // (المباريات دي ما بتظهرش في الـ DB فـ HasConflictAsync مش شايفها)
                    if (plan.Any(p => p.At < s + slot && p.At + slot > s))
                        continue;

                    var conflict = await availability.HasConflictAsync(
                        cup.YouthCenterActivityId,
                        day,
                        TimeOnly.FromDateTime(s),
                        TimeOnly.FromDateTime(s + slot));

                    if (!conflict) return s;
                }
            }

            throw new BusinessException(
                "تعذر إيجاد مواعيد متاحة كافية خلال الفترة القادمة.",
                "Could not find enough free time slots in the coming days.");
        }

        // ─────────────────────────────────────────────────────────────────────
        // ترتيب مباريات المجموعات: الفريق ما يلعبش مرتين ورا بعض،
        // ويفضّل تبديل المجموعات
        // ─────────────────────────────────────────────────────────────────────
        private static List<CupMatch> OrderGroupMatches(List<CupMatch> matches)
        {
            var remaining = matches
                .OrderBy(m => m.GroupId)
                .ThenBy(m => m.Round)
                .ThenBy(m => m.MatchNumber)
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