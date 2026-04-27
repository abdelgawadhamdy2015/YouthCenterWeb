using System.Linq.Expressions;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces
{
    public interface IGenericService<TEntity, TDto, TCreateDto> // أضفنا TEntity هنا للتعامل مع الـ Includes
    {
        Task<List<TDto>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes);

        Task<TDto?> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes);

        Task<TDto> CreateAsync(TCreateDto dto);

        Task<bool> DeleteAsync(int id);
    }
}