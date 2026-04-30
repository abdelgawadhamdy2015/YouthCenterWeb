// Helper extension — use anywhere you need the current user's info
using System.Security.Claims;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Constants;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Extentions
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
            => int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

        public static UserRole GetRole(this ClaimsPrincipal user)
            => Enum.Parse<UserRole>(user.FindFirstValue(ClaimTypes.Role)!);

        public static int? GetYouthCenterId(this ClaimsPrincipal user)
        {
            var val = user.FindFirstValue(ClaimsConstants.YouthCenterId);
            return string.IsNullOrEmpty(val) ? null : int.Parse(val);
        }
    }
}