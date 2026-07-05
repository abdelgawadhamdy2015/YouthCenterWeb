using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mappers
{
    public class UserMapper : IMapper<User, UserDto, RegisterDto>
    {
        public User CreateEntity(RegisterDto createDto)
        {
            return new User
            {
                Name = createDto.Name,
                Email = createDto.Email,
                Mobile = createDto.Mobile,
                Bio = createDto.Bio,
                ActivityId = createDto.FavActivityId,

            };
        }

        public UserDto ToDto(User entity)
        {
            return new UserDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Email = entity.Email,
                Mobile = entity.Mobile,
                ImageUrl = entity.ImageUrl,
                YouthCenterId = entity.YouthCenterId,
                YouthCenterName = entity.YouthCenter?.NameAr,
                Role = entity.Role,
                ReservationCount = entity.ReservationCount,
                FavActivityId = entity.ActivityId,
                Bio = entity.Bio
            };
        }

        public User ToEntity(UserDto dto)
        {
            return new User
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email,
                Mobile = dto.Mobile,
                ImageUrl = dto.ImageUrl,
                YouthCenterId = dto.YouthCenterId,
                Role = dto.Role,
                Bio = dto.Bio,
                ActivityId = dto.FavActivityId
            };
        }

        public User UpdateEntity(User entity, UserDto updateDto)
        {
            entity.Name = updateDto.Name;
            entity.Mobile = updateDto.Mobile;
            entity.ImageUrl = updateDto.ImageUrl;
            entity.Bio = updateDto.Bio;
            entity.ActivityId = updateDto.FavActivityId;

            return entity;
        }
    }
}