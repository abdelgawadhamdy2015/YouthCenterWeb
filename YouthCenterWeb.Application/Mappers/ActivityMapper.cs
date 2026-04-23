using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.Models;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mapper
{
    public class ActivityMapper : IMapper<Activity, ActivityDto, CreateActivityDto>
    {
        public Activity CreateEntity(CreateActivityDto createDto)
        {
            return new Activity
            {
                Name = createDto.Name
            };
        }

        public ActivityDto ToDto(Activity entity)
        {
            return new ActivityDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public Activity ToEntity(ActivityDto dto)
        {
            return new Activity
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}