using System.Linq.Expressions;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class GenericService<TEntity, TDto, TCreateDto>(
        IGenericRepo<TEntity> repo,
        IMapper<TEntity, TDto, TCreateDto> mapper) : IGenericService<TEntity, TDto, TCreateDto>
        where TEntity : class
    {
        private readonly IGenericRepo<TEntity> _repo = repo;
        private readonly IMapper<TEntity, TDto, TCreateDto> _mapper = mapper;

        public async Task<List<TDto>> GetAllAsync(params Expression<Func<TEntity, object>>[] includes)
        {

            var entities = await _repo.GetAllAsync(includes);
            return entities.Select(_mapper.ToDto).ToList();
        }

        public async Task<TDto?> GetByIdAsync(int id, params Expression<Func<TEntity, object>>[] includes)
        {
            var entity = await _repo.GetByIdAsync(id, includes);
            return entity == null ? default : _mapper.ToDto(entity);
        }

        public async Task<TDto> CreateAsync(TCreateDto dto)
        {
            var entity = _mapper.CreateEntity(dto);
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

        public async Task<TDto> UpdateAsync(int id, TDto dto)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) throw new NotFoundException(typeof(TEntity).Name, id);

            entity = _mapper.UpdateEntity(entity, dto);
            var result = await _repo.UpdateAsync(entity);
            await _repo.SaveChangesAsync();
            return _mapper.ToDto(result);
        }

    }
}