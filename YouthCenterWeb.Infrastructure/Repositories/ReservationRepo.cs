using System.Formats.Asn1;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extensions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class ReservationRepo(AppDbContext context) : IReservationRepo
{
    private readonly AppDbContext _context = context;

    // ── shared include chain — one place to maintain ─────────────────────────
    private IQueryable<Reservation> WithRelations() =>
        _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenterActivity)
                .ThenInclude(x => x!.YouthCenter)
            .Include(x => x.YouthCenterActivity)
                .ThenInclude(x => x!.Activity);

    // ── single query entry point — replaces all the Get*Async overloads ───────
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

        if (dto?.PageNumber != null && dto.PageSize != null)
            query = query.Skip((int)((dto.PageNumber - 1) * dto.PageSize))
                .Take((int)dto.PageSize);


        return query.ToListAsync();
    }

    // ── single record ─────────────────────────────────────────────────────────
    public Task<Reservation?> GetByIdWithRelationsAsync(int id) =>
        WithRelations().FirstOrDefaultAsync(x => x.Id == id);

    // ── lookup for activity pricing ───────────────────────────────────────────
    public Task<YouthCenterActivity?> GetYouthCenterActivity(int youthCenterActivityId) =>
        _context.YouthCenterActivities
            .Include(y => y.YouthCenter)
            .Include(y => y.Activity)
            .FirstOrDefaultAsync(y => y.Id == youthCenterActivityId);

    // ── write operations ──────────────────────────────────────────────────────
    public async Task AddAsync(Reservation entity) =>
        await _context.Reservations.AddAsync(entity);

    // pass the already-loaded entity from the service — no second DB hit
    public void Delete(Reservation entity) =>
        _context.Reservations.Remove(entity);

    // called after Accept / Reject / any status change
    public void Update(Reservation entity) =>
        _context.Reservations.Update(entity);

    public Task SaveChangesAsync() =>
        _context.SaveChangesAsync();


    public async Task<bool> HasActiveOverlapAsync(int activityId, DateTime date, TimeOnly start, TimeOnly end)
    {
        var now = DateTime.UtcNow;

        return await _context.Reservations.AnyAsync(x =>
            x.YouthCenterActivityId == activityId &&
            x.Date.Date == date.Date &&
            x.StartTime < end && x.EndTime > start &&
            (
                x.Status == ReservationStatus.Confirmed ||
                (x.Status == ReservationStatus.AwaitingPayment && x.PaymentLockExpiresAt > now)
            ));
    }

    // Called by a background job every ~1 min, and also opportunistically before any lock attempt
    public async Task ExpireStaleLocksAsync()
    {
        var now = DateTime.UtcNow;
        var stale = await _context.Reservations
            .Where(x => x.Status == ReservationStatus.AwaitingPayment && x.PaymentLockExpiresAt <= now)
            .ToListAsync();

        foreach (var r in stale)
            r.Status = ReservationStatus.Cancelled; // or remove entirely, your call

        if (stale.Count > 0)
            await _context.SaveChangesAsync();
    }


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
            query = query.Where(x => x.YouthCenterActivity!.YouthCenterId == dto.YouthCenterId); // bug fix: was <=

        if (dto?.UserId != null)
            query = query.Where(x => x.UserId == dto.UserId); // bug fix: was <=

        return await query.CountAsync();
    }

    public async Task<int> GetActivityCount(FilteredReservationDto? dto)
    {
        var query = WithRelations().AsQueryable();

        if (dto?.YouthCenterId != null)
            query = query.Where(x => x.YouthCenterActivity!.YouthCenterId == dto.YouthCenterId); // bug fix: was <=

        if (dto?.UserId != null)
            query = query.Where(x => x.UserId == dto.UserId); // bug fix: was <=

        if (dto?.ActivityId != null)
            query = query.Where(x => x.YouthCenterActivity!.ActivityId == dto.ActivityId); // bug fix: was <=

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
            .Select(g => new PeakHourDto
            {
                Hour = g.Key,
                BookingCount = g.Count()
            })
            .OrderBy(x => x.Hour)
            .ToListAsync();
    }

    public async Task<List<PeakHourDto>> GetAvailBookingHoursAsync(AvailBookingDto? dto)
    {
        var query = WithRelations().AsQueryable();

        if (dto?.YouthCenterActivityId != null)
            query = query.Where(x =>
                x.YouthCenterActivityId == dto.YouthCenterActivityId);

        if (dto?.Date != null)
        {
            var targetDate = dto.Date.Date;
            query = query.Where(x => x.Date.Date == targetDate);
        }

        var reservations = await query
      .Where(x => x.Status == ReservationStatus.Confirmed ||
                 (x.Status == ReservationStatus.AwaitingPayment && x.PaymentLockExpiresAt > DateTime.UtcNow))
      .Select(x => new { StartHour = x.StartTime.Hour, EndHour = x.EndTime.Hour })
      .ToListAsync();

        var hourCounts = reservations
     .SelectMany(r =>
     {
         int start = r.StartHour;
         int end = r.EndHour == 0 ? 24 : r.EndHour; // treat midnight as 24

         return Enumerable.Range(start, end - start)
                          .Select(hour => hour % 24); // wrap 24 back to 0
     })
     .GroupBy(hour => hour)
     .ToDictionary(
         g => g.Key,
         g => g.Count());

        return Enumerable.Range(0, 24)
            .Select(hour => new PeakHourDto
            {
                Hour = hour,
                BookingCount = hourCounts.GetValueOrDefault(hour, 0)
            })
            .ToList();
    }
}