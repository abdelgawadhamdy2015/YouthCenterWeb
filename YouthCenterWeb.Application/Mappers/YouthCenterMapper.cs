using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mappers
{
    public class YouthCenterMapper : IMapper<YouthCenter, YouthCenterDto, CreateYouthCenterDto>
    {
        public YouthCenter CreateEntity(CreateYouthCenterDto createDto)
        {
            return new YouthCenter
            {
                Name = createDto.Name,
                Address = createDto.Address,
                Phone = createDto.Phone,
                Description = createDto.Description,
                IsActive = createDto.IsActive
            };
        }

        public YouthCenterDto ToDto(YouthCenter entity)
        {
            return new YouthCenterDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Address = entity.Address,
                Phone = entity.Phone,
                IsActive = entity.IsActive,
                Description = entity.Description,
                ActivitiesIds = entity.YouthCenterActivities.Select(a => a.ActivityId).ToList(),
                ActivitiesNames = entity.YouthCenterActivities.Select(a => a.Activity?.Name ?? "").ToList(),
            };
        }

        public YouthCenter ToEntity(YouthCenterDto dto)
        {
            return new YouthCenter
            {
                Id = dto.Id,
                Name = dto.Name,
                Address = dto.Address,
                Phone = dto.Phone,
                Description = dto.Description,
                IsActive = dto.IsActive
            };
        }

        public YouthCenter UpdateEntity(YouthCenter entity, YouthCenterDto updateDto)
        {
            entity.Name = updateDto.Name;
            entity.Address = updateDto.Address;
            entity.Phone = updateDto.Phone;
            entity.Description = updateDto.Description;
            entity.IsActive = updateDto.IsActive;
            return entity;
        }
    }
}