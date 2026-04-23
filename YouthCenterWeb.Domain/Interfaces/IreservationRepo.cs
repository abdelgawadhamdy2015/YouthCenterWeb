using YouthCenterWeb.Models;

namespace YouthCenterWeb.Application.Interfaces;

public interface IReservationRepo
{
    Task<List<Reservation>> GetAllWithRelationsAsync();
    Task<Reservation?> GetByIdWithRelationsAsync(int id);
    Task<List<Reservation>> GetUserReservationsAsync(int userId);
    Task<List<Reservation>> GetYouthCenterReservationsAsync(int youthCenterId);
    Task<List<Reservation>> GetReservationsByStatusAsync(ReservationStatus reservationStatus);

    Task AddAsync(Reservation entity);
    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();
}