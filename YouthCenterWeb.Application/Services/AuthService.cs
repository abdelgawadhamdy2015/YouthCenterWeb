using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Configurations;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Hubs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class AuthService(
        IAuthRepo authRepo,
        IOtpRepo otpRepo,
        IJwtService jwtService,
        IRefreshTokenRepo refreshRepo,
        IOptions<JwtOptions> jwtOptions,
        IHubContext<SessionHub> sessionHub) : IAuthService
    {
        // Register stays the same, but Success() now needs a fresh session id.
        private readonly JwtOptions _jwt = jwtOptions.Value;
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
        // ── Refresh ───────────────────────────────────────────────────────────────
        public async Task<BaseResponse<LoginData>> RefreshAsync(RefreshTokenDto dto)
        {
            var fail = Fail("Session expired. Please log in again.",
                            "انتهت الجلسة، سجّل الدخول مرة أخرى.");

            var oldHash = Hash(dto.RefreshToken);
            var stored = await refreshRepo.GetByHashAsync(oldHash);
            if (stored is null) return fail;

            if (stored.RevokedAt is not null)
            {
                // مهلة قصيرة لطلبات متزامنة شرعية. بعدها = إعادة استخدام = سرقة محتملة
                var grace = stored.ReplacedByHash is not null &&
                            DateTime.UtcNow - stored.RevokedAt < TimeSpan.FromSeconds(10);
                if (!grace) await refreshRepo.RevokeAllForUserAsync(stored.UserId);
                return fail;
            }

            if (stored.ExpiresAt <= DateTime.UtcNow) return fail;

            var user = await authRepo.GetByIdAsync(stored.UserId);
            if (user is null || user.IsDeleted || user.CurrentSessionId != stored.SessionId)
                return fail;                       // سجّل دخول من جهاز آخر/تغيّر الباسورد/حُذف

            var newRaw = GenerateRawToken();
            var newHash = Hash(newRaw);

            // ذري: لو طلب آخر سبقنا يرجع false
            if (!await refreshRepo.TryRevokeAsync(stored.Id, newHash)) return fail;

            await refreshRepo.AddAsync(NewEntity(user.Id, stored.SessionId, newHash));
            await refreshRepo.SaveChangesAsync();

            return new BaseResponse<LoginData>
            {
                Result = 1,
                Data = BuildLoginData(user, jwtService.GenerateToken(user, stored.SessionId), newRaw),
                Alert = new Alert { MessageEn = "Success", MessageAr = "تمت العملية بنجاح" }
            };
        }

        // ── Logout ────────────────────────────────────────────────────────────────
        public async Task<BaseResponse<object>> LogoutAsync(int userId)
        {
            await refreshRepo.RevokeAllForUserAsync(userId);
            var user = await authRepo.GetByIdAsync(userId);
            if (user is not null) await authRepo.ClearSessionAsync(user); // يبطل الـ access token فورًا

            return new BaseResponse<object>
            {
                Result = 1,
                Alert = new Alert { MessageEn = "Logged out", MessageAr = "تم تسجيل الخروج" }
            };
        }

        // ── helpers ───────────────────────────────────────────────────────────────
        private async Task<string> IssueRefreshTokenAsync(int userId, Guid sessionId)
        {
            var raw = GenerateRawToken();
            await refreshRepo.AddAsync(NewEntity(userId, sessionId, Hash(raw)));
            await refreshRepo.SaveChangesAsync();
            return raw;
        }

        private RefreshToken NewEntity(int userId, Guid sessionId, string hash) => new()
        {
            UserId = userId,
            SessionId = sessionId,
            TokenHash = hash,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays)
        };

        private static string GenerateRawToken() =>
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
                .Replace('+', '-').Replace('/', '_').TrimEnd('=');

        private static string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        // ── shared helpers ────────────────────────────────────────────────────
        private async Task<BaseResponse<LoginData>> SuccessAsync(User user)
        {
            var sessionId = Guid.NewGuid();
            await authRepo.UpdateSessionAsync(user, sessionId);

            // الجهاز القديم يفقد الـ refresh فورًا
            await refreshRepo.RevokeAllForUserAsync(user.Id);
            var refresh = await IssueRefreshTokenAsync(user.Id, sessionId);

            await sessionHub.Clients.Group($"user:{user.Id}")
                .SendAsync("ForceLogout", new { reason = "logged_in_elsewhere", at = DateTime.UtcNow });

            return new BaseResponse<LoginData>
            {
                Result = 1,
                Data = BuildLoginData(user, jwtService.GenerateToken(user, sessionId), refresh),
                Alert = new Alert { MessageEn = "Success", MessageAr = "تمت العملية بنجاح" }
            };
        }

        private static LoginData BuildLoginData(User user, string token, string refresh) => new()
        {
            Email = user.Email,
            Name = user.Name,
            Mobile = user.Mobile,
            Role = user.Role,
            FavActivityName = user.Activity?.NameAr,
            FavActivityId = user.ActivityId,
            YouthCenterName = user.YouthCenter?.NameAr,
            ReservationCount = user.ReservationCount,
            Token = token,
            RefreshToken = refresh
        };

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