using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.Application.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepo _repo;

    public ReservationService(IReservationRepo repo)
    {
        _repo = repo;
    }

    public async Task<List<ReservationDto>> GetAllAsync()
    {
        var data = await _repo.GetAllWithRelationsAsync();

        return data.Select(x => new ReservationDto
        {
            Id = x.Id,
            Date = x.Date,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Username = x.User?.Name,
            YouthCenterName = x.YouthCenter?.Name,
            TotalPrice = x.TotalPrice
        }).ToList();
    }

    public async Task<ReservationDto?> GetByIdAsync(int id)
    {
        var x = await _repo.GetByIdWithRelationsAsync(id);
        if (x == null) return null;

        return new ReservationDto
        {
            Id = x.Id,
            Date = x.Date,
            StartTime = x.StartTime,
            EndTime = x.EndTime,
            Username = x.User?.Name,
            YouthCenterName = x.YouthCenter?.Name,
            TotalPrice = x.TotalPrice
        };
    }

    public async Task<ReservationDto> CreateAsync(CreateReservationDto dto)
    {
        var entity = new Reservation
        {
            Date = dto.Date,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            UserId = dto.UserId,
            ActivityId = dto.ActivityId,
            YouthCenterId = dto.YouthCenterId
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return new ReservationDto
        {
            Id = entity.Id,
            Date = entity.Date,
            StartTime = entity.StartTime,
            EndTime = entity.EndTime
        };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _repo.DeleteAsync(id);
        if (!result) return false;

        await _repo.SaveChangesAsync();
        return true;
    }
}