using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

public class ReservationRepo(AppDbContext context) : IReservationRepo
{
    private readonly AppDbContext _context = context;

    public async Task<List<Reservation>> GetAllWithRelationsAsync()
    {
        return await _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenter)
            .Include(x => x.Activity)
            .ToListAsync();
    }

    public async Task<Reservation?> GetByIdWithRelationsAsync(int id)
    {
        return await _context.Reservations
            .Include(x => x.User)
            .Include(x => x.YouthCenter)
            .Include(x => x.Activity)
            .FirstOrDefaultAsync(x => x.Id == id);
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
}