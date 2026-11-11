using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Common.ICupScheduler
{
    public interface ICupScheduler
    {

        Task<int> ScheduleAsync(Cup cup, bool overwriteManual = false);
    }
}