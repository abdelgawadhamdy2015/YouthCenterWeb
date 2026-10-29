using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController(INotificationService service) : ControllerBase
    {
        private int UserId => User.GetUserId();

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(new BaseResponse<List<NotificationDto>>
            {
                Result = 1,
                Data = await service.GetMineAsync(UserId)
            });

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount() =>
            Ok(new BaseResponse<int>
            {
                Result = 1,
                Data = await service.GetUnreadCountAsync(UserId)
            });

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await service.MarkReadAsync(UserId, id);
            return Ok(new BaseResponse<object> { Result = 1 });
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> ReadAll()
        {
            await service.MarkAllReadAsync(UserId);
            return Ok(new BaseResponse<object> { Result = 1 });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await service.DeleteAsync(UserId, id);
            return Ok(new BaseResponse<object> { Result = 1 });
        }
    }
}