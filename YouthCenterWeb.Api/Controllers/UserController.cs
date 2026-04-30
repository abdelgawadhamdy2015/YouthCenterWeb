// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using YouthCenterWeb.Application.Common.Constants;
// using YouthCenterWeb.DTOs;
// using YouthCenterWeb.Models;
// using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

// namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class UserController(IUserService userService) : ControllerBase
//     {
//         private readonly IUserService _userService = userService;

//         [HttpGet("Email")]
//         public async Task<IActionResult> GetByEmailAsync(string email)
//         {
//             var result = await _userService.GetByEmailAsync(email);

//             return Ok(new BaseResponse<UserDto>
//             {
//                 Result = result == null ? 0 : 1,
//                 Data = result == null ? null : result,
//                 Alert = new Alert
//                 {
//                     MessageAr = result == null ? Messages.Data.NoDataAr : Messages.Data.FoundAr,
//                     MessageEn = result == null ? Messages.Data.NoDataEn : Messages.Data.FoundEn
//                 }
//             });
//         }

//         [HttpGet("Id")]
//         public async Task<IActionResult> GetByIdAsync(int? id)
//         {
//             var result = await _userService.GetByIdAsync(id);

//             return Ok(new BaseResponse<UserDto>

//             {
//                 Result = result == null ? 0 : 1,
//                 Data = result ?? null,
//                 Alert = new Alert
//                 {
//                     MessageAr = result == null ? Messages.Data.NoDataAr : Messages.Data.FoundAr,
//                     MessageEn = result == null ? Messages.Data.NoDataEn : Messages.Data.FoundEn
//                 }
//             });
//         }
//         [HttpGet]
//         public async Task<IActionResult> GetAllAsync()
//         {
//             var result = await _userService.GetAllAsync();

//             return Ok(new BaseResponse<List<UserDto>>
//             {
//                 Result = 1,
//                 Data = result,
//                 Alert = new Alert
//                 {
//                     MessageAr = Messages.Data.FoundAr,
//                     MessageEn = Messages.Data.FoundEn

//                 }
//             });
//         }

//         [HttpPost("update")]
//         public async Task<IActionResult> UpdateAsync(UpdateUserDto dto)
//         {
//             var result = await _userService.UpdateAsync(dto);

//             return Ok(new BaseResponse<UserDto>
//             {
//                 Result = result == null ? 0 : 1,
//                 Data = result ?? null,
//                 Alert = new Alert
//                 {
//                     MessageAr = result == null ? Messages.Data.CreationFailedAr : Messages.Data.UpdatedAr,
//                     MessageEn = result == null ? Messages.Data.CreationFailedEn : Messages.Data.UpdatedEn
//                 }
//             });
//         }

//     }
// }

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = Policies.RequireAuthenticated)]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        // ── GET api/user ──────────────────────────────────────────────────────
        // SuperAdmin only — full user list
        [HttpGet]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();

            return Ok(new BaseResponse<List<UserDto>>
            {
                Result = result.Count != 0 ? 1 : 0,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = result.Count != 0 ? Messages.Data.FoundAr : Messages.Data.NoDataAr,
                    MessageEn = result.Count != 0 ? Messages.Data.FoundEn : Messages.Data.NoDataEn
                }
            });
        }

        // ── GET api/user/profile ──────────────────────────────────────────────
        // Every user gets their own profile — id comes from JWT
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _userService.GetByIdAsync(User.GetUserId())
                ?? throw new NotFoundException("User", User.GetUserId());

            return Ok(new BaseResponse<UserDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        // ── GET api/user/{id} ─────────────────────────────────────────────────
        // SuperAdmin → any id
        // Admin/User → redirected to own profile, id param ignored
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var role = User.GetRole();

            // non-SuperAdmin can only see themselves
            var targetId = role == UserRole.SuperAdmin
                ? id
                : User.GetUserId();

            var result = await _userService.GetByIdAsync(targetId)
                ?? throw new NotFoundException("User", targetId);

            return Ok(new BaseResponse<UserDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        // ── GET api/user/by-email ─────────────────────────────────────────────
        // SuperAdmin only — searching users by email is admin-level operation
        // email in header not query string — keeps it out of server logs
        [HttpGet("by-email")]
        [Authorize(Policy = Policies.RequireSuperAdmin)]
        public async Task<IActionResult> GetByEmail([FromHeader(Name = "X-User-Email")] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new BaseResponse<UserDto>
                {
                    Result = 0,
                    Data = null,
                    Alert = new Alert
                    {
                        MessageAr = Messages.Validation.EmailRequiredAr,
                        MessageEn = Messages.Validation.EmailRequiredEn
                    }
                });

            var result = await _userService.GetByEmailAsync(email)
                ?? throw new NotFoundException($"User with email '{email}' not found.");

            return Ok(new BaseResponse<UserDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        // ── PUT api/user/profile ──────────────────────────────────────────────
        [HttpPut("profile")]
        public async Task<IActionResult> Update(UpdateUserDto dto)
        {
            var result = await _userService.UpdateAsync(User.GetUserId(), dto);

            return Ok(new BaseResponse<UserDto>
            {
                Result = 1,
                Data = result,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.UpdatedAr,
                    MessageEn = Messages.Data.UpdatedEn
                }
            });
        }
    }
}