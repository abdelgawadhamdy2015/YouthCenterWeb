using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.Application.Interfaces;

public interface IReservationService
{
    Task<List<ReservationDto>> GetReservationsAsync(FilteredReservationDto? dto = null);

    Task<ReservationDto?> GetByIdAsync(int id);

    Task<ReservationDto?> CreateReservationAsync(CreateReservationDto dto, int userId, UserRole role, int? adminCenterId);

    Task DeleteAsync(int id, int userId, UserRole role, int? adminCenterId);
    Task AcceptAsync(int id, int reviewedBy, int? adminCenterId, UserRole role);
    Task RejectAsync(int id, string reason, int reviewedBy, int? adminCenterId, UserRole role);
}