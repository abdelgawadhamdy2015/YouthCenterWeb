using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mappers
{
    public class YouthCenterActivityMapper : IMapper<YouthCenterActivity, YouthCenterActivityDto, CreateYouthCenterActivityDto>
    {
        public YouthCenterActivity CreateEntity(CreateYouthCenterActivityDto createDto)
        {
            return new YouthCenterActivity
            {
                YouthCenterId = createDto.YouthCenterId,
                ActivityId = createDto.ActivityId,
                IsAvailable = createDto.IsAvailable,
                MaxCapacity = createDto.MaxCapacity,
                Price = createDto.Price
            };
        }

        public YouthCenterActivityDto ToDto(YouthCenterActivity entity)
        {
            return new YouthCenterActivityDto
            {
                Id = entity.Id,
                ActivityNameAr = entity.Activity?.NameAr ?? string.Empty,
                ActivityNameEn = entity.Activity?.NameEn ?? string.Empty,
                YouthCenterNameAr = entity.YouthCenter?.NameAr ?? string.Empty,
                YouthCenterNameEn = entity.YouthCenter?.NameEn ?? string.Empty,
                YouthCenterId = entity.YouthCenterId,
                ActivityId = entity.ActivityId,
                IsAvailable = entity.IsAvailable,
                MaxCapacity = entity.MaxCapacity,
                Price = entity.Price
            };
        }

        public YouthCenterActivity ToEntity(YouthCenterActivityDto dto)
        {
            return new YouthCenterActivity
            {
                Id = dto.Id,
                YouthCenterId = dto.YouthCenterId,
                ActivityId = dto.ActivityId,
                IsAvailable = dto.IsAvailable,
                MaxCapacity = dto.MaxCapacity,
                Price = dto.Price
            };
        }

        public YouthCenterActivity UpdateEntity(YouthCenterActivity entity, YouthCenterActivityDto updateDto)
        {
            entity.YouthCenterId = updateDto.YouthCenterId;
            entity.ActivityId = updateDto.ActivityId;
            entity.IsAvailable = updateDto.IsAvailable;
            entity.MaxCapacity = updateDto.MaxCapacity;
            entity.Price = updateDto.Price;

            return entity;
        }
    }
}