namespace YouthCenterWeb.YouthCenterWeb.Application.Common.helper;

public static class CairoClock
{
    private static readonly TimeZoneInfo Tz = Resolve();

    private static TimeZoneInfo Resolve()
    {
        try { return TimeZoneInfo.FindSystemTimeZoneById("Africa/Cairo"); }
        catch { return TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time"); }
    }

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Tz);
    public static DateTime Today => Now.Date;
}