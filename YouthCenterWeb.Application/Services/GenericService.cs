using System.Linq.Expressions;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class GenericService<TEntity, TDto, TCreateDto>(
        IGenericRepo<TEntity> repo,
        IMapper<TEntity, TDto, TCreateDto> mapper) : IGenericService<TEntity, TDto>
        where TEntity : class
    {
        private readonly IGenericRepo<TEntity> _repo = repo;
        private readonly IMapper<TEntity, TDto, TCreateDto> _mapper = mapper;

        public async Task<List<TDto>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
        {
            // نمرر الـ includes للمستودع
            var entities = await _repo.GetAllAsync(includes);
            return entities.Select(_mapper.ToDto).ToList();
        }

        public async Task<TDto?> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes)
        {
            // نمرر الـ includes للمستودع
            var entity = await _repo.GetByIdAsync(id, includes);
            return entity == null ? default : _mapper.ToDto(entity);
        }

        public async Task<TDto> CreateAsync(TDto dto)
        {
            var entity = _mapper.ToEntity(dto);
            var result = await _repo.AddAsync(entity);
            await _repo.SaveChangesAsync();
            return _mapper.ToDto(result);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            bool deleted = await _repo.DeleteAsync(id);
            if (deleted) await _repo.SaveChangesAsync();
            return deleted;
        }
    }
}