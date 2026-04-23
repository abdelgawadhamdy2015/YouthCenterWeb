using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;

namespace YouthCenterWeb.Application.Interfaces;

public interface IReservationService
{
    Task<List<ReservationDto>> GetAllAsync();
    Task<List<ReservationDto>> GetUserReservationsAsync(int userId);
    Task<List<ReservationDto>> GetYouthCenterReservationsAsync(int youthCenterId);

    Task<List<ReservationDto>> GetReservationsByStatusAsync(ReservationStatus reservationStatus);
    Task<ReservationDto?> GetByIdAsync(int id);

    Task<ReservationDto> CreateAsync(CreateReservationDto dto);

    Task<bool> DeleteAsync(int id);
}