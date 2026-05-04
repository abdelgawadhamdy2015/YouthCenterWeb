using System.Linq.Expressions;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

public interface IYouthCenterActivityRepo : IGenericRepo<YouthCenterActivity>
{
    Task<List<YouthCenterActivity>?> GetByActivityId(int? activityId, params Expression<Func<YouthCenterActivity, object>>[] includes);
    Task<List<YouthCenterActivity>?> GetByYouthCenterId(int? youthCenterId, params Expression<Func<YouthCenterActivity, object>>[] includes);

}