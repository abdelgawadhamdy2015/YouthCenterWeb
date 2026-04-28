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
                Location = createDto.Location,
                Mobile = createDto.Mobile,
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
                Location = entity.Location,
                Mobile = entity.Mobile,
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
                Location = dto.Location,
                Mobile = dto.Mobile,
                Description = dto.Description,
                IsActive = dto.IsActive
            };
        }

    }
}