using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Services;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IYouthCenterActivityService : IGenericService<YouthCenterActivity, YouthCenterActivityDto, CreateYouthCenterActivityDto>
    {

        Task<List<YouthCenterActivityDto>?> GetYouthCenterActivitiesByActivityIdAsync(int ActivityId);
        Task<List<YouthCenterActivityDto>?> GetYouthCenterActivitiesByYouthCenterIdAsync(int YouthCenterId);

    }
}