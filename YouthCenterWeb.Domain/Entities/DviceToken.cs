
namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities;

public class DeviceToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = null!;
    public string Platform { get; set; } = null!; // "ios" | "android"
    public DateTime UpdatedAt { get; set; }
}