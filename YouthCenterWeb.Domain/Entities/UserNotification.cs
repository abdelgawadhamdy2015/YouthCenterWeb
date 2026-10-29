namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class UserNotification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Title { get; set; } = null!;
        public string Body { get; set; } = null!;
        public string? Type { get; set; }
        public int? TargetId { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}