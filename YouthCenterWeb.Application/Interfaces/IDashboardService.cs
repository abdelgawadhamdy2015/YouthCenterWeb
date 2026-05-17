using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IDashboardService
    {
        public Task<DashboardOverviewDto> GetDashboardOverviewDtoAsync(int? youthCenterId);
    }

}