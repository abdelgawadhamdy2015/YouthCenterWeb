using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.Application.Interfaces;

public interface IReservationRepo
{
    Task<List<Reservation>> GetAllWithRelationsAsync();
    Task<Reservation?> GetByIdWithRelationsAsync(int id);
    Task<List<Reservation>> GetUserReservationsAsync(int userId);
    Task<List<Reservation>> GetYouthCenterReservationsAsync(int youthCenterId);
    Task<List<Reservation>> GetReservationsByStatusAsync(ReservationStatus reservationStatus);
    Task<List<Reservation>> GetFiltersReservationsAsync(
        FilteredReservationDto dto
    );

    Task AddAsync(Reservation entity);
    Task<bool> DeleteAsync(int id);
    Task SaveChangesAsync();
}