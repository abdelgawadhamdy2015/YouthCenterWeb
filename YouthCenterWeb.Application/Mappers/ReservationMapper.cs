
using YouthCenterWeb.Models;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Data.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mapper
{
    public class ReservationMapper : IMapper<Reservation, ReservationDto, CreateReservationDto>
    {
        public Reservation CreateEntity(CreateReservationDto createDto)
        {
            return new Reservation
            {
                Date = createDto.Date,
                StartTime = createDto.StartTime,
                EndTime = createDto.EndTime,
                UserId = createDto.UserId,
                ActivityId = createDto.ActivityId,
                YouthCenterId = createDto.YouthCenterId,
                Status = ReservationStatus.Pending
            };
        }


        public ReservationDto ToDto(Reservation x)
        {
            return new ReservationDto
            {
                Id = x.Id,
                Date = x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                UserId = x.UserId,
                Username = x.User?.Name,
                YouthCenterId = x.YouthCenterId,
                Status = x.Status,
                YouthCenterName = x.YouthCenter?.Name,
                TotalPrice = x.TotalPrice
            };
        }

        public Reservation ToEntity(ReservationDto dto)
        {
            return new Reservation
            {
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                UserId = dto.UserId,
                ActivityId = dto.ActivityId,
                YouthCenterId = dto.YouthCenterId,
                Status = dto.Status
            };
        }


    }
}