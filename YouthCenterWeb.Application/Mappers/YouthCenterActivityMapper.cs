using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

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
            ActivityName = entity.Activity?.Name ?? string.Empty,
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
}