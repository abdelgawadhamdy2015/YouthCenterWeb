using System.Linq.Expressions;
using YouthCenterWeb.Models;

public interface IYouthCenterRepo
{
    Task<YouthCenter?> GetByIdAsync(int id);
    Task<List<YouthCenter>> GetAllAsync();
    Task<YouthCenter> CreateAsync(YouthCenter youthCenter, List<int>? activitiesIds);
    Task UpdateAsync(YouthCenter youthCenter);
    Task DeleteAsync(int id);
}