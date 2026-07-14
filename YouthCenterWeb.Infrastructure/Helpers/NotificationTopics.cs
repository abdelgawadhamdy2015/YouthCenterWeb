namespace YouthCenterWeb.YouthCenterWeb.Infrastructure.Helpers
{
    public static class NotificationTopics
    {
        public static string Activity(int activityId)
            => $"activity_{activityId}";
    }
}