using System.Linq.Expressions;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface IUserRepo
    {
        Task<List<User>> GetAllAsync(params Expression<Func<User, object>>[] includes);
        Task<User?> GetByIdAsync(int id, params Expression<Func<User, object>>[] includes);
        Task<User?> GetByEmailAsync(string email, params Expression<Func<User, object>>[] includes);
        Task AddAsync(User entity);
        void Update(User entity);
        void Delete(User entity);
        Task SaveChangesAsync();
    }
}