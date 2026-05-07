using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

public interface IActivityService : IGenericService<Activity, ActivityDto, CreateActivityDto>
{
    public Task<List<ActivityDto>> GetActivitiesByCenter(int youthCenterId);
}