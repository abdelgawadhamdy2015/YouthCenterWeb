using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController(IUserService userService) : ControllerBase
    {
        private readonly IUserService _userService = userService;

        [HttpGet("Email")]
        public async Task<IActionResult> GetByEmailAsync(string email)
        {
            var result = await _userService.GetByEmailAsync(email);

            return Ok(new BaseResponse<UserDto>
            {
                Result = result == null ? 0 : 1,
                Data = result == null ? null : result,
                Alert = new Alert
                {
                    MessageAr = result == null ? Messages.Data.NoDataAr : Messages.Data.FoundAr,
                    MessageEn = result == null ? Messages.Data.NoDataEn : Messages.Data.FoundEn
                }
            });
        }

        [HttpGet("Id")]
        public async Task<IActionResult> GetByIdAsync(int? id)
        {
            var result = await _userService.GetByIdAsync(id);

            return Ok(new BaseResponse<UserDto>

            {
                Result = result == null ? 0 : 1,
                Data = result ?? null,
                Alert = new Alert
                {
                    MessageAr = result == null ? Messages.Data.NoDataAr : Messages.Data.FoundAr,
                    MessageEn = result == null ? Messages.Data.NoDataEn : Messages.Data.FoundEn
                }
            });
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var result = await _userService.GetAllAsync();

            return Ok(new BaseResponse<List<UserDto>>
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
    }
}