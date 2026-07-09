using System.Security.Cryptography;
using BCrypt.Net;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class OtpService(IOtpRepo otpRepo, IEmailService emailService) : IOtpService
    {
        private const int ExpiryMinutes = 5;
        private const int ResendCooldown = 60;
        private const int MaxAttempts = 5;

        //──────────────────────────────────────────────────────────────
        // Send OTP
        //──────────────────────────────────────────────────────────────

        public async Task<BaseResponse<object>> SendOtpAsync(SendOtpDto dto)
        {
            var latest = await otpRepo.GetLatestAsync(dto.Email);

            if (latest != null)
            {
                var secondsSinceLast =
                    (DateTime.UtcNow - latest.CreatedAt).TotalSeconds;

                if (secondsSinceLast < ResendCooldown)
                {
                    var wait = (int)Math.Ceiling(ResendCooldown - secondsSinceLast);

                    return Fail(
                        $"Please wait {wait} seconds before requesting another code.",
                        $"انتظر {wait} ثانية قبل طلب رمز جديد."
                    );
                }
            }

            // Remove previous OTPs
            await otpRepo.DeleteAllForEmailAsync(dto.Email);

            var code = GenerateCode();

            var otp = new OtpEntry
            {
                Email = dto.Email,
                CodeHash = HashOtp(code),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
                Attempts = 0,
                IsVerified = false
            };

            await otpRepo.AddAsync(otp);
            await otpRepo.SaveChangesAsync();

#if DEBUG
            Console.WriteLine($"[OTP] {dto.Email} -> {code}");
#endif

            await emailService.SendAsync(
                dto.Email,
                "Your verification code",
                $"Your verification code is {code}. It expires in {ExpiryMinutes} minutes."
            );

#if DEBUG
            return new BaseResponse<object>
            {
                Result = 1,
                Data = new
                {
                    Otp = code
                },
                Alert = new Alert
                {
                    MessageEn = "OTP generated successfully.",
                    MessageAr = "تم إنشاء رمز التحقق."
                }
            };
#else
            return Ok(
                "OTP sent successfully.",
                "تم إرسال رمز التحقق بنجاح."
            );
#endif
        }

        //──────────────────────────────────────────────────────────────
        // Verify OTP
        //──────────────────────────────────────────────────────────────

        public async Task<BaseResponse<object>> VerifyOtpAsync(VerifyOtpDto dto)
        {
            var entry = await otpRepo.GetLatestAsync(dto.Email);

            if (entry == null)
            {
                return Fail(
                    "OTP not found.",
                    "لم يتم العثور على رمز التحقق."
                );
            }

            if (entry.IsVerified)
            {
                return Fail(
                    "OTP has already been used.",
                    "تم استخدام رمز التحقق بالفعل."
                );
            }

            if (entry.ExpiresAt < DateTime.UtcNow)
            {
                return Fail(
                    "OTP has expired.",
                    "انتهت صلاحية رمز التحقق."
                );
            }

            if (entry.Attempts >= MaxAttempts)
            {
                return Fail(
                    "Maximum verification attempts exceeded.",
                    "تم تجاوز الحد الأقصى للمحاولات."
                );
            }

            entry.Attempts++;

            if (!BCrypt.Net.BCrypt.Verify(dto.Otp, entry.CodeHash))
            {
                await otpRepo.SaveChangesAsync();

                return Fail(
                    "Invalid verification code.",
                    "رمز التحقق غير صحيح."
                );
            }

            entry.IsVerified = true;

            // Optional:
            // await otpRepo.DeleteAsync(entry);

            await otpRepo.SaveChangesAsync();

            return Ok(
                "Email verified successfully.",
                "تم التحقق من البريد الإلكتروني بنجاح."
            );
        }

        //──────────────────────────────────────────────────────────────
        // Helpers
        //──────────────────────────────────────────────────────────────

        private static string GenerateCode()
        {
            return RandomNumberGenerator
                .GetInt32(100000, 1000000)
                .ToString();
        }

        private static string HashOtp(string otp)
        {
            return BCrypt.Net.BCrypt.HashPassword(otp);
        }

        private static BaseResponse<object> Ok(string en, string ar)
        {
            return new BaseResponse<object>
            {
                Result = 1,
                Data = null,
                Alert = new Alert
                {
                    MessageEn = en,
                    MessageAr = ar
                }
            };
        }

        private static BaseResponse<object> Fail(string en, string ar)
        {
            return new BaseResponse<object>
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
}