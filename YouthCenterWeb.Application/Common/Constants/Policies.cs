// Core/Constants/Policies.cs
namespace YouthCenterWeb.YouthCenterWeb.Application.Common.Constants
{
    public static class Policies
    {
        public const string RequireAuthenticated = "RequireAuthenticated";
        public const string RequireAdmin = "RequireAdmin";
        public const string RequireSuperAdmin = "RequireSuperAdmin";
        public const string RequireOwnCenter = "RequireOwnCenter";
        public const string RequireOwnUser = "RequireOwnUser";

    }

    public static class ClaimsConstants
    {
        public const string YouthCenterId = "YouthCenterId";
        public const string Role = "Role";
    }
}