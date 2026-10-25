namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities
{
    public class OtpEntry
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string CodeHash { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public int Attempts { get; set; }
        public bool IsVerified { get; set; }

    }
}