public class EmailOtp
{
    public int Id { get; set; }
    public string Email { get; set; } = default!;
    public string CodeHash { get; set; } = default!;   // خزّن hash مش الكود نفسه
    public DateTime ExpiresAt { get; set; }
    public int FailedAttempts { get; set; }
    public bool Used { get; set; }
    public DateTime CreatedAt { get; set; }
}