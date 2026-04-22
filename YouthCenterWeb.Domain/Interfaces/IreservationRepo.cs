using YouthCenterWeb.Models;

namespace YouthCenterWeb.Application.Interfaces;

public interface IReservationRepo
{
    Task<List<Reservation>> GetAllWithRelationsAsync();
    Task<Reservation?> GetByIdWithRelationsAsync(int id);

    Task AddAsync(Reservation entity);
    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();
}