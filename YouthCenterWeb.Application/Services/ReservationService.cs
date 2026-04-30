
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class ReservationService(
        IReservationRepo repo,
        IMapper<Reservation, ReservationDto, CreateReservationDto> mapper)
        : IReservationService
    {
        // Controller builds the filter — service just executes it
        public async Task<List<ReservationDto>> GetReservationsAsync(FilteredReservationDto? filter)
        {
            var reservations = await repo.GetFiltersReservationsAsync(filter);
            return reservations.Select(mapper.ToDto).ToList();
        }

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var entity = await repo.GetByIdWithRelationsAsync(id);
            return entity == null ? null : mapper.ToDto(entity);
        }

        public async Task<ReservationDto?> CreateReservationAsync(
            CreateReservationDto dto, int userId, UserRole role, int? adminCenterId)
        {
            var activity = await repo.GetYouthCenterActivity(dto.YouthCenterActivityId)
                ?? throw new NotFoundException("Activity not found");

            if (role == UserRole.Admin && activity.YouthCenterId != adminCenterId)
                throw new ForbiddenException("Activity does not belong to your center");

            if (role == UserRole.User)
                dto.UserId = userId;

            var entity = mapper.CreateEntity(dto);
            entity.TotalPrice = activity.Price * entity.DurationHours ?? 0;

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();
            return mapper.ToDto(entity);
        }

        // ReservationService.cs
        public async Task DeleteAsync(int id, int userId, UserRole role, int? adminCenterId)
        {
            var reservation = await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Reservation", id);

            if (role == UserRole.User)
            {
                if (reservation.UserId != userId)
                    throw new ForbiddenException();

                var canDelete = reservation.Status is
                    ReservationStatus.Pending or ReservationStatus.Completed;

                if (!canDelete)
                    throw new BusinessException("You can only delete Pending or Completed reservations.");
            }
            else if (role == UserRole.Admin)
            {
                if (reservation.YouthCenterActivity?.YouthCenterId != adminCenterId)
                    throw new ForbiddenException();

                if (reservation.Status != ReservationStatus.Completed)
                    throw new BusinessException("Admin can only delete Completed reservations.");
            }

            repo.Delete(reservation);   // ← entity passed directly, no second DB call
            await repo.SaveChangesAsync();
        }

        public async Task AcceptAsync(int id, int reviewedBy, int? adminCenterId, UserRole role)
        {
            var reservation = await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Reservation", id);

            if (role == UserRole.Admin &&
                reservation.YouthCenterActivity?.YouthCenterId != adminCenterId)
                throw new ForbiddenException();

            if (reservation.Status != ReservationStatus.Pending)
                throw new BusinessException("Only Pending reservations can be accepted.");

            reservation.Status = ReservationStatus.Confirmed;
            reservation.ReviewedBy = reviewedBy;
            reservation.ReviewedAt = DateTime.UtcNow;

            repo.Update(reservation);   // ← tell EF the entity changed
            await repo.SaveChangesAsync();
        }

        public async Task RejectAsync(int id, string reason, int reviewedBy, int? adminCenterId, UserRole role)
        {
            var reservation = await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Reservation", id);

            if (role == UserRole.Admin &&
                reservation.YouthCenterActivity?.YouthCenterId != adminCenterId)
                throw new ForbiddenException();

            if (reservation.Status != ReservationStatus.Pending)
                throw new BusinessException("Only Pending reservations can be rejected.");

            reservation.Status = ReservationStatus.Cancelled;
            reservation.RejectionReason = reason;
            reservation.ReviewedBy = reviewedBy;
            reservation.ReviewedAt = DateTime.UtcNow;

            repo.Update(reservation);
            await repo.SaveChangesAsync();
        }


    }
}