using YouthCenterWeb.Application.Interfaces;
using BCrypt.Net;
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.Data.DTO;
using YouthCenterWeb.InterFaces;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Repositories;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepo _authRepo;
        private readonly IJwtService _jwtService;

        public AuthService(IAuthRepo authRepo, IJwtService jwtService)
        {
            _authRepo = authRepo;
            _jwtService = jwtService;
        }

        public async Task<BaseResponse<LoginData>> LoginAsync(LoginDto dto)
        {
            var user = await _authRepo.GetByEmailAsync(dto.Email);

            if (user == null)
            {
                return Fail("Invalid email", "البريد الإلكتروني غير صحيح");
            }

            var valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.HashedPassword);

            if (!valid)
            {
                return Fail("Invalid password", "كلمة المرور غير صحيحة");
            }



            return new BaseResponse<LoginData>
            {
                Result = 1,
                Data = new LoginData
                {
                    Email = user.Email,
                    Name = user.Name,
                    Phone = user.Mobile ?? "",
                    RoleId = user.RoleId,
                    Token = _jwtService.GenerateToken(user)
                },
                Alert = new Alert
                {
                    MessageEn = "Login successful",
                    MessageAr = "تم تسجيل الدخول بنجاح"
                }
            };
        }

        public async Task<BaseResponse<LoginData>> RegisterAsync(CreateUserDto dto)
        {
            var existing = await _authRepo.GetByEmailAsync(dto.Email);

            if (existing != null)
            {
                return Fail("Email exists", "البريد مستخدم بالفعل");
            }

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Mobile = dto.Mobile,
                YouthCenterId = dto.YouthCenterId,
                RoleId = dto.RoleId,
                HashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _authRepo.AddAsync(user);
            await _authRepo.SaveChangesAsync();

            return new BaseResponse<LoginData>
            {
                Result = 1,
                Alert = new Alert
                {
                    MessageEn = "User registered successfully",
                    MessageAr = "تم تسجيل المستخدم بنجاح"
                }
            };
        }

        private BaseResponse<LoginData> Fail(string en, string ar)
        {
            return new BaseResponse<LoginData>
            {
                Result = 0,
                Alert = new Alert
                {
                    MessageEn = en,
                    MessageAr = ar
                }
            };
        }
    }
}