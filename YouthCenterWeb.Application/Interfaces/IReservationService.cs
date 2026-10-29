using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

public interface IReservationService
{
    Task<List<ReservationDto>> GetReservationsAsync(FilteredReservationDto? filter);
    Task<ReservationDto?> GetByIdAsync(int id);
    Task<ReservationDto?> GetByIdSecuredAsync(int id, int userId, UserRole role, int? adminCenterId);

    Task<ReservationDto?> CreateReservationAsync(
        CreateReservationDto dto, int userId, UserRole role, int? adminCenterId);

    Task<ReservationDto?> UpdateReservationAsync(
        UpdateReservationDto dto, int id, int userId, UserRole role, int? adminCenterId);

    Task DeleteAsync(int id, int userId, UserRole role, int? adminCenterId);
    Task AcceptAsync(int id, int reviewedBy, int? adminCenterId, UserRole role);
    Task RejectAsync(int id, string reason, int reviewedBy, int? adminCenterId, UserRole role);

    // جديد
    Task<ReservationDto?> CancelAsync(int id, int userId);

    Task<List<PeakHourDto>?> GetAvailHoursAsync(AvailBookingDto dto);

    Task<ReservationDto?> LockForPaymentAsync(CreateReservationDto dto, int userId);
    Task<ReservationDto?> PayAgainAsync(int reservationId, int userId);
    Task<ReservationDto?> ConfirmPaymentAsync(int reservationId, ConfirmPaymentDto dto);
    Task FailPaymentAsync(int reservationId, string reason);
}