namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups
{
    public class RegenerateScheduleDto
    {
        /// false (الافتراضي) = احتفظ بالمباريات اللي الأدمن عدّلها يدوي
        public bool OverwriteManual { get; set; }
    }
}