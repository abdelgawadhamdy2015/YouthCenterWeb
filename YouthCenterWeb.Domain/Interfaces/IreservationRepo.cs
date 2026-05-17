// Application/Interfaces/IReservationRepo.cs
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface IReservationRepo
    {
        Task<List<Reservation>> GetFiltersReservationsAsync(FilteredReservationDto? dto);
        Task<Reservation?> GetByIdWithRelationsAsync(int id);
        Task<int> GetCount(FilterReservationDashboardDto? dto);

        Task<YouthCenterActivity?> GetYouthCenterActivity(int youthCenterActivityId);
        Task<List<PeakHourDto>> GetPeakBookingHoursAsync(FilteredReservationDto? dto);
        Task AddAsync(Reservation entity);
        void Delete(Reservation entity);    // sync — EF tracks it
        void Update(Reservation entity);    // sync — EF tracks it
        Task SaveChangesAsync();
    }
}