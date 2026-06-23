
namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

using YouthCenterWeb.Domain.Entities;

public interface IYouthCenterRepo
{
    Task<List<YouthCenter>> GetAllAsync();
    Task<YouthCenter?> GetByIdAsync(int id);
    Task AddAsync(YouthCenter entity);
    void Update(YouthCenter entity);
    void Delete(YouthCenter entity);
    Task SaveChangesAsync();
}