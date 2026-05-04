using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mapper
{
    public class ActivityMapper : IMapper<Activity, ActivityDto, CreateActivityDto>
    {
        public Activity CreateEntity(CreateActivityDto createDto)
        {
            return new Activity
            {
                NameAr = createDto.NameAr,
                NameEn = createDto.NameEn

            };
        }

        public ActivityDto ToDto(Activity entity)
        {
            return new ActivityDto
            {
                Id = entity.Id,
                NameAr = entity.NameAr,
                NameEn = entity.NameEn

            };
        }

        public Activity ToEntity(ActivityDto dto)
        {
            return new Activity
            {
                Id = dto.Id,
                NameAr = dto.NameAr,
                NameEn = dto.NameEn

            };
        }

        public Activity UpdateEntity(Activity entity, ActivityDto updateDto)
        {
            entity.NameAr = updateDto.NameAr;
            entity.NameEn = updateDto.NameEn;
            return entity;
        }
    }
}