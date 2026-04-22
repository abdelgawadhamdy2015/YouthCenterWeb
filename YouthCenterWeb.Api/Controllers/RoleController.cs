using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RoleController(IRoleService roleService) : ControllerBase
    {
        private readonly IRoleService service = roleService;

        [HttpGet]
        public async Task<IActionResult> getAllRoles()
        {
            List<RoleDto> roles = await service.GetAllAsync();
            if (roles.Count == 0) return NotFound(new BaseResponse<RoleDto>
            {
                Result = 0,
                Alert = new Alert
                {
                    MessageAr = " لا يوجد بيانات",
                    MessageEn = "No Data"
                }
            });
            return Ok(new BaseResponse<List<RoleDto>>
            {
                Result = 1,
                Data = roles,
                Alert = new Alert
                {
                    MessageAr = "تم العثور على البيانات",
                    MessageEn = "Roles found"
                }
            });
        }

    }

}