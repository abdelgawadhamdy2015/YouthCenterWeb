using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extensions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class ReservationRepo(AppDbContext context) : IReservationRepo
{
    private readonly AppDbContext _context = context;


    private IQueryable<Reservation> WithRelations() =>
        _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenterActivity)
                .ThenInclude(x => x!.YouthCenter)
            .Include(x => x.YouthCenterActivity)
                .ThenInclude(x => x!.Activity);

    // ── queries ───────────────────────────────────────────────────────────────

    public Task<Reservation?> GetByIdWithRelationsAsync(int id) =>
        WithRelations().FirstOrDefaultAsync(x => x.Id == id);

    public Task<YouthCenterActivity?> GetYouthCenterActivity(int youthCenterActivityId) =>
        _context.YouthCenterActivities
            .Include(y => y.YouthCenter)
            .Include(y => y.Activity)
            .FirstOrDefaultAsync(y => y.Id == youthCenterActivityId);

    public Task<List<int>> GetCenterAdminIdsAsync(int youthCenterId) =>
        _context.Users
            .Where(u => u.YouthCenterId == youthCenterId
                     && u.Role == UserRole.Admin
                     && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync();

    // ── writes ────────────────────────────────────────────────────────────────
    public async Task AddAsync(Reservation entity) =>
        await _context.Reservations.AddAsync(entity);

    public void Delete(Reservation entity) =>
        _context.Reservations.Remove(entity);

    public void Update(Reservation entity) =>
        _context.Reservations.Update(entity);

    public Task SaveChangesAsync() =>
        _context.SaveChangesAsync();

    // ── availability ──────────────────────────────────────────────────────────
    public async Task<bool> HasActiveOverlapAsync(
        int activityId, DateTime date, TimeOnly start, TimeOnly end)
    {
        var now = DateTime.UtcNow;

        return await _context.Reservations.AnyAsync(x =>
            x.YouthCenterActivityId == activityId &&
            x.Date.Date == date.Date &&
            x.StartTime < end && x.EndTime > start &&
            (
                x.Status == ReservationStatus.Confirmed ||
                x.Status == ReservationStatus.Pending ||
                (x.Status == ReservationStatus.AwaitingPayment &&
                 x.PaymentLockExpiresAt > now)
            ));
    }

    public async Task ExpireStaleLocksAsync()
    {
        var now = DateTime.UtcNow;
        await _context.Reservations
            .Where(x => x.Status == ReservationStatus.AwaitingPayment &&
                        x.PaymentLockExpiresAt <= now)
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Status, ReservationStatus.Cancelled)
                .SetProperty(x => x.CancelledAt, now)
                .SetProperty(x => x.PaymentLockExpiresAt, (DateTime?)null));
    }

    // ── statistics ────────────────────────────────────────────────────────────
    public async Task<int> GetCount(FilteredReservationDto? dto)
    {
        var query = WithRelations().AsQueryable();

        if (dto?.Status != null)
            query = query.Where(x => x.Status == dto.Status);
        if (dto?.DateFrom != null)
            query = query.Where(x => x.Date >= dto.DateFrom);
        if (dto?.DateTo != null)
            query = query.Where(x => x.Date <= dto.DateTo);
        if (dto?.YouthCenterId != null)
            query = query.Where(x => x.YouthCenterActivity!.YouthCenterId == dto.YouthCenterId);
        if (dto?.UserId != null)
            query = query.Where(x => x.UserId == dto.UserId);

        return await query.CountAsync();
    }

    public async Task<int> GetActivityCount(FilteredReservationDto? dto)
    {
        var query = WithRelations().AsQueryable();

        if (dto?.YouthCenterId != null)
            query = query.Where(x => x.YouthCenterActivity!.YouthCenterId == dto.YouthCenterId);
        if (dto?.UserId != null)
            query = query.Where(x => x.UserId == dto.UserId);
        if (dto?.ActivityId != null)
            query = query.Where(x => x.YouthCenterActivity!.ActivityId == dto.ActivityId);

        return await query.CountAsync();
    }

    public async Task<List<PeakHourDto>> GetPeakBookingHoursAsync(FilteredReservationDto? dto)
    {
        var query = WithRelations().AsQueryable();

        if (dto?.DateFrom != null)
            query = query.Where(x => x.Date >= dto.DateFrom);
        if (dto?.DateTo != null)
            query = query.Where(x => x.Date <= dto.DateTo);
        if (dto?.YouthCenterId != null)
            query = query.Where(x => x.YouthCenterActivity!.YouthCenterId == dto.YouthCenterId);
        if (dto?.UserId != null)
            query = query.Where(x => x.UserId == dto.UserId);

        return await query
            .GroupBy(x => x.StartTime.Hour)
            .Select(g => new PeakHourDto { Hour = g.Key, BookingCount = g.Count() })
            .OrderBy(x => x.Hour)
            .ToListAsync();
    }


    public async Task<List<PeakHourDto>> GetAvailBookingHoursAsync(AvailBookingDto? dto)
    {
        var query = WithRelations().AsQueryable();

        if (dto?.YouthCenterActivityId != null)
            query = query.Where(x => x.YouthCenterActivityId == dto.YouthCenterActivityId);

        if (dto?.Date != null)
        {
            var targetDate = dto.Date.Date;
            query = query.Where(x => x.Date.Date == targetDate);
        }

        var now = DateTime.UtcNow;

        var reservations = await query
            .Where(x => x.Status == ReservationStatus.Confirmed ||
                        x.Status == ReservationStatus.Pending ||
                        (x.Status == ReservationStatus.AwaitingPayment &&
                         x.PaymentLockExpiresAt > now))
            .Select(x => new { StartHour = x.StartTime.Hour, EndHour = x.EndTime.Hour })
            .ToListAsync();

        var hourCounts = reservations
            .SelectMany(r =>
            {
                int start = r.StartHour;
                int end = r.EndHour == 0 ? 24 : r.EndHour;
                return Enumerable.Range(start, end - start).Select(h => h % 24);
            })
            .GroupBy(h => h)
            .ToDictionary(g => g.Key, g => g.Count());

        if (dto != null)
        {
            var targetDate = dto.Date.Date;

            var matchSlots = await _context.CupMatches
     .Where(m => m.Cup.YouthCenterActivityId == dto.YouthCenterActivityId
              && m.ScheduledAt != null
              && m.ScheduledAt.Value.Date == targetDate
              && m.Status != CupMatchStatus.Completed)
     .Select(m => new { m.ScheduledAt, Minutes = m.Cup.MatchDurationMinutes + m.Cup.HalftimeMinutes })
     .ToListAsync();

            foreach (var m in matchSlots)
            {
                var s = m.ScheduledAt!.Value;
                var e = s.AddMinutes(m.Minutes);
                for (var h = new DateTime(s.Year, s.Month, s.Day, s.Hour, 0, 0); h < e; h = h.AddHours(1))
                    hourCounts[h.Hour] = hourCounts.GetValueOrDefault(h.Hour) + 1;
            }
        }

        return Enumerable.Range(0, 24)
            .Select(hour => new PeakHourDto
            {
                Hour = hour,
                BookingCount = hourCounts.GetValueOrDefault(hour, 0)
            })
            .ToList();
    }


    public Task<List<Reservation>> GetRefundsAsync(int? youthCenterId, bool pendingOnly)
    {
        var q = WithRelations().Where(r => r.RefundPending || r.RefundedAt != null);
        if (youthCenterId != null)
            q = q.Where(r => r.YouthCenterActivity!.YouthCenterId == youthCenterId);
        if (pendingOnly)
            q = q.Where(r => r.RefundPending);
        return q.OrderByDescending(r => r.CancelledAt ?? r.PaidAt).Take(200).ToListAsync();
    }

    private IQueryable<Reservation> Scoped(FilteredReservationDto? dto)
    {
        var q = _context.Reservations.AsNoTracking().AsQueryable();
        if (dto?.YouthCenterId != null)
            q = q.Where(x => x.YouthCenterActivity!.YouthCenterId == dto.YouthCenterId);
        if (dto?.UserId != null)
            q = q.Where(x => x.UserId == dto.UserId);
        if (dto?.DateFrom != null) q = q.Where(x => x.Date >= dto.DateFrom);
        if (dto?.DateTo != null) q = q.Where(x => x.Date <= dto.DateTo);
        return q;
    }

    public Task<Dictionary<ReservationStatus, int>> GetStatusCountsAsync(FilteredReservationDto? dto) =>
        Scoped(dto)
            .GroupBy(x => x.Status)
            .Select(g => new { g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count);

    public Task<List<ActivityBreakdownDto>> GetActivityBreakdownAsync(FilteredReservationDto? dto) =>
        Scoped(dto)
            .GroupBy(x => new { x.YouthCenterActivity!.ActivityId, x.YouthCenterActivity.Activity.NameAr })
            .Select(g => new ActivityBreakdownDto { ActivityName = g.Key.NameAr, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

    private static IQueryable<Reservation> ApplyFilters(IQueryable<Reservation> query, FilteredReservationDto? dto)
    {
        if (dto?.UserId != null) query = query.Where(x => x.UserId == dto.UserId);
        if (dto?.YouthCenterId != null) query = query.Where(x => x.YouthCenterActivity != null &&
                                            x.YouthCenterActivity.YouthCenterId == dto.YouthCenterId);
        if (dto?.ActivityId != null) query = query.Where(x => x.YouthCenterActivity != null &&
                                            x.YouthCenterActivity.ActivityId == dto.ActivityId);
        if (dto?.Status != null) query = query.Where(x => x.Status == dto.Status);
        if (dto?.DateFrom != null) query = query.Where(x => x.Date >= dto.DateFrom);
        if (dto?.DateTo != null) query = query.Where(x => x.Date <= dto.DateTo);
        if (dto?.StartTime != null) query = query.Where(x => x.StartTime >= dto.StartTime);
        if (dto?.EndTime != null) query = query.Where(x => x.EndTime <= dto.EndTime);
        return query;
    }

    public Task<List<Reservation>> GetFiltersReservationsAsync(FilteredReservationDto? dto)
    {
        var query = ApplyFilters(WithRelations(), dto)
            .OrderByDescending(x => x.Date)
            .ThenByDescending(x => x.StartTime)
            .AsQueryable();

        if (dto?.PageNumber != null && dto.PageSize != null)
        {
            var size = Math.Clamp(dto.PageSize.Value, 1, 100);
            var page = Math.Max(1, dto.PageNumber.Value);
            query = query.Skip((page - 1) * size).Take(size);
        }
        return query.ToListAsync();
    }

    public Task<int> GetFilteredCountAsync(FilteredReservationDto? dto) =>
        ApplyFilters(_context.Reservations.AsNoTracking(), dto).CountAsync();
}