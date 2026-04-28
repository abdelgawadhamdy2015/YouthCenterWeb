using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController(IGenericService<Role, RoleDto, RoleDto> roleService) : ControllerBase
    {
        private readonly IGenericService<Role, RoleDto, RoleDto> service = roleService;

        [HttpGet]
        public async Task<IActionResult> getAllRoles()
        {
            List<RoleDto> roles = await service.GetAllAsync();
            if (roles.Count == 0) return NotFound(new BaseResponse<RoleDto>
            {
                Result = 0,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.NoDataAr,
                    MessageEn = Messages.Data.NoDataEn
                }
            });
            return Ok(new BaseResponse<List<RoleDto>>
            {
                Result = 1,
                Data = roles,
                Alert = new Alert
                {
                    MessageAr = Messages.Data.FoundAr,
                    MessageEn = Messages.Data.FoundEn
                }
            });
        }

        [HttpPost]
        public Task<IActionResult> CreateRole([FromBody] RoleDto roleDto)
        {
            return service.CreateAsync(roleDto).ContinueWith<IActionResult>(t =>
            {
                if (t.IsFaulted || t.Result == null) return BadRequest(new BaseResponse<RoleDto>
                {
                    Result = 0,
                    Alert = new Alert
                    {
                        MessageAr = Messages.Data.CreationFailedAr,
                        MessageEn = Messages.Data.CreationFailedEn
                    }
                });
                return Ok(new BaseResponse<RoleDto>
                {
                    Result = 1,
                    Data = t.Result,
                    Alert = new Alert
                    {
                        MessageAr = Messages.Data.CreatedAr,
                        MessageEn = Messages.Data.CreatedEn
                    }
                });
            });
        }

    }



}