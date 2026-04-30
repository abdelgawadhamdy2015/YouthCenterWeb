// Application/Services/YouthCenterService.cs
using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Exeptions;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Interfaces;
using YouthCenterWeb.YouthCenterWeb.Domain.Interfaces;

namespace YouthCenterWeb.YouthCenterWeb.Application.Services
{
    public class YouthCenterService(
        IYouthCenterRepo repo,
        IMapper<YouthCenter, YouthCenterDto, CreateYouthCenterDto> mapper)
        : IYouthCenterService
    {
        public async Task<List<YouthCenterDto>> GetAllAsync()
        {
            var entities = await repo.GetAllAsync();
            return entities.Select(mapper.ToDto).ToList();
        }

        public async Task<YouthCenterDto?> GetByIdAsync(int id)
        {
            var entity = await repo.GetByIdAsync(id);
            return entity == null ? null : mapper.ToDto(entity);
        }

        public async Task<YouthCenterDto> CreateAsync(CreateYouthCenterDto dto)
        {
            var entity = mapper.CreateEntity(dto);

            await repo.AddAsync(entity);
            await repo.SaveChangesAsync();

            return mapper.ToDto(entity);
        }

        public async Task<YouthCenterDto> UpdateAsync(int id, UpdatedYouthCenterDto dto)
        {
            var entity = await repo.GetByIdAsync(id)
                ?? throw new NotFoundException("YouthCenter", id);

            // update only the fields that belong to the caller
            entity.Name = dto.Name;
            entity.Address = dto.Address;
            entity.City = dto.City;
            entity.Phone = dto.Phone;
            entity.Email = dto.Email;
            entity.Description = dto.Description;

            repo.Update(entity);
            await repo.SaveChangesAsync();

            return mapper.ToDto(entity);
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await repo.GetByIdAsync(id)
                ?? throw new NotFoundException("YouthCenter", id);

            repo.Delete(entity);
            await repo.SaveChangesAsync();
        }
    }
}