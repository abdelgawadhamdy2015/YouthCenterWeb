using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Infrastructure.Data;

namespace YouthCenterWeb.YouthCenterWeb.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NotificationController(AppDbContext db) : ControllerBase
    {
        private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await db.Notifications
                .Where(n => n.UserId == UserId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
            return Ok(new BaseResponse<List<UserNotification>> { Result = 1, Data = items });
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> UnreadCount()
        {
            var count = await db.Notifications.CountAsync(n => n.UserId == UserId && !n.IsRead);
            return Ok(new BaseResponse<int> { Result = 1, Data = count });
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            await db.Notifications
                .Where(n => n.Id == id && n.UserId == UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
            return Ok(new BaseResponse<object> { Result = 1 });
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> ReadAll()
        {
            await db.Notifications
                .Where(n => n.UserId == UserId && !n.IsRead)
                .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));
            return Ok(new BaseResponse<object> { Result = 1 });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await db.Notifications
                .Where(n => n.Id == id && n.UserId == UserId)
                .ExecuteDeleteAsync();
            return Ok(new BaseResponse<object> { Result = 1 });
        }
    }
}