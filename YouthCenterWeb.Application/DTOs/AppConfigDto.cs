namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs;

public class AppConfigDto
{
    public bool PaymentsEnabled { get; set; }
    public int CancelCutoffHours { get; set; }
    public string MinAppVersion { get; set; } = "0.0.0";
    public string LatestAppVersion { get; set; } = "0.0.0";
    public string? StoreUrlAndroid { get; set; }
    public string? StoreUrlIos { get; set; }
}