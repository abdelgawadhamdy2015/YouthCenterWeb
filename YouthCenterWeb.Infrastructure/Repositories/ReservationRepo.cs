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

    private static readonly TimeSpan MatchDuration = TimeSpan.FromHours(1);

    private IQueryable<Reservation> WithRelations() =>
        _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenterActivity)
                .ThenInclude(x => x!.YouthCenter)
            .Include(x => x.YouthCenterActivity)
                .ThenInclude(x => x!.Activity);

    // ── queries ───────────────────────────────────────────────────────────────
    public Task<List<Reservation>> GetFiltersReservationsAsync(FilteredReservationDto? dto)
    {
        var query = WithRelations().AsQueryable();

        if (dto?.UserId != null)
            query = query.Where(x => x.UserId == dto.UserId);

        if (dto?.YouthCenterId != null)
            query = query.Where(x =>
                x.YouthCenterActivity != null &&
                x.YouthCenterActivity.YouthCenterId == dto.YouthCenterId);

        if (dto?.ActivityId != null)
            query = query.Where(x =>
                x.YouthCenterActivity != null &&
                x.YouthCenterActivity.ActivityId == dto.ActivityId);

        if (dto?.Status != null)
            query = query.Where(x => x.Status == dto.Status);

        if (dto?.DateFrom != null)
            query = query.Where(x => x.Date >= dto.DateFrom);

        if (dto?.DateTo != null)
            query = query.Where(x => x.Date <= dto.DateTo);

        if (dto?.StartTime != null)
            query = query.Where(x => x.StartTime >= dto.StartTime);

        if (dto?.EndTime != null)
            query = query.Where(x => x.EndTime <= dto.EndTime);

        // ترتيب ثابت قبل الـ Skip/Take (من غيره الترقيم غير مضمون)
        query = query.OrderByDescending(x => x.Date).ThenByDescending(x => x.StartTime);

        if (dto?.PageNumber != null && dto.PageSize != null)
            query = query.Skip((int)((dto.PageNumber - 1) * dto.PageSize))
                         .Take((int)dto.PageSize);

        return query.ToListAsync();
    }

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
        var stale = await _context.Reservations
            .Where(x => x.Status == ReservationStatus.AwaitingPayment &&
                        x.PaymentLockExpiresAt <= now)
            .ToListAsync();

        foreach (var r in stale)
            r.Status = ReservationStatus.Cancelled;

        if (stale.Count > 0)
            await _context.SaveChangesAsync();
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

    /// <summary>
    /// عدد الحجوزات لكل ساعة (حجوزات + مباريات مجدولة). Pending بيقفل الساعة.
    /// </summary>
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

            var matchHours = await _context.CupMatches
                .Where(m =>
                    m.Cup.YouthCenterActivityId == dto.YouthCenterActivityId &&
                    m.ScheduledAt != null &&
                    m.ScheduledAt.Value.Date == targetDate &&
                    m.Status != CupMatchStatus.Completed)
                .Select(m => m.ScheduledAt!.Value)
                .ToListAsync();

            foreach (var scheduledAt in matchHours)
            {
                var matchStart = scheduledAt.Hour;
                var matchEnd = matchStart + (int)MatchDuration.TotalHours;

                foreach (var hour in Enumerable.Range(matchStart, matchEnd - matchStart)
                                               .Select(h => h % 24))
                {
                    hourCounts[hour] = hourCounts.GetValueOrDefault(hour) + 1;
                }
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
}