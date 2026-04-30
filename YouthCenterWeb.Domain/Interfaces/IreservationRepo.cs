// Application/Interfaces/IReservationRepo.cs
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

public interface IReservationRepo
{
    Task<List<Reservation>> GetFiltersReservationsAsync(FilteredReservationDto? dto);
    Task<Reservation?> GetByIdWithRelationsAsync(int id);
    Task<YouthCenterActivity?> GetYouthCenterActivity(int youthCenterActivityId);
    Task AddAsync(Reservation entity);
    void Delete(Reservation entity);    // sync — EF tracks it
    void Update(Reservation entity);    // sync — EF tracks it
    Task SaveChangesAsync();
}