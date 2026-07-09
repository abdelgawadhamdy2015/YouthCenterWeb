using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories
{
    public class AvailabilityRepo(AppDbContext context) : IAvailabilityRepo
    {
        private readonly AppDbContext _context = context;

        // Matches don't have a Duration field yet — treated as fixed 1-hour
        // slots, same as a reservation. Change this if match duration ever
        // becomes configurable per activity.
        private static readonly TimeSpan MatchDuration = TimeSpan.FromHours(1);

        public async Task<bool> HasConflictAsync(
            int youthCenterActivityId,
            DateTime date,
            TimeOnly start,
            TimeOnly end,
            int? excludeReservationId = null,
            int? excludeMatchId = null)
        {
            var targetDate = date.Date;
            var now = DateTime.UtcNow;

            // ── 1) تعارض مع حجز موجود (مؤكد أو مقفول لحين الدفع) ──────────────
            var reservationConflict = await _context.Reservations.AnyAsync(x =>
                x.YouthCenterActivityId == youthCenterActivityId &&
                x.Date.Date == targetDate &&
                x.Id != excludeReservationId &&
                x.StartTime < end && x.EndTime > start &&
                (x.Status == ReservationStatus.Confirmed ||
                 (x.Status == ReservationStatus.AwaitingPayment && x.PaymentLockExpiresAt > now)));

            if (reservationConflict) return true;

            // ── 2) تعارض مع مباراة مجدولة على نفس النشاط ──────────────────────
            var matches = await _context.CupMatches
                .Where(m =>
                    m.Cup.YouthCenterActivityId == youthCenterActivityId &&
                    m.Id != excludeMatchId &&
                    m.ScheduledAt != null &&
                    m.ScheduledAt.Value.Date == targetDate &&
                    m.Status != CupMatchStatus.Completed) // مباراة منتهية مش هتتعاد جدولتها، لكن ساعتها لسه محجوزة تاريخياً — لو عايز تستثنيها برضو شيل الشرط ده
                .Select(m => m.ScheduledAt!.Value)
                .ToListAsync();

            foreach (var scheduledAt in matches)
            {
                var matchStart = TimeOnly.FromDateTime(scheduledAt);
                var matchEnd = matchStart.Add(MatchDuration);

                if (matchStart < end && matchEnd > start)
                    return true;
            }

            return false;
        }
    }
}