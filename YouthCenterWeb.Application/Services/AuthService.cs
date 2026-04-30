using BCrypt.Net;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class AuthService(
        IAuthRepo authRepo,
        IJwtService jwtService) : IAuthService
    {
        // ── Register — public, role locked to User ────────────────────────────
        public async Task<BaseResponse<LoginData>> RegisterAsync(RegisterDto dto)
        {
            var existing = await authRepo.GetByEmailAsync(dto.Email);
            if (existing != null)
                return Fail(Messages.Auth.EmailExistsEn, Messages.Auth.EmailExistsAr);

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Mobile = dto.Mobile,
                Role = UserRole.User,          // always User — never from dto
                YouthCenterId = null,                   // users have no center
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await authRepo.AddAsync(user);
            await authRepo.SaveChangesAsync();

            return Success(user);
        }

        // ── Login — all roles ─────────────────────────────────────────────────
        public async Task<BaseResponse<LoginData>> LoginAsync(LoginDto dto)
        {
            var user = await authRepo.GetByEmailAsync(dto.Email);

            // same message for both cases — don't tell attacker which one failed
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword))
                return Fail("Invalid email or password.", "البريد أو كلمة المرور غير صحيحة");

            return Success(user);
        }

        // ── Create Admin — SuperAdmin only (enforced in controller) ───────────
        public async Task<BaseResponse<LoginData>> CreateAdminAsync(CreateAdminDto dto)
        {
            var existing = await authRepo.GetByEmailAsync(dto.Email);
            if (existing != null)
                return Fail("Email already exists.", "البريد مستخدم بالفعل");

            // YouthCenterId required for admin — validate here
            if (dto.YouthCenterId == null)
                return Fail("YouthCenterId is required for Admin.", "يجب تحديد مركز الشباب");

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Mobile = dto.Mobile,
                Role = UserRole.Admin,         // always Admin — never from dto
                YouthCenterId = dto.YouthCenterId,      // set by SuperAdmin only
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await authRepo.AddAsync(user);
            await authRepo.SaveChangesAsync();

            // no token returned — SuperAdmin creates admin, admin logs in separately
            return new BaseResponse<LoginData>
            {
                Result = 1,
                Data = null,
                Alert = new Alert
                {
                    MessageEn = Messages.Data.CreatedEn,  // ← fixed: En in En field
                    MessageAr = Messages.Data.CreatedAr   // ← fixed: Ar in Ar field
                }
            };
        }

        // ── shared helpers ────────────────────────────────────────────────────
        private BaseResponse<LoginData> Success(User user) =>
            new()
            {
                Result = 1,
                Data = new LoginData
                {
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Mobile ?? "",
                    Role = user.Role,
                    Token = jwtService.GenerateToken(user)
                },
                Alert = new Alert
                {
                    MessageEn = "Success",
                    MessageAr = "تمت العملية بنجاح"
                }
            };

        private static BaseResponse<LoginData> Fail(string en, string ar) =>
            new()
            {
                Result = 0,
                Data = null,
                Alert = new Alert
                {
                    MessageEn = en,
                    MessageAr = ar
                }
            };
    }
}