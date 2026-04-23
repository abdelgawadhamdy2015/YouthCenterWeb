using System.Linq.Expressions;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IGenericService<TEntity, TDto> // أضفنا TEntity هنا للتعامل مع الـ Includes
    {
        Task<List<TDto>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);

        Task<TDto?> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes);

        Task<TDto> CreateAsync(TDto dto);

        Task<bool> DeleteAsync(int id);
    }
}