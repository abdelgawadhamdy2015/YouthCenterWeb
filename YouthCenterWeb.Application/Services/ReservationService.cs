using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class ReservationService(
        IReservationRepo repo,
        IAvailabilityRepo availabilityRepo,
        IPaymentGateway paymentGateway,
        INotificationService notifications,
        IConfiguration config,
        IMapper<Reservation, ReservationDto, CreateReservationDto> mapper)
        : IReservationService
    {
        private const int CancelCutoffHours = 2;

        // الدفع الإلكتروني: يتحكم فيه appsettings → Payments:Enabled
        private bool PaymentsEnabled => config.GetValue<bool>("Payments:Enabled");

        private static readonly TimeZoneInfo CairoTz = ResolveCairoTz();

        private static TimeZoneInfo ResolveCairoTz()
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); }
            catch { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); }
        }

        private static int ToCents(decimal price) =>
            checked((int)Math.Round(price * 100m, MidpointRounding.AwayFromZero));

        // ============================================================
        // NOTIFICATION HELPERS
        // ============================================================

        private Task NotifyUserAsync(Reservation r, string title, string body) =>
            notifications.SendToUserAsync(
                userId: r.UserId,
                title: title,
                body: body,
                type: "reservation",
                targetId: r.Id);

        private async Task NotifyCenterAdminsAsync(
            int? youthCenterId, int reservationId, string title, string body)
        {
            if (youthCenterId is null) return;

            foreach (var adminId in await repo.GetCenterAdminIdsAsync(youthCenterId.Value))
            {
                await notifications.SendToUserAsync(
                    userId: adminId,
                    title: title,
                    body: body,
                    type: "reservation",
                    targetId: reservationId);
            }
        }

        // ============================================================
        // GET
        // ============================================================

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

        // نسخة بفحص الملكية: اللاعب يشوف حجزه فقط، والأدمن حجوزات مركزه فقط
        public async Task<ReservationDto?> GetByIdSecuredAsync(
            int id, int userId, UserRole role, int? adminCenterId)
        {
            var entity = await repo.GetByIdWithRelationsAsync(id);
            if (entity is null) return null;

            if (role == UserRole.User && entity.UserId != userId) return null;

            if (role == UserRole.Admin &&
                entity.YouthCenterActivity?.YouthCenterId != adminCenterId)
                return null;

            return mapper.ToDto(entity);
        }

        // ============================================================
        // CREATE
        // ============================================================

        public async Task<ReservationDto?> CreateReservationAsync(
            CreateReservationDto dto,
            int userId,
            UserRole role,
            int? adminCenterId)
        {
            var activity = await repo.GetYouthCenterActivity(dto.YouthCenterActivityId)
                ?? throw new NotFoundException(
                    messageAr: Messages.Data.NoDataAr,
                    messageEn: Messages.Data.NoDataAr);

            if (role == UserRole.Admin && activity.YouthCenterId != adminCenterId)
                throw new ForbiddenException("Activity does not belong to your center");

            if (role == UserRole.User)
                dto.UserId = userId;

            var conflict = await availabilityRepo.HasConflictAsync(
                dto.YouthCenterActivityId, dto.Date, dto.StartTime, dto.EndTime);

            if (conflict)
            {
                throw new BusinessException(
                    messageAr: "هذا الموعد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                    messageEn: "This time slot is no longer available — either booked or reserved for a match.");
            }

            var entity = mapper.CreateEntity(dto);

            // السعر دايمًا من السيرفر
            entity.TotalPrice = activity.Price * entity.DurationHours ?? 0;

            var isStaff = role == UserRole.Admin || role == UserRole.SuperAdmin;
            var usePayment = !isStaff && PaymentsEnabled;

            if (isStaff)
            {
                entity.Status = ReservationStatus.Confirmed;
                entity.PaidAt = DateTime.UtcNow;
                entity.PaymentTransactionId = "MANUAL";
                entity.ReviewedBy = userId;
                entity.ReviewedAt = DateTime.UtcNow;
            }
            else if (usePayment)
            {
                entity.Status = ReservationStatus.AwaitingPayment;
                entity.PaymentLockExpiresAt = DateTime.UtcNow.AddMinutes(10);
            }
            else
            {
                // الدفع متوقف مؤقتًا: الحجز ينتظر موافقة الأدمن
                entity.Status = ReservationStatus.Pending;
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

            // ── دفع إلكتروني ─────────────────────────────────────
            if (usePayment)
            {
                var merchantOrderId = $"RES-{entity.Id}-{Guid.NewGuid():N}";

                try
                {
                    var paymentOrder = await paymentGateway.CreatePaymentOrderAsync(
                        merchantOrderId: merchantOrderId,
                        amountCents: ToCents(entity.TotalPrice));

                    if (string.IsNullOrWhiteSpace(paymentOrder.PaymentUrl))
                    {
                        throw new BusinessException(
                            messageAr: "تعذر إنشاء رابط الدفع.",
                            messageEn: "Failed to create payment URL.");
                    }

                    entity.MerchantOrderId = merchantOrderId;
                    entity.PaymentTransactionId = paymentOrder.OrderId;

                    repo.Update(entity);
                    await repo.SaveChangesAsync();

                    var dtoResult = mapper.ToDto(entity);
                    dtoResult.PaymentUrl = paymentOrder.PaymentUrl;
                    return dtoResult;
                }
                catch (PaymobPaymentGateway.PaymobException ex)
                {
                    Console.WriteLine($"Paymob Create Error [{ex.StatusCode}]: {ex.ResponseBody}");
                    throw new BusinessException(
                        messageAr: "تعذر إنشاء عملية الدفع حالياً. يرجى المحاولة مرة أخرى.",
                        messageEn: "Unable to create the payment order. Please try again.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Paymob HTTP Error: {ex.Message}");
                    throw new BusinessException(
                        messageAr: "تعذر الاتصال بخدمة الدفع حالياً.",
                        messageEn: "Unable to connect to the payment service.");
                }
            }

            // ── حجز بدون دفع (Pending أو أدمن) ─────────────────────
            entity.YouthCenterActivity = activity;

            if (entity.Status == ReservationStatus.Pending)
            {
                await NotifyUserAsync(entity,
                    "تم استلام طلب الحجز",
                    "طلب حجزك قيد المراجعة من إدارة المركز.");

                await NotifyCenterAdminsAsync(
                    activity.YouthCenterId, entity.Id,
                    "طلب حجز جديد",
                    $"طلب حجز جديد بتاريخ {entity.Date:yyyy-MM-dd} في انتظار موافقتك.");
            }

            return mapper.ToDto(entity);
        }

        // ============================================================
        // CANCEL (by the player)
        // ============================================================

        public async Task<ReservationDto?> CancelAsync(int id, int userId)
        {
            var r = await repo.GetByIdWithRelationsAsync(id);

            // نفس الرد للغير موجود وللغير تابع للمستخدم
            if (r is null || r.UserId != userId)
                throw new NotFoundException("Reservation", id);

            if (r.Status is not (ReservationStatus.Pending
                              or ReservationStatus.AwaitingPayment
                              or ReservationStatus.Confirmed))
            {
                throw new BusinessException(
                    messageAr: "لا يمكن إلغاء هذا الحجز.",
                    messageEn: "This reservation cannot be cancelled.");
            }

            var nowCairo = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, CairoTz);
            var start = r.Date.Date + r.StartTime.ToTimeSpan();

            if (start <= nowCairo)
            {
                throw new BusinessException(
                    messageAr: "موعد الحجز بدأ بالفعل.",
                    messageEn: "The reservation has already started.");
            }

            var wasPaid = r.Status == ReservationStatus.Confirmed;

            if (wasPaid && start - nowCairo < TimeSpan.FromHours(CancelCutoffHours))
            {
                throw new BusinessException(
                    messageAr: $"لا يمكن إلغاء الحجز المؤكد قبل أقل من {CancelCutoffHours} ساعة من الموعد.",
                    messageEn: $"A confirmed reservation cannot be cancelled less than {CancelCutoffHours} hours before it starts.");
            }

            r.Status = ReservationStatus.Cancelled;
            r.CancelledAt = DateTime.UtcNow;
            r.PaymentLockExpiresAt = null;
            r.RejectionReason = "Cancelled by user";
            r.RefundPending = wasPaid
                              && r.PaidAt != null
                              && r.PaymentTransactionId != "MANUAL";

            repo.Update(r);
            await repo.SaveChangesAsync();

            await NotifyUserAsync(r,
                "تم إلغاء الحجز",
                r.RefundPending
                    ? "تم إلغاء حجزك وسيتم مراجعة الاسترداد من إدارة المركز."
                    : "تم إلغاء حجزك بنجاح.");

            await NotifyCenterAdminsAsync(
                r.YouthCenterActivity?.YouthCenterId, r.Id,
                "إلغاء حجز",
                r.RefundPending
                    ? $"{r.User?.Name} ألغى حجزًا مدفوعًا بتاريخ {r.Date:yyyy-MM-dd} ويحتاج استرداد."
                    : $"{r.User?.Name} ألغى حجزًا بتاريخ {r.Date:yyyy-MM-dd}.");

            return mapper.ToDto(r);
        }

        // ============================================================
        // DELETE
        // ============================================================

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
                {
                    throw new BusinessException(
                        "You can only delete Pending or Completed reservations.");
                }
            }
            else if (role == UserRole.Admin)
            {
                if (reservation.YouthCenterActivity?.YouthCenterId != adminCenterId)
                    throw new ForbiddenException();

                if (reservation.Status != ReservationStatus.Completed)
                {
                    throw new BusinessException(
                        "Admin can only delete Completed reservations.");
                }
            }

            repo.Delete(reservation);
            await repo.SaveChangesAsync();
        }

        // ============================================================
        // ACCEPT / REJECT
        // ============================================================

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

            repo.Update(reservation);
            await repo.SaveChangesAsync();

            await NotifyUserAsync(reservation,
                "تم تأكيد حجزك",
                $"تم تأكيد حجزك بتاريخ {reservation.Date:yyyy-MM-dd}.");
        }

        public async Task RejectAsync(
            int id, string reason, int reviewedBy, int? adminCenterId, UserRole role)
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

            await NotifyUserAsync(reservation,
                "تم رفض حجزك",
                string.IsNullOrWhiteSpace(reason)
                    ? "تم رفض حجزك."
                    : $"تم رفض حجزك: {reason}");
        }

        // ============================================================
        // UPDATE
        // ============================================================

        public async Task<ReservationDto?> UpdateReservationAsync(
            UpdateReservationDto dto,
            int id,
            int userId,
            UserRole role,
            int? adminCenterId)
        {
            var entity = await repo.GetByIdWithRelationsAsync(id)
                ?? throw new NotFoundException("Reservation", id);

            if (entity.Status != ReservationStatus.Pending &&
                entity.Status != ReservationStatus.AwaitingPayment)
            {
                throw new BusinessException(
                    "Only Pending or AwaitingPayment reservations can be updated.");
            }

            if (role == UserRole.User && entity.UserId != userId)
                throw new ForbiddenException();

            if (role == UserRole.Admin &&
                entity.YouthCenterActivity?.YouthCenterId != adminCenterId)
                throw new ForbiddenException();

            var newDate = dto.Date ?? entity.Date;
            var newStart = dto.StartTime ?? entity.StartTime;
            var newEnd = dto.EndTime ?? entity.EndTime;
            var newActivityId = dto.YouthCenterActivityId ?? entity.YouthCenterActivityId;

            var timeOrActivityChanged =
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
                {
                    throw new BusinessException(
                        messageAr: "هذا الموعد الجديد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                        messageEn: "The new time slot is no longer available — either booked or reserved for a match.");
                }
            }

            var wasAwaitingPayment = entity.Status == ReservationStatus.AwaitingPayment;
            var oldTotalPrice = entity.TotalPrice;

            entity = ApplyUpdate(entity, dto);

            if (timeOrActivityChanged)
            {
                var activity = await repo.GetYouthCenterActivity(newActivityId)
                    ?? throw new NotFoundException(
                        messageAr: Messages.Data.NoDataAr,
                        messageEn: Messages.Data.NoDataAr);

                entity.TotalPrice = activity.Price * entity.DurationHours ?? 0;
            }

            var paymentAmountChanged = oldTotalPrice != entity.TotalPrice;

            repo.Update(entity);
            await repo.SaveChangesAsync();

            var dtoResult = mapper.ToDto(entity);

            // أمر دفع جديد فقط لو الدفع مفعّل + الحجز منتظر دفع + المبلغ اتغيّر
            if (PaymentsEnabled && wasAwaitingPayment && paymentAmountChanged)
            {
                try
                {
                    var newMerchantOrderId = $"RES-{entity.Id}-{Guid.NewGuid():N}";

                    var paymentOrder = await paymentGateway.CreatePaymentOrderAsync(
                        merchantOrderId: newMerchantOrderId,
                        amountCents: ToCents(entity.TotalPrice));

                    if (string.IsNullOrWhiteSpace(paymentOrder.PaymentUrl))
                    {
                        throw new BusinessException(
                            messageAr: "تعذر إنشاء رابط الدفع.",
                            messageEn: "Failed to create payment URL.");
                    }

                    entity.MerchantOrderId = newMerchantOrderId;
                    entity.PaymentTransactionId = paymentOrder.OrderId;

                    repo.Update(entity);
                    await repo.SaveChangesAsync();

                    dtoResult.PaymentUrl = paymentOrder.PaymentUrl;
                }
                catch (PaymobPaymentGateway.PaymobException ex)
                {
                    Console.WriteLine($"Paymob Update Error [{ex.StatusCode}]: {ex.ResponseBody}");
                    throw new BusinessException(
                        messageAr: "تعذر تحديث عملية الدفع حالياً. يرجى المحاولة مرة أخرى.",
                        messageEn: "Unable to update the payment order. Please try again.");
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Paymob Update HTTP Error: {ex.Message}");
                    throw new BusinessException(
                        messageAr: "تعذر الاتصال بخدمة الدفع حالياً.",
                        messageEn: "Unable to connect to the payment service.");
                }
            }

            return dtoResult;
        }

        private static Reservation ApplyUpdate(Reservation reservation, UpdateReservationDto dto)
        {
            if (dto.Date.HasValue) reservation.Date = dto.Date.Value;
            if (dto.StartTime.HasValue) reservation.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) reservation.EndTime = dto.EndTime.Value;

            if (dto.YouthCenterActivityId.HasValue)
                reservation.YouthCenterActivityId = dto.YouthCenterActivityId.Value;

            if (dto.SpecialRequest != null)
                reservation.SpecialRequest = dto.SpecialRequest;

            // dto.TotalPrice متجاهل عمدًا: السعر دايمًا من السيرفر
            return reservation;
        }

        // ============================================================
        // AVAILABLE HOURS
        // ============================================================

        public async Task<List<PeakHourDto>?> GetAvailHoursAsync(AvailBookingDto dto) =>
            await repo.GetAvailBookingHoursAsync(dto);

        // ============================================================
        // LOCK FOR PAYMENT
        // ============================================================

        public async Task<ReservationDto?> LockForPaymentAsync(CreateReservationDto dto, int userId)
        {
            if (!PaymentsEnabled)
            {
                throw new BusinessException(
                    messageAr: "الدفع الإلكتروني غير متاح حالياً.",
                    messageEn: "Online payment is currently unavailable.");
            }

            var activity = await repo.GetYouthCenterActivity(dto.YouthCenterActivityId)
                ?? throw new NotFoundException(
                    messageAr: Messages.Data.NoDataAr,
                    messageEn: Messages.Data.NoDataAr);

            dto.UserId = userId;

            var conflict = await availabilityRepo.HasConflictAsync(
                dto.YouthCenterActivityId, dto.Date, dto.StartTime, dto.EndTime);

            if (conflict)
            {
                throw new BusinessException(
                    messageAr: "هذا الموعد لم يعد متاحاً — إما محجوز أو مخصص لمباراة.",
                    messageEn: "This time slot is no longer available — either booked or reserved for a match.");
            }

            var entity = mapper.CreateEntity(dto);
            entity.Status = ReservationStatus.AwaitingPayment;
            entity.PaymentLockExpiresAt = DateTime.UtcNow.AddMinutes(10);
            entity.TotalPrice = activity.Price * entity.DurationHours ?? 0;

            try
            {
                await repo.AddAsync(entity);
                await repo.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new BusinessException("This time slot was just taken by another user.");
            }

            return mapper.ToDto(entity);
        }

        // ============================================================
        // PAY AGAIN
        // ============================================================

        public async Task<ReservationDto?> PayAgainAsync(int reservationId, int userId)
        {
            if (!PaymentsEnabled)
            {
                throw new BusinessException(
                    messageAr: "الدفع الإلكتروني غير متاح حالياً.",
                    messageEn: "Online payment is currently unavailable.");
            }

            var reservation = await repo.GetByIdWithRelationsAsync(reservationId)
                ?? throw new NotFoundException("Reservation", reservationId);

            if (reservation.UserId != userId)
                throw new ForbiddenException();

            if (reservation.Status != ReservationStatus.AwaitingPayment)
            {
                throw new BusinessException(
                    messageAr: "هذا الحجز لا يحتاج إلى دفع حالياً.",
                    messageEn: "This reservation does not require payment.");
            }

            var amountCents = ToCents(reservation.TotalPrice);

            // ── 1) محاولة إعادة استخدام الأمر القديم ──────────────
            if (!string.IsNullOrWhiteSpace(reservation.PaymentTransactionId))
            {
                try
                {
                    var existing = await paymentGateway.CreatePaymentForExistingOrderAsync(
                        reservation.PaymentTransactionId, amountCents);

                    if (!string.IsNullOrWhiteSpace(existing.PaymentUrl))
                    {
                        reservation.PaymentLockExpiresAt = DateTime.UtcNow.AddMinutes(10);
                        repo.Update(reservation);
                        await repo.SaveChangesAsync();

                        var result = mapper.ToDto(reservation);
                        result.PaymentUrl = existing.PaymentUrl;
                        return result;
                    }
                }
                catch (PaymobPaymentGateway.PaymobException ex)
                {
                    Console.WriteLine($"Paymob Existing Order Error [{ex.StatusCode}]: {ex.ResponseBody}");
                    // نكمل وننشئ أمر جديد
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Paymob Existing Order HTTP Error: {ex.Message}");
                    throw new BusinessException(
                        messageAr: "تعذر الاتصال بخدمة الدفع حالياً.",
                        messageEn: "Unable to connect to the payment service.");
                }
            }

            // ── 2) أمر دفع جديد ───────────────────────────────────
            var newMerchantOrderId = $"RES-{reservation.Id}-{Guid.NewGuid():N}";

            try
            {
                var paymentOrder = await paymentGateway.CreatePaymentOrderAsync(
                    merchantOrderId: newMerchantOrderId,
                    amountCents: amountCents);

                if (string.IsNullOrWhiteSpace(paymentOrder.PaymentUrl))
                {
                    throw new BusinessException(
                        messageAr: "تعذر إنشاء رابط الدفع.",
                        messageEn: "Failed to create payment URL.");
                }

                reservation.MerchantOrderId = newMerchantOrderId;
                reservation.PaymentTransactionId = paymentOrder.OrderId;
                reservation.PaymentLockExpiresAt = DateTime.UtcNow.AddMinutes(10);

                repo.Update(reservation);
                await repo.SaveChangesAsync();

                var result = mapper.ToDto(reservation);
                result.PaymentUrl = paymentOrder.PaymentUrl;
                return result;
            }
            catch (PaymobPaymentGateway.PaymobException ex)
            {
                Console.WriteLine($"Paymob New Order Error [{ex.StatusCode}]: {ex.ResponseBody}");
                throw new BusinessException(
                    messageAr: "تعذر إنشاء عملية دفع جديدة حالياً. يرجى المحاولة مرة أخرى.",
                    messageEn: "Unable to create a new payment order. Please try again.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Paymob New Order HTTP Error: {ex.Message}");
                throw new BusinessException(
                    messageAr: "تعذر الاتصال بخدمة الدفع حالياً.",
                    messageEn: "Unable to connect to the payment service.");
            }
        }

        // ============================================================
        // CONFIRM PAYMENT  (يُستدعى من الـ webhook فقط)
        // ============================================================

        public async Task<ReservationDto?> ConfirmPaymentAsync(
            int reservationId, ConfirmPaymentDto dto)
        {
            var reservation = await repo.GetByIdWithRelationsAsync(reservationId)
                ?? throw new NotFoundException("Reservation", reservationId);

            // دفع وصل لحجز ملغي/منتهي: الفلوس اتسحبت → لازم استرداد
            if (reservation.Status == ReservationStatus.Cancelled)
            {
                reservation.RefundPending = true;
                reservation.PaymentTransactionId = dto.TransactionId;
                reservation.PaidAt = DateTime.UtcNow;

                repo.Update(reservation);
                await repo.SaveChangesAsync();

                await NotifyCenterAdminsAsync(
                    reservation.YouthCenterActivity?.YouthCenterId, reservation.Id,
                    "دفع على حجز ملغي",
                    $"وصل دفع على حجز ملغي رقم {reservation.Id} ويحتاج استرداد.");

                throw new BusinessException(
                    messageAr: "الحجز ملغي. سيتم استرداد المبلغ.",
                    messageEn: "Reservation is cancelled. The payment will be refunded.");
            }

            if (reservation.Status != ReservationStatus.AwaitingPayment)
                throw new BusinessException("Reservation is not awaiting payment.");

            if (reservation.PaymentLockExpiresAt.HasValue &&
                reservation.PaymentLockExpiresAt < DateTime.UtcNow)
            {
                throw new BusinessException("Payment window expired. Please pay again.");
            }

            if (dto.AmountPaid.HasValue && dto.AmountPaid.Value != reservation.TotalPrice)
                throw new BusinessException("Paid amount does not match reservation price.");

            reservation.Status = ReservationStatus.Confirmed;
            reservation.PaymentTransactionId = dto.TransactionId;
            reservation.PaidAt = DateTime.UtcNow;
            reservation.PaymentLockExpiresAt = null;

            repo.Update(reservation);
            await repo.SaveChangesAsync();

            await NotifyUserAsync(reservation,
                "تم الدفع بنجاح",
                "تم تأكيد حجزك بعد إتمام الدفع.");

            await NotifyCenterAdminsAsync(
                reservation.YouthCenterActivity?.YouthCenterId, reservation.Id,
                "حجز جديد مدفوع",
                $"حجز جديد مدفوع بتاريخ {reservation.Date:yyyy-MM-dd}.");

            return mapper.ToDto(reservation);
        }

        // ============================================================
        // PAYMENT FAILED
        // ============================================================

        public async Task FailPaymentAsync(int reservationId, string reason)
        {
            var reservation = await repo.GetByIdWithRelationsAsync(reservationId)
                ?? throw new NotFoundException("Reservation", reservationId);

            if (reservation.Status != ReservationStatus.AwaitingPayment)
                return;

            // الحجز يفضل AwaitingPayment عشان اللاعب يقدر يدفع تاني
            reservation.RejectionReason = reason;

            repo.Update(reservation);
            await repo.SaveChangesAsync();
        }
    }
}