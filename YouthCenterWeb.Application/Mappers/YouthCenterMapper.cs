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
                NameAr = createDto.NameAr,
                NameEn = createDto.NameEn,
                Email = createDto.Email,
                Longitude = createDto.Longitude,
                Latitude = createDto.Latitude,
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
                NameAr = entity.NameAr,
                NameEn = entity.NameEn,
                Email = entity.Email,
                Longitude = entity.Longitude,
                Latitude = entity.Latitude,
                Address = entity.Address,
                Phone = entity.Phone,
                IsActive = entity.IsActive,
                Description = entity.Description,
                ActivitiesIds = entity.YouthCenterActivities.Select(a => a.ActivityId).ToList(),
                ActivitiesNames = entity.YouthCenterActivities.Select(a => a.Activity?.NameAr ?? "").ToList(),
            };
        }

        public YouthCenter ToEntity(YouthCenterDto dto)
        {
            return new YouthCenter
            {
                Id = dto.Id,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn,
                Email = dto.Email,
                Longitude = dto.Longitude,
                Latitude = dto.Latitude,
                Address = dto.Address,
                Phone = dto.Phone,
                Description = dto.Description,
                IsActive = dto.IsActive
            };
        }

        public YouthCenter UpdateEntity(YouthCenter entity, YouthCenterDto updateDto)
        {
            entity.NameAr = updateDto.NameAr;
            entity.NameEn = updateDto.NameEn;
            entity.Email = updateDto.Email;
            entity.Address = updateDto.Address;
            entity.Longitude = updateDto.Longitude;
            entity.Latitude = updateDto.Latitude;
            entity.Phone = updateDto.Phone;
            entity.Description = updateDto.Description;
            entity.IsActive = updateDto.IsActive;
            return entity;
        }
    }
}