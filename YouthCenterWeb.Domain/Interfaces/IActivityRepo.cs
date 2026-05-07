using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

public interface IActivityRepo : IGenericRepo<Activity>
{
    public Task<List<Activity>> GetActivitiesByCenter(int youthCenterId);
}