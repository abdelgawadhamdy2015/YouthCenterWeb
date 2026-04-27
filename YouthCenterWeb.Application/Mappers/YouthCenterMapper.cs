using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;

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
                PricePerHour = createDto.PricePerHour,
                Description = createDto.Description,

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
                PricePerHour = entity.PricePerHour,
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
                PricePerHour = dto.PricePerHour,
                Description = dto.Description,

            };
        }

    }
}