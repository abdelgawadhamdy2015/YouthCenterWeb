using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

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
        _context.YouthCenterActivities.FindAsync(youthCenterActivityId).AsTask();

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
}