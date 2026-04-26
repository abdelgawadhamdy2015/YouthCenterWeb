using System.Linq.Expressions;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces
{
    public interface IUserRepo : IGenericRepo<User>
    {
        Task<User?> GetByEmailAsync(string email, params Expression<Func<User, object>>[] includes);
    }
}