// Domain/Interfaces/IYouthCenterRepo.cs
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface IYouthCenterRepo
    {
        Task<List<YouthCenter>> GetAllAsync();
        Task<YouthCenter?> GetByIdAsync(int id);
        Task AddAsync(YouthCenter entity);
        void Update(YouthCenter entity);
        void Delete(YouthCenter entity);
        Task SaveChangesAsync();
    }
}