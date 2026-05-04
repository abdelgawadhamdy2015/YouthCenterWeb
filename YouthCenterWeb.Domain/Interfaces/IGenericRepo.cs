using System.Linq.Expressions;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

public interface IGenericRepo<T> where T : class
{
    Task<List<T>?> GetAllAsync(params Expression<Func<T, object>>[] includes);

    Task<T?> GetByIdAsync(int? id, params Expression<Func<T, object>>[] includes);

    Task AddAsync(T entity);

    Task UpdateAsync(T entity);

    Task DeleteAsync(T entity);

    Task SaveChangesAsync();
}