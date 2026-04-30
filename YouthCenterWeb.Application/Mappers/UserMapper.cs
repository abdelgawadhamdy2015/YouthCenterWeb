using YouthCenterWeb.Data.DTOs;
using YouthCenterWeb.DTOs;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

public class UserMapper : IMapper<User, UserDto, RegisterDto>
{
    public User CreateEntity(RegisterDto createDto)
    {
        return new User
        {
            Name = createDto.Name,
            Email = createDto.Email,
            Mobile = createDto.Mobile,
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
            YouthCenterId = entity.YouthCenterId ?? 0,
            Role = entity.Role,
            ReservationCount = entity.ReservationCount
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
            Role = dto.Role
        };
    }

    public User UpdateEntity(User entity, UserDto updateDto)
    {
        entity.Name = updateDto.Name;
        entity.Mobile = updateDto.Mobile;
        entity.ImageUrl = updateDto.ImageUrl;

        return entity;
    }
}