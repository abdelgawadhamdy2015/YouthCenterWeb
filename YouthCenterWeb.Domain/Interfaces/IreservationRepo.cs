using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

public interface IReservationRepo
{
    Task<List<Reservation>> GetFiltersReservationsAsync(FilteredReservationDto? dto);
    Task<Reservation?> GetByIdWithRelationsAsync(int id);
    Task<YouthCenterActivity?> GetYouthCenterActivity(int youthCenterActivityId);

    Task AddAsync(Reservation entity);
    void Delete(Reservation entity);
    void Update(Reservation entity);
    Task SaveChangesAsync();

    Task<bool> HasActiveOverlapAsync(int activityId, DateTime date, TimeOnly start, TimeOnly end);
    Task ExpireStaleLocksAsync();

    Task<int> GetCount(FilteredReservationDto? dto);
    Task<int> GetActivityCount(FilteredReservationDto? dto);
    Task<List<PeakHourDto>> GetPeakBookingHoursAsync(FilteredReservationDto? dto);
    Task<List<PeakHourDto>> GetAvailBookingHoursAsync(AvailBookingDto? dto);

    // جديد
    Task<List<int>> GetCenterAdminIdsAsync(int youthCenterId);
}