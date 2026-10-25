using Microsoft.AspNetCore.SignalR;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Hubs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class AuthService(
        IAuthRepo authRepo,
        IOtpRepo otpRepo,
        IJwtService jwtService,
        IHubContext<SessionHub> sessionHub) : IAuthService
    {
        // Register stays the same, but Success() now needs a fresh session id.

        public async Task<BaseResponse<LoginData>> RegisterAsync(RegisterDto dto)
        {
            var emailVerified = await otpRepo.IsVerifiedAsync(dto.Email);
            if (!emailVerified)
                return Fail("Email is not verified. Please verify your OTP first.",
                             "البريد الإلكتروني غير موثق، يرجى التحقق من الرمز أولاً");

            var existing = await authRepo.GetByEmailAsync(dto.Email);
            if (existing != null)
                return Fail(Messages.Auth.EmailExistsEn, Messages.Auth.EmailExistsAr);

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Mobile = dto.Mobile,
                Role = UserRole.User,
                YouthCenterId = null,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await authRepo.AddAsync(user);
            await authRepo.SaveChangesAsync();

            await otpRepo.DeleteAllForEmailAsync(dto.Email);
            await otpRepo.SaveChangesAsync();

            return await SuccessAsync(user);
        }

        public async Task<BaseResponse<LoginData>> LoginAsync(LoginDto dto)
        {
            var user = await authRepo.GetByEmailAsync(dto.Email);

            if (user == null || user.IsDeleted ||
 !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
                return Fail("Invalid email or password.", "البريد أو كلمة المرور غير صحيحة");

            return await SuccessAsync(user);
        }

        public async Task<BaseResponse<LoginData>> CreateAdminAsync(CreateAdminDto dto)
        {
            // unchanged — no token issued here
            var existing = await authRepo.GetByEmailAsync(dto.Email);
            if (existing != null)
                return Fail("Email already exists.", "البريد مستخدم بالفعل");

            if (dto.YouthCenterId == null)
                return Fail("YouthCenterId is required for Admin.", "يجب تحديد مركز الشباب");

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Mobile = dto.Mobile,
                Role = UserRole.Admin,
                YouthCenterId = dto.YouthCenterId,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await authRepo.AddAsync(user);
            await authRepo.SaveChangesAsync();

            return new BaseResponse<LoginData>
            {
                Result = 1,
                Data = null,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.CreatedEn,
                    MessageAr = Messages.Data.CreatedAr
                }
            };
        }

        // ── shared helpers ────────────────────────────────────────────────────
        private async Task<BaseResponse<LoginData>> SuccessAsync(User user)
        {
            var sessionId = Guid.NewGuid();
            await authRepo.UpdateSessionAsync(user, sessionId);

            // notify any previously connected device for this user — instantly
            await sessionHub.Clients.Group($"user:{user.Id}")
                .SendAsync("ForceLogout", new
                {
                    reason = "logged_in_elsewhere",
                    at = DateTime.UtcNow
                });

            return new BaseResponse<LoginData>
            {
                Result = 1,
                Data = new LoginData
                {
                    Email = user.Email,
                    Name = user.Name,
                    Mobile = user.Mobile,
                    Role = user.Role,
                    FavActivityName = user.Activity?.NameAr,
                    FavActivityId = user.ActivityId,
                    YouthCenterName = user.YouthCenter?.NameAr,
                    Token = jwtService.GenerateToken(user, sessionId)
                },
                Alert = new Alert { MessageEn = "Success", MessageAr = "تمت العملية بنجاح" }
            };
        }

        private static BaseResponse<LoginData> Fail(string en, string ar) =>
            new()
            {
                Result = 0,
                Data = null,
                Alert = new Alert { MessageEn = en, MessageAr = ar }
            };

        public async Task<BaseResponse<object>> ResetPasswordAsync(ResetPasswordDto dto)
        {
            const string invalidEn = "Invalid or expired verification.";
            const string invalidAr = "التحقق غير صالح أو منتهي، اطلب كودًا جديدًا.";

            if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 8)
                return FailObj("Password must be at least 8 characters.",
                               "كلمة المرور يجب ألا تقل عن 8 أحرف.");

            // 1) لازم يكون في OTP موثّق وغير منتهي لهذا الإيميل
            var verified = await otpRepo.IsVerifiedAsync(dto.Email);
            var latest = await otpRepo.GetLatestAsync(dto.Email);

            if (!verified || latest is null || !latest.IsVerified || latest.ExpiresAt < DateTime.UtcNow)
                return FailObj(invalidEn, invalidAr);

            // 2) المستخدم لازم يكون موجود وغير محذوف
            var user = await authRepo.GetByEmailAsync(dto.Email);
            if (user is null || user.IsDeleted)
                return FailObj(invalidEn, invalidAr); // نفس الرسالة: منع كشف الإيميلات

            // 3) غيّر الباسورد + أبطل الجلسات القديمة
            await authRepo.UpdatePasswordAsync(user, BCrypt.Net.BCrypt.HashPassword(dto.NewPassword));

            // 4) احرق الـ OTP
            await otpRepo.DeleteAllForEmailAsync(dto.Email);
            await otpRepo.SaveChangesAsync();

            // 5) اطرد الأجهزة المتصلة
            await sessionHub.Clients.Group($"user:{user.Id}")
                .SendAsync("ForceLogout", new
                {
                    reason = "password_changed",
                    at = DateTime.UtcNow
                });

            return new BaseResponse<object>
            {
                Result = 1,
                Data = null,
                Alert = new Alert
                {
                    MessageEn = "Password updated successfully.",
                    MessageAr = "تم تحديث كلمة المرور بنجاح."
                }
            };
        }

        private static BaseResponse<object> FailObj(string en, string ar) =>
            new()
            {
                Result = 0,
                Data = null,
                Alert = new Alert { MessageEn = en, MessageAr = ar }
            };
    }
}