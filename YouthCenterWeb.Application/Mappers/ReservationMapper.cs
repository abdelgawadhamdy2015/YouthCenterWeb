using YouthCenterWeb.DTOs;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mappers
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
                UserId = createDto.UserId ?? 0,
                YouthCenterActivityId = createDto.YouthCenterActivityId,
                Status = ReservationStatus.Pending,

            };
        }


        public ReservationDto ToDto(Reservation x)
        {

            var reservationDto = new ReservationDto
            {
                Id = x.Id,
                Date = x.Date,
                StartTime = x.StartTime,
                EndTime = x.EndTime,
                UserId = x.UserId,
                Username = x.User?.Name,
                YouthCenterActivityId = x.YouthCenterActivityId,
                Status = x.Status,
                Longitude = x.YouthCenterActivity?.YouthCenter.Longitude,
                Latitude = x.YouthCenterActivity?.YouthCenter.Latitude,
                YouthCenterName = x.YouthCenterActivity?.YouthCenter?.NameAr,
                YouthCenterId = x.YouthCenterActivity?.YouthCenterId,
                ActivityName = x.YouthCenterActivity?.Activity?.NameAr,
                ActivityId = x.YouthCenterActivity?.ActivityId,
                TotalPrice = x.TotalPrice,
                DurationHours = x.DurationHours,
                SpecialRequest = x.SpecialRequest,
                RejectionReason = x.RejectionReason,
                RefundPending = x.RefundPending,
                ReviewedAt = x.ReviewedAt,
                ReviewedBy = x.ReviewedBy,
                CancelledAt = x.CancelledAt,
                PaymentLockExpiresAt = x.PaymentLockExpiresAt,
                PaidAt = x.PaidAt,
                PaymentTransactionId = x.PaymentTransactionId



            };
            return reservationDto;
        }

        public Reservation ToEntity(ReservationDto dto)
        {
            return new Reservation
            {
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                UserId = dto.UserId,
                YouthCenterActivityId = dto.YouthCenterActivityId,
                Status = dto.Status,
                SpecialRequest = dto.SpecialRequest

            };
        }

        public Reservation UpdateEntity(Reservation entity, ReservationDto updateDto)
        {
            entity.Date = updateDto.Date;
            entity.StartTime = updateDto.StartTime;
            entity.EndTime = updateDto.EndTime;
            entity.UserId = updateDto.UserId;
            entity.YouthCenterActivityId = updateDto.YouthCenterActivityId;
            entity.Status = updateDto.Status;
            entity.SpecialRequest = updateDto.SpecialRequest;

            return entity;
        }
    }
}