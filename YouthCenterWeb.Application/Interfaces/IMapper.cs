public interface IMapper<TEntity, TDto, TCreateDto>
{
    TDto ToDto(TEntity entity);
    TEntity ToEntity(TDto dto);

    TEntity CreateEntity(TCreateDto createDto);

    TEntity UpdateEntity(TEntity entity, TDto updateDto);
}