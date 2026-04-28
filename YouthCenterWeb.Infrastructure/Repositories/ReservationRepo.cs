using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class ReservationRepo(AppDbContext context) : IReservationRepo
{
    private readonly AppDbContext _context = context;

    public async Task<List<Reservation>> GetAllWithRelationsAsync()
    {
        var reservations = await _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenterActivity)
            .Include(x => x.YouthCenterActivity!.YouthCenter)
            .Include(x => x.YouthCenterActivity!.Activity)
            .ToListAsync();

        return reservations;
    }


    public async Task<List<Reservation>> GetUserReservationsAsync(int userId)
    {
        return await _context.Reservations
          .Where(x => x.UserId == userId)
          .Include(x => x.User)
          .Include(x => x.YouthCenterActivity)
          .Include(x => x.YouthCenterActivity!.YouthCenter)
          .Include(x => x.YouthCenterActivity!.Activity)
          .ToListAsync();

    }

    public async Task<List<Reservation>> GetYouthCenterReservationsAsync(int youthCenterId)
    {
        if (youthCenterId == 0) return new List<Reservation>();

        return await _context.Reservations.Where(a => a.YouthCenterActivity != null ? (a.YouthCenterActivity.YouthCenterId == youthCenterId) : false)
          .Include(b => b.User)
          .Include(c => c.YouthCenterActivity)
          .Include(c => c.YouthCenterActivity!.YouthCenter)
          .Include(c => c.YouthCenterActivity!.Activity)
          .ToListAsync();
    }
    public async Task<List<Reservation>> GetReservationsByStatusAsync(ReservationStatus reservationStatus)
    {
        return await _context.Reservations.Where(a => a.Status == reservationStatus)
                .Include(b => b.User)
                .Include(c => c.YouthCenterActivity)
                .Include(c => c.YouthCenterActivity!.YouthCenter)
                .Include(c => c.YouthCenterActivity!.Activity)
                .ToListAsync();
    }

    public async Task<Reservation?> GetByIdWithRelationsAsync(int id)
    {
        return await _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenterActivity)
            .Include(x => x.YouthCenterActivity!.YouthCenter)
            .Include(x => x.YouthCenterActivity!.Activity)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<YouthCenterActivity?> GetYouthCenterActivity(int youthCenterActivityId)
    {
        return await _context.YouthCenterActivities.FindAsync(youthCenterActivityId);
    }

    public async Task AddAsync(Reservation entity)
    {

        await _context.Reservations.AddAsync(entity);

    }

    public async Task<bool> DeleteAsync(int id)
    {
        var entity = await _context.Reservations.FindAsync(id);
        if (entity == null) return false;

        _context.Reservations.Remove(entity);
        return true;
    }



    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task<List<Reservation>> GetFiltersReservationsAsync(FilteredReservationDto dto)
    {
        var query = _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenterActivity)
            .Include(x => x.YouthCenterActivity!.YouthCenter)
            .Include(x => x.YouthCenterActivity!.Activity)
            .AsQueryable();

        if (dto.DateFrom != null && dto.DateFrom != DateTime.MinValue)
            query = query.Where(x => x.Date >= dto.DateFrom);
        if (dto.DateTo != null && dto.DateTo != DateTime.MinValue)
            query = query.Where(x => x.Date <= dto.DateTo);
        if (dto.StartTime != null && dto.StartTime != TimeOnly.MinValue)
            query = query.Where(x => x.StartTime >= dto.StartTime);
        if (dto.EndTime != null && dto.EndTime != TimeOnly.MinValue)
            query = query.Where(x => x.EndTime <= dto.EndTime);
        if (dto.YouthCenterId != null && dto.YouthCenterId != 0)
            query = query.Where(x => x.YouthCenterActivity != null ? (x.YouthCenterActivity.YouthCenterId == dto.YouthCenterId) : false);

        if (dto.ActivityId != null && dto.ActivityId != 0)
            query = query.Where(x => x.YouthCenterActivity != null ? (x.YouthCenterActivity.ActivityId == dto.ActivityId) : false);
        if (dto.UserId != null && dto.UserId != 0)
            query = query.Where(x => x.UserId == dto.UserId);
        if (dto.status != null)
            query = query.Where(x => x.Status == dto.status);
        return query.ToListAsync();
    }
}