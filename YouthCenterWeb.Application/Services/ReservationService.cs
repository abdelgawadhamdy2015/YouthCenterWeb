using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class ReservationService(
        IReservationRepo repo,
        IAvailabilityRepo availabilityRepo, // ✅ new — shared reservation/match conflict check
        IPaymentGateway paymentGateway,
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
                ?? throw new NotFoundException(messageAr: Messages.Data.NoDataAr, messageEn: Messages.Data.NoDataAr);

            if (role == UserRole.Admin && activity.YouthCenterId != adminCenterId)
                throw new ForbiddenException("Activity does not belong to your center");

            if (role == UserRole.User)
                dto.UserId = userId;

            // ✅ يتحقق من تعارض الحجوزات والمباريات مع بعض
            var conflict = await availabilityRepo.HasConflictAsync(
                dto.YouthCenterActivityId, dto.Date, dto.StartTime, dto.EndTime);

            if (conflict)
                throw new BusinessException(
                    messageAr: "هذا الموعد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                    messageEn: "This time slot is no longer available — either booked or reserved for a match.");

            var entity = mapper.CreateEntity(dto);
            entity.TotalPrice = activity.Price * entity.DurationHours ?? 0;

            if (role == UserRole.Admin || role == UserRole.SuperAdmin)
            {
                entity.Status = ReservationStatus.Confirmed;
                entity.PaidAt = DateTime.UtcNow;
                entity.PaymentTransactionId = "MANUAL";
                entity.ReviewedBy = userId;
                entity.ReviewedAt = DateTime.UtcNow;
            }
            else
            {
                entity.Status = ReservationStatus.AwaitingPayment;
                entity.PaymentLockExpiresAt = DateTime.UtcNow.AddMinutes(10);
            }

            try
            {
                await repo.AddAsync(entity);
                await repo.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new BusinessException("This time slot was just taken by another user.");
            }

            // Only for User role — now that the lock row exists and has an Id,
            // create the payment intent with the gateway and attach the reference.
            if (role == UserRole.User)
            {
                var paymentOrder = await paymentGateway.CreatePaymentOrderAsync(
                    merchantOrderId: entity.Id.ToString(),
                    amountCents: (int)(entity.TotalPrice * 100));

                entity.PaymentTransactionId = paymentOrder.OrderId; // gateway's order id, for tracing
                if (paymentOrder.PaymentUrl == null)
                    throw new BusinessException("Failed to create payment order.");
                repo.Update(entity);
                await repo.SaveChangesAsync();

                var dtoResult = mapper.ToDto(entity);
                dtoResult.PaymentUrl = paymentOrder.PaymentUrl; // Flutter opens this
                return dtoResult;
            }

            var youthCenterActivity = await repo.GetYouthCenterActivity(entity.YouthCenterActivityId);
            entity.YouthCenterActivity = youthCenterActivity;
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

        public async Task<ReservationDto?> UpdateReservationAsync(UpdateReservationDto dto, int id, int userId, UserRole role, int? adminCenterId)
        {
            var entity = await repo.GetByIdWithRelationsAsync(id)
                          ?? throw new NotFoundException("Reservation", id);

            if (entity.Status != ReservationStatus.Pending)
                throw new BusinessException("Only Pending reservations can be updated.");

            if (role == UserRole.User && entity.UserId != userId)
                throw new ForbiddenException();

            if (role == UserRole.Admin &&
                                   entity.YouthCenterActivity?.YouthCenterId != adminCenterId)
                throw new ForbiddenException();

            // ✅ لو الموعد أو النشاط اتغير، لازم نتأكد إن المكان الجديد لسه متاح
            // (الكود الأصلي كان بيحدّث من غير أي فحص تعارض على الإطلاق)
            var newDate = dto.Date ?? entity.Date;
            var newStart = dto.StartTime ?? entity.StartTime;
            var newEnd = dto.EndTime ?? entity.EndTime;
            var newActivityId = dto.YouthCenterActivityId ?? entity.YouthCenterActivityId;

            bool timeOrActivityChanged =
                newDate != entity.Date ||
                newStart != entity.StartTime ||
                newEnd != entity.EndTime ||
                newActivityId != entity.YouthCenterActivityId;

            if (timeOrActivityChanged)
            {
                var conflict = await availabilityRepo.HasConflictAsync(
                    newActivityId, newDate, newStart, newEnd,
                    excludeReservationId: entity.Id);

                if (conflict)
                    throw new BusinessException(
                        messageAr: "هذا الموعد الجديد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                        messageEn: "The new time slot is no longer available — either booked or reserved for a match.");
            }

            entity = UpdateReservationDto(entity, dto);

            repo.Update(entity);
            await repo.SaveChangesAsync();

            return mapper.ToDto(entity);
        }

        private static Reservation UpdateReservationDto(Reservation reservation, UpdateReservationDto dto)
        {


            if (dto.Date.HasValue)
                reservation.Date = dto.Date.Value;

            if (dto.StartTime.HasValue)
                reservation.StartTime = dto.StartTime.Value;

            if (dto.EndTime.HasValue)
                reservation.EndTime = dto.EndTime.Value;

            if (dto.YouthCenterActivityId.HasValue)
                reservation.YouthCenterActivityId =
                    dto.YouthCenterActivityId.Value;

            if (dto.SpecialRequest != null)
                reservation.SpecialRequest = dto.SpecialRequest;

            if (dto.TotalPrice != null)
                reservation.TotalPrice = (decimal)dto.TotalPrice;

            return reservation;
        }



        public async Task<List<PeakHourDto>?> GetAvailHoursAsync(AvailBookingDto dto)
        {
            // ⚠️ لسه بترجع أوقات الحجوزات بس. لازم تتعدل عشان تحسب المباريات
            // كمان — راجع الرد الجاي عشان التعديل المطلوب في ReservationRepo.
            return await repo.GetAvailBookingHoursAsync(dto);
        }

        public async Task<ReservationDto?> LockForPaymentAsync(CreateReservationDto dto, int userId)
        {
            var activity = await repo.GetYouthCenterActivity(dto.YouthCenterActivityId)
                ?? throw new NotFoundException(messageAr: Messages.Data.NoDataAr, messageEn: Messages.Data.NoDataAr);

            dto.UserId = userId;

            // ✅ App-level check (fast path, good UX message) — reservations + matches
            var conflict = await availabilityRepo.HasConflictAsync(
                dto.YouthCenterActivityId, dto.Date, dto.StartTime, dto.EndTime);

            if (conflict)
                throw new BusinessException(
                    messageAr: "هذا الموعد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                    messageEn: "This time slot is no longer available — either booked or reserved for a match.");

            var entity = mapper.CreateEntity(dto);
            entity.Status = ReservationStatus.AwaitingPayment;
            entity.PaymentLockExpiresAt = DateTime.UtcNow.AddMinutes(10); // tune this
            entity.TotalPrice = activity.Price * entity.DurationHours ?? 0;

            try
            {
                await repo.AddAsync(entity);
                await repo.SaveChangesAsync(); // DB unique index catches any race the check above missed
            }
            catch (DbUpdateException)
            {
                throw new BusinessException("This time slot was just taken by another user.");
            }

            return mapper.ToDto(entity);
        }

        public async Task<ReservationDto?> ConfirmPaymentAsync(int reservationId, ConfirmPaymentDto dto)
        {
            var reservation = await repo.GetByIdWithRelationsAsync(reservationId)
                ?? throw new NotFoundException("Reservation", reservationId);

            if (reservation.Status != ReservationStatus.AwaitingPayment)
                throw new BusinessException("Reservation is not awaiting payment.");

            if (reservation.PaymentLockExpiresAt < DateTime.UtcNow)
                throw new BusinessException("Payment window expired. Please rebook.");

            // حماية إضافية — لو المبلغ اللي دفعه العميل مختلف عن سعر الحجز، مترفضش تأكيده
            if (dto.AmountPaid.HasValue && dto.AmountPaid != reservation.TotalPrice)
                throw new BusinessException("Paid amount does not match reservation price.");

            reservation.Status = ReservationStatus.Confirmed;
            reservation.PaymentTransactionId = dto.TransactionId;
            reservation.PaidAt = DateTime.UtcNow;
            reservation.PaymentLockExpiresAt = null;

            repo.Update(reservation);
            await repo.SaveChangesAsync();

            return mapper.ToDto(reservation);
        }

        public async Task FailPaymentAsync(int reservationId, string reason)
        {
            var reservation = await repo.GetByIdWithRelationsAsync(reservationId)
                ?? throw new NotFoundException("Reservation", reservationId);

            if (reservation.Status != ReservationStatus.AwaitingPayment) return; // idempotent no-op

            reservation.Status = ReservationStatus.Cancelled;
            reservation.RejectionReason = reason;

            repo.Update(reservation); // slot is freed automatically — next overlap check ignores Cancelled
            await repo.SaveChangesAsync();
        }
    }
}