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
        IAvailabilityRepo availabilityRepo,
        IPaymentGateway paymentGateway,
        IMapper<Reservation, ReservationDto, CreateReservationDto> mapper)
        : IReservationService
    {
        // ============================================================
        // GET RESERVATIONS
        // ============================================================

        public async Task<List<ReservationDto>> GetReservationsAsync(
            FilteredReservationDto? filter)
        {
            var reservations =
                await repo.GetFiltersReservationsAsync(filter);

            return reservations
                .Select(mapper.ToDto)
                .ToList();
        }

        // ============================================================
        // GET BY ID
        // ============================================================

        public async Task<ReservationDto?> GetByIdAsync(int id)
        {
            var entity =
                await repo.GetByIdWithRelationsAsync(id);

            return entity == null
                ? null
                : mapper.ToDto(entity);
        }

        // ============================================================
        // CREATE RESERVATION
        // ============================================================

        public async Task<ReservationDto?> CreateReservationAsync(
            CreateReservationDto dto,
            int userId,
            UserRole role,
            int? adminCenterId)
        {
            var activity =
                await repo.GetYouthCenterActivity(
                    dto.YouthCenterActivityId)
                ?? throw new NotFoundException(
                    messageAr: Messages.Data.NoDataAr,
                    messageEn: Messages.Data.NoDataAr);

            // --------------------------------------------------------
            // ADMIN CENTER VALIDATION
            // --------------------------------------------------------

            if (role == UserRole.Admin &&
                activity.YouthCenterId != adminCenterId)
            {
                throw new ForbiddenException(
                    "Activity does not belong to your center");
            }

            // --------------------------------------------------------
            // USER ID
            // --------------------------------------------------------

            if (role == UserRole.User)
                dto.UserId = userId;

            // --------------------------------------------------------
            // AVAILABILITY CHECK
            // Reservations + Matches
            // --------------------------------------------------------

            var conflict =
                await availabilityRepo.HasConflictAsync(
                    dto.YouthCenterActivityId,
                    dto.Date,
                    dto.StartTime,
                    dto.EndTime);

            if (conflict)
            {
                throw new BusinessException(
                    messageAr:
                        "هذا الموعد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                    messageEn:
                        "This time slot is no longer available — either booked or reserved for a match.");
            }

            // --------------------------------------------------------
            // CREATE ENTITY
            // --------------------------------------------------------

            var entity =
                mapper.CreateEntity(dto);

            // IMPORTANT:
            // Never trust the price coming from the client.
            entity.TotalPrice =
                activity.Price * entity.DurationHours ?? 0;

            // --------------------------------------------------------
            // ADMIN / SUPER ADMIN
            // Manual confirmation
            // --------------------------------------------------------

            if (role == UserRole.Admin ||
                role == UserRole.SuperAdmin)
            {
                entity.Status =
                    ReservationStatus.Confirmed;

                entity.PaidAt =
                    DateTime.UtcNow;

                entity.PaymentTransactionId =
                    "MANUAL";

                entity.ReviewedBy =
                    userId;

                entity.ReviewedAt =
                    DateTime.UtcNow;
            }
            else
            {
                // ----------------------------------------------------
                // NORMAL USER
                // ----------------------------------------------------

                entity.Status =
                    ReservationStatus.AwaitingPayment;

                entity.PaymentLockExpiresAt =
                    DateTime.UtcNow.AddMinutes(10);
            }

            // --------------------------------------------------------
            // SAVE RESERVATION FIRST
            // We need entity.Id before creating Paymob order.
            // --------------------------------------------------------

            try
            {
                await repo.AddAsync(entity);
                await repo.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new BusinessException(
                    "This time slot was just taken by another user.");
            }

            // ========================================================
            // USER PAYMENT
            // ========================================================

            if (role == UserRole.User)
            {
                var amountCents =
                    checked(
                        (int)Math.Round(
                            entity.TotalPrice * 100m,
                            MidpointRounding.AwayFromZero));

                // IMPORTANT:
                // Generate merchant order ID ONCE.
                //
                // Do NOT use entity.Id directly.
                //
                // Example:
                // RES-20-8c6f...
                //
                // This value is saved in Reservation and reused
                // for tracing this payment attempt.
                var merchantOrderId =
                    $"RES-{entity.Id}-{Guid.NewGuid():N}";

                try
                {
                    var paymentOrder =
                        await paymentGateway.CreatePaymentOrderAsync(
                            merchantOrderId:
                                merchantOrderId,
                            amountCents:
                                amountCents);

                    if (string.IsNullOrWhiteSpace(
                            paymentOrder.PaymentUrl))
                    {
                        throw new BusinessException(
                            messageAr:
                                "تعذر إنشاء رابط الدفع.",
                            messageEn:
                                "Failed to create payment URL.");
                    }

                    // ------------------------------------------------
                    // Save payment information ONLY after Paymob
                    // successfully creates the order.
                    // ------------------------------------------------

                    entity.MerchantOrderId =
                        merchantOrderId;

                    entity.PaymentTransactionId =
                        paymentOrder.OrderId;

                    repo.Update(entity);

                    await repo.SaveChangesAsync();

                    var dtoResult =
                        mapper.ToDto(entity);

                    dtoResult.PaymentUrl =
                        paymentOrder.PaymentUrl;

                    return dtoResult;
                }
                catch (
                    PaymobPaymentGateway.PaymobException ex)
                {
                    Console.WriteLine(
                        $"Paymob Create Error " +
                        $"[{ex.StatusCode}]: " +
                        $"{ex.ResponseBody}");

                    // Keep reservation AwaitingPayment.
                    // User can retry later.
                    throw new BusinessException(
                        messageAr:
                            "تعذر إنشاء عملية الدفع حالياً. يرجى المحاولة مرة أخرى.",
                        messageEn:
                            "Unable to create the payment order. Please try again.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(
                        $"Paymob HTTP Error: {ex.Message}");

                    // Keep reservation AwaitingPayment.
                    throw new BusinessException(
                        messageAr:
                            "تعذر الاتصال بخدمة الدفع حالياً.",
                        messageEn:
                            "Unable to connect to the payment service.");
                }
            }

            // ========================================================
            // ADMIN / SUPER ADMIN RESULT
            // ========================================================

            var youthCenterActivity =
                await repo.GetYouthCenterActivity(
                    entity.YouthCenterActivityId);

            entity.YouthCenterActivity =
                youthCenterActivity;

            return mapper.ToDto(entity);
        }

        // ============================================================
        // DELETE
        // ============================================================

        public async Task DeleteAsync(
            int id,
            int userId,
            UserRole role,
            int? adminCenterId)
        {
            var reservation =
                await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException(
                    "Reservation",
                    id);

            if (role == UserRole.User)
            {
                if (reservation.UserId != userId)
                    throw new ForbiddenException();

                var canDelete =
                    reservation.Status is
                        ReservationStatus.Pending
                        or ReservationStatus.Completed;

                if (!canDelete)
                {
                    throw new BusinessException(
                        "You can only delete Pending or Completed reservations.");
                }
            }
            else if (role == UserRole.Admin)
            {
                if (reservation.YouthCenterActivity?.YouthCenterId
                    != adminCenterId)
                {
                    throw new ForbiddenException();
                }

                if (reservation.Status !=
                    ReservationStatus.Completed)
                {
                    throw new BusinessException(
                        "Admin can only delete Completed reservations.");
                }
            }

            repo.Delete(reservation);

            await repo.SaveChangesAsync();
        }

        // ============================================================
        // ACCEPT
        // ============================================================

        public async Task AcceptAsync(
            int id,
            int reviewedBy,
            int? adminCenterId,
            UserRole role)
        {
            var reservation =
                await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException(
                    "Reservation",
                    id);

            if (role == UserRole.Admin &&
                reservation.YouthCenterActivity?.YouthCenterId
                != adminCenterId)
            {
                throw new ForbiddenException();
            }

            if (reservation.Status !=
                ReservationStatus.Pending)
            {
                throw new BusinessException(
                    "Only Pending reservations can be accepted.");
            }

            reservation.Status =
                ReservationStatus.Confirmed;

            reservation.ReviewedBy =
                reviewedBy;

            reservation.ReviewedAt =
                DateTime.UtcNow;

            repo.Update(reservation);

            await repo.SaveChangesAsync();
        }

        // ============================================================
        // REJECT
        // ============================================================

        public async Task RejectAsync(
            int id,
            string reason,
            int reviewedBy,
            int? adminCenterId,
            UserRole role)
        {
            var reservation =
                await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException(
                    "Reservation",
                    id);

            if (role == UserRole.Admin &&
                reservation.YouthCenterActivity?.YouthCenterId
                != adminCenterId)
            {
                throw new ForbiddenException();
            }

            if (reservation.Status !=
                ReservationStatus.Pending)
            {
                throw new BusinessException(
                    "Only Pending reservations can be rejected.");
            }

            reservation.Status =
                ReservationStatus.Cancelled;

            reservation.RejectionReason =
                reason;

            reservation.ReviewedBy =
                reviewedBy;

            reservation.ReviewedAt =
                DateTime.UtcNow;

            repo.Update(reservation);

            await repo.SaveChangesAsync();
        }

        // ============================================================
        // UPDATE RESERVATION
        // ============================================================

        public async Task<ReservationDto?> UpdateReservationAsync(
            UpdateReservationDto dto,
            int id,
            int userId,
            UserRole role,
            int? adminCenterId)
        {
            var entity =
                await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException(
                    "Reservation",
                    id);

            // --------------------------------------------------------
            // STATUS
            // --------------------------------------------------------

            if (entity.Status !=
                    ReservationStatus.Pending &&
                entity.Status !=
                    ReservationStatus.AwaitingPayment)
            {
                throw new BusinessException(
                    "Only Pending or AwaitingPayment reservations can be updated.");
            }

            // --------------------------------------------------------
            // USER AUTHORIZATION
            // --------------------------------------------------------

            if (role == UserRole.User &&
                entity.UserId != userId)
            {
                throw new ForbiddenException();
            }

            // --------------------------------------------------------
            // ADMIN AUTHORIZATION
            // --------------------------------------------------------

            if (role == UserRole.Admin &&
                entity.YouthCenterActivity?.YouthCenterId
                != adminCenterId)
            {
                throw new ForbiddenException();
            }

            // --------------------------------------------------------
            // NEW VALUES
            // --------------------------------------------------------

            var newDate =
                dto.Date ?? entity.Date;

            var newStart =
                dto.StartTime ?? entity.StartTime;

            var newEnd =
                dto.EndTime ?? entity.EndTime;

            var newActivityId =
                dto.YouthCenterActivityId
                ?? entity.YouthCenterActivityId;

            // --------------------------------------------------------
            // CHECK WHETHER PAYMENT-RELEVANT DATA CHANGED
            // --------------------------------------------------------

            var timeOrActivityChanged =
                newDate != entity.Date ||
                newStart != entity.StartTime ||
                newEnd != entity.EndTime ||
                newActivityId !=
                    entity.YouthCenterActivityId;

            // --------------------------------------------------------
            // CHECK AVAILABILITY
            // --------------------------------------------------------

            if (timeOrActivityChanged)
            {
                var conflict =
                    await availabilityRepo.HasConflictAsync(
                        newActivityId,
                        newDate,
                        newStart,
                        newEnd,
                        excludeReservationId:
                            entity.Id);

                if (conflict)
                {
                    throw new BusinessException(
                        messageAr:
                            "هذا الموعد الجديد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                        messageEn:
                            "The new time slot is no longer available — either booked or reserved for a match.");
                }
            }

            // --------------------------------------------------------
            // KEEP OLD PAYMENT STATE
            // --------------------------------------------------------

            var wasAwaitingPayment =
                entity.Status ==
                ReservationStatus.AwaitingPayment;

            var oldTotalPrice =
                entity.TotalPrice;

            // --------------------------------------------------------
            // APPLY CHANGES
            // --------------------------------------------------------

            entity =
                UpdateReservationDto(
                    entity,
                    dto);

            // --------------------------------------------------------
            // RECALCULATE PRICE
            // --------------------------------------------------------

            if (timeOrActivityChanged)
            {
                var activity =
                    await repo.GetYouthCenterActivity(
                        newActivityId)
                    ?? throw new NotFoundException(
                        messageAr:
                            Messages.Data.NoDataAr,
                        messageEn:
                            Messages.Data.NoDataAr);

                entity.TotalPrice =
                    activity.Price *
                    entity.DurationHours ?? 0;
            }

            var paymentAmountChanged =
                oldTotalPrice != entity.TotalPrice;

            // --------------------------------------------------------
            // SAVE RESERVATION
            // --------------------------------------------------------

            repo.Update(entity);

            await repo.SaveChangesAsync();

            var dtoResult =
                mapper.ToDto(entity);

            // ========================================================
            // IMPORTANT PAYMENT RULE
            // ========================================================
            //
            // If this is only a normal update:
            //
            //   specialRequest
            //
            // DON'T create another Paymob order.
            //
            // If reservation is awaiting payment and the amount
            // did NOT change, keep the existing payment order.
            //
            // If amount changed, the old Paymob order represents
            // the old amount, so a NEW Paymob order is required.
            // ========================================================

            if (wasAwaitingPayment &&
                paymentAmountChanged)
            {
                try
                {
                    var amountCents =
                        checked(
                            (int)Math.Round(
                                entity.TotalPrice * 100m,
                                MidpointRounding.AwayFromZero));

                    // ------------------------------------------------
                    // NEW PAYMOB ORDER
                    //
                    // Because amount changed, we CANNOT reuse the
                    // previous Paymob order.
                    //
                    // Therefore generate a NEW merchantOrderId.
                    // ------------------------------------------------

                    var newMerchantOrderId =
                        $"RES-{entity.Id}-{Guid.NewGuid():N}";

                    var paymentOrder =
                        await paymentGateway
                            .CreatePaymentOrderAsync(
                                merchantOrderId:
                                    newMerchantOrderId,
                                amountCents:
                                    amountCents);

                    if (string.IsNullOrWhiteSpace(
                            paymentOrder.PaymentUrl))
                    {
                        throw new BusinessException(
                            messageAr:
                                "تعذر إنشاء رابط الدفع.",
                            messageEn:
                                "Failed to create payment URL.");
                    }

                    // ------------------------------------------------
                    // Replace old Paymob payment reference.
                    // ------------------------------------------------

                    entity.MerchantOrderId =
                        newMerchantOrderId;

                    entity.PaymentTransactionId =
                        paymentOrder.OrderId;

                    repo.Update(entity);

                    await repo.SaveChangesAsync();

                    dtoResult.PaymentUrl =
                        paymentOrder.PaymentUrl;
                }
                catch (
                    PaymobPaymentGateway.PaymobException ex)
                {
                    Console.WriteLine(
                        $"Paymob Update Error " +
                        $"[{ex.StatusCode}]: " +
                        $"{ex.ResponseBody}");

                    throw new BusinessException(
                        messageAr:
                            "تعذر تحديث عملية الدفع حالياً. يرجى المحاولة مرة أخرى.",
                        messageEn:
                            "Unable to update the payment order. Please try again.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(
                        $"Paymob Update HTTP Error: " +
                        $"{ex.Message}");

                    throw new BusinessException(
                        messageAr:
                            "تعذر الاتصال بخدمة الدفع حالياً.",
                        messageEn:
                            "Unable to connect to the payment service.");
                }
            }

            return dtoResult;
        }

        // ============================================================
        // UPDATE DTO -> ENTITY
        // ============================================================

        private static Reservation UpdateReservationDto(
            Reservation reservation,
            UpdateReservationDto dto)
        {
            if (dto.Date.HasValue)
                reservation.Date =
                    dto.Date.Value;

            if (dto.StartTime.HasValue)
                reservation.StartTime =
                    dto.StartTime.Value;

            if (dto.EndTime.HasValue)
                reservation.EndTime =
                    dto.EndTime.Value;

            if (dto.YouthCenterActivityId.HasValue)
            {
                reservation.YouthCenterActivityId =
                    dto.YouthCenterActivityId.Value;
            }

            if (dto.SpecialRequest != null)
            {
                reservation.SpecialRequest =
                    dto.SpecialRequest;
            }

            // IMPORTANT:
            // dto.TotalPrice is intentionally ignored.
            //
            // The server calculates TotalPrice from the activity.
            //
            // NEVER trust the price sent by Flutter.

            return reservation;
        }

        // ============================================================
        // AVAILABLE HOURS
        // ============================================================

        public async Task<List<PeakHourDto>?> GetAvailHoursAsync(
            AvailBookingDto dto)
        {
            return await repo.GetAvailBookingHoursAsync(dto);
        }

        // ============================================================
        // LOCK FOR PAYMENT
        // ============================================================

        public async Task<ReservationDto?> LockForPaymentAsync(
            CreateReservationDto dto,
            int userId)
        {
            var activity =
                await repo.GetYouthCenterActivity(
                    dto.YouthCenterActivityId)
                ?? throw new NotFoundException(
                    messageAr:
                        Messages.Data.NoDataAr,
                    messageEn:
                        Messages.Data.NoDataAr);

            dto.UserId =
                userId;

            // --------------------------------------------------------
            // AVAILABILITY
            // --------------------------------------------------------

            var conflict =
                await availabilityRepo.HasConflictAsync(
                    dto.YouthCenterActivityId,
                    dto.Date,
                    dto.StartTime,
                    dto.EndTime);

            if (conflict)
            {
                throw new BusinessException(
                    messageAr:
                        "هذا الموعد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                    messageEn:
                        "This time slot is no longer available — either booked or reserved for a match.");
            }

            // --------------------------------------------------------
            // CREATE RESERVATION
            // --------------------------------------------------------

            var entity =
                mapper.CreateEntity(dto);

            entity.Status =
                ReservationStatus.AwaitingPayment;

            entity.PaymentLockExpiresAt =
                DateTime.UtcNow.AddMinutes(10);

            entity.TotalPrice =
                activity.Price *
                entity.DurationHours ?? 0;

            try
            {
                await repo.AddAsync(entity);

                await repo.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new BusinessException(
                    "This time slot was just taken by another user.");
            }

            return mapper.ToDto(entity);
        }

        // ============================================================
        // PAY AGAIN
        // ============================================================

        public async Task<ReservationDto?> PayAgainAsync(
            int reservationId,
            int userId)
        {
            var reservation =
                await repo.GetByIdWithRelationsAsync(
                    reservationId)
                ?? throw new NotFoundException(
                    "Reservation",
                    reservationId);

            // --------------------------------------------------------
            // OWNERSHIP
            // --------------------------------------------------------

            if (reservation.UserId != userId)
                throw new ForbiddenException();

            // --------------------------------------------------------
            // STATUS
            // --------------------------------------------------------

            if (reservation.Status !=
                ReservationStatus.AwaitingPayment)
            {
                throw new BusinessException(
                    messageAr:
                        "هذا الحجز لا يحتاج إلى دفع حالياً.",
                    messageEn:
                        "This reservation does not require payment.");
            }

            // --------------------------------------------------------
            // CALCULATE CURRENT AMOUNT
            // --------------------------------------------------------

            var amountCents =
                checked(
                    (int)Math.Round(
                        reservation.TotalPrice * 100m,
                        MidpointRounding.AwayFromZero));

            // --------------------------------------------------------
            // CASE 1
            // Existing Paymob Order exists.
            //
            // Try to reuse it.
            // --------------------------------------------------------

            if (!string.IsNullOrWhiteSpace(
                    reservation.PaymentTransactionId))
            {
                try
                {
                    var paymentOrder =
                        await paymentGateway
                            .CreatePaymentForExistingOrderAsync(
                                reservation.PaymentTransactionId,
                                amountCents);

                    if (!string.IsNullOrWhiteSpace(
                            paymentOrder.PaymentUrl))
                    {
                        // Renew payment lock.
                        reservation.PaymentLockExpiresAt =
                            DateTime.UtcNow.AddMinutes(10);

                        repo.Update(reservation);

                        await repo.SaveChangesAsync();

                        var result =
                            mapper.ToDto(reservation);

                        result.PaymentUrl =
                            paymentOrder.PaymentUrl;

                        return result;
                    }
                }
                catch (
                    PaymobPaymentGateway.PaymobException ex)
                {
                    Console.WriteLine(
                        $"Paymob Existing Order Error " +
                        $"[{ex.StatusCode}]: " +
                        $"{ex.ResponseBody}");

                    // ------------------------------------------------
                    // Existing order cannot be reused.
                    //
                    // We continue below and create a NEW order.
                    // ------------------------------------------------
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine(
                        $"Paymob Existing Order HTTP Error: " +
                        $"{ex.Message}");

                    throw new BusinessException(
                        messageAr:
                            "تعذر الاتصال بخدمة الدفع حالياً.",
                        messageEn:
                            "Unable to connect to the payment service.");
                }
            }

            // ========================================================
            // CASE 2
            // Existing Paymob order cannot be reused.
            //
            // Create a NEW Paymob Order.
            //
            // IMPORTANT:
            // NEW merchantOrderId => no duplicate.
            // ========================================================

            var newMerchantOrderId =
                $"RES-{reservation.Id}-{Guid.NewGuid():N}";

            try
            {
                var paymentOrder =
                    await paymentGateway
                        .CreatePaymentOrderAsync(
                            merchantOrderId:
                                newMerchantOrderId,
                            amountCents:
                                amountCents);

                if (string.IsNullOrWhiteSpace(
                        paymentOrder.PaymentUrl))
                {
                    throw new BusinessException(
                        messageAr:
                            "تعذر إنشاء رابط الدفع.",
                        messageEn:
                            "Failed to create payment URL.");
                }

                // ----------------------------------------------------
                // Replace payment reference with NEW order.
                // ----------------------------------------------------

                reservation.MerchantOrderId =
                    newMerchantOrderId;

                reservation.PaymentTransactionId =
                    paymentOrder.OrderId;

                reservation.PaymentLockExpiresAt =
                    DateTime.UtcNow.AddMinutes(10);

                repo.Update(reservation);

                await repo.SaveChangesAsync();

                var result =
                    mapper.ToDto(reservation);

                result.PaymentUrl =
                    paymentOrder.PaymentUrl;

                return result;
            }
            catch (
                PaymobPaymentGateway.PaymobException ex)
            {
                Console.WriteLine(
                    $"Paymob New Order Error " +
                    $"[{ex.StatusCode}]: " +
                    $"{ex.ResponseBody}");

                throw new BusinessException(
                    messageAr:
                        "تعذر إنشاء عملية دفع جديدة حالياً. يرجى المحاولة مرة أخرى.",
                    messageEn:
                        "Unable to create a new payment order. Please try again.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine(
                    $"Paymob New Order HTTP Error: " +
                    $"{ex.Message}");

                throw new BusinessException(
                    messageAr:
                        "تعذر الاتصال بخدمة الدفع حالياً.",
                    messageEn:
                        "Unable to connect to the payment service.");
            }
        }

        // ============================================================
        // CONFIRM PAYMENT
        // ============================================================

        public async Task<ReservationDto?> ConfirmPaymentAsync(
            int reservationId,
            ConfirmPaymentDto dto)
        {
            var reservation =
                await repo.GetByIdWithRelationsAsync(
                    reservationId)
                ?? throw new NotFoundException(
                    "Reservation",
                    reservationId);

            if (reservation.Status !=
                ReservationStatus.AwaitingPayment)
            {
                throw new BusinessException(
                    "Reservation is not awaiting payment.");
            }

            // --------------------------------------------------------
            // PAYMENT LOCK
            // --------------------------------------------------------

            if (reservation.PaymentLockExpiresAt.HasValue &&
                reservation.PaymentLockExpiresAt <
                    DateTime.UtcNow)
            {
                throw new BusinessException(
                    "Payment window expired. Please pay again.");
            }

            // --------------------------------------------------------
            // AMOUNT VALIDATION
            // --------------------------------------------------------

            if (dto.AmountPaid.HasValue &&
                dto.AmountPaid.Value !=
                    reservation.TotalPrice)
            {
                throw new BusinessException(
                    "Paid amount does not match reservation price.");
            }

            // --------------------------------------------------------
            // PAYMENT CONFIRMED
            // --------------------------------------------------------

            reservation.Status =
                ReservationStatus.Confirmed;

            reservation.PaymentTransactionId =
                dto.TransactionId;

            reservation.PaidAt =
                DateTime.UtcNow;

            reservation.PaymentLockExpiresAt =
                null;

            repo.Update(reservation);

            await repo.SaveChangesAsync();

            return mapper.ToDto(reservation);
        }

        // ============================================================
        // PAYMENT FAILED
        // ============================================================

        public async Task FailPaymentAsync(
            int reservationId,
            string reason)
        {
            var reservation =
                await repo.GetByIdWithRelationsAsync(
                    reservationId)
                ?? throw new NotFoundException(
                    "Reservation",
                    reservationId);

            // --------------------------------------------------------
            // IMPORTANT:
            //
            // Payment failure does NOT cancel the reservation.
            //
            // It remains AwaitingPayment so the user can pay later.
            // --------------------------------------------------------

            if (reservation.Status !=
                ReservationStatus.AwaitingPayment)
            {
                return;
            }

            reservation.RejectionReason =
                reason;

            // Keep:
            //
            // reservation.Status =
            //     ReservationStatus.AwaitingPayment;
            //
            // Do NOT cancel here.

            repo.Update(reservation);

            await repo.SaveChangesAsync();
        }
    }
}