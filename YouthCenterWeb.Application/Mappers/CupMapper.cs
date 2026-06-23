using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mappers;

public class CupMapper : IMapper<Cup, CupDto, CreateCupDto>
{
    public CupDto ToDto(Cup entity)
    {
        return new CupDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,

            ActivityId = entity.ActivityId,
            ActivityName = entity.Activity?.NameAr ?? string.Empty,

            YouthCenterId = entity.YouthCenterId,
            YouthCenterName = entity.YouthCenter?.NameAr ?? string.Empty,

            CreatedByAdminId = entity.CreatedByAdminId,
            CreatedByAdminName = entity.CreatedByAdmin?.Name ?? string.Empty,

            Format = entity.Format,
            Status = entity.Status,

            MaxTeams = entity.MaxTeams,
            CurrentTeamCount = entity.CurrentTeamCount,

            EntryFee = entity.EntryFee,

            RegistrationDeadline = entity.RegistrationDeadline,
            StartDate = entity.StartDate,
            CreatedAt = entity.CreatedAt,

            TeamsCount = entity.Teams?.Count ?? 0,
            MatchesCount = entity.Matches?.Count ?? 0
        };
    }

    public Cup ToEntity(CupDto dto)
    {
        return new Cup
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,

            ActivityId = dto.ActivityId,
            YouthCenterId = dto.YouthCenterId,
            CreatedByAdminId = dto.CreatedByAdminId,

            Format = dto.Format,
            Status = dto.Status,

            MaxTeams = dto.MaxTeams,
            CurrentTeamCount = dto.CurrentTeamCount,

            EntryFee = dto.EntryFee,

            RegistrationDeadline = dto.RegistrationDeadline,
            StartDate = dto.StartDate,
            CreatedAt = dto.CreatedAt
        };
    }

    public Cup CreateEntity(CreateCupDto createDto)
    {
        return new Cup
        {
            Name = createDto.Name,
            Description = createDto.Description,

            ActivityId = createDto.ActivityId,
            YouthCenterId = createDto.YouthCenterId,
            CreatedByAdminId = createDto.CreatedByAdminId,

            Format = createDto.Format,
            Status = CupStatus.Draft,

            MaxTeams = createDto.MaxTeams,
            CurrentTeamCount = 0,

            EntryFee = createDto.EntryFee,

            RegistrationDeadline = createDto.RegistrationDeadline,
            StartDate = createDto.StartDate,

            CreatedAt = DateTime.UtcNow
        };
    }

    public Cup UpdateEntity(Cup entity, CupDto updateDto)
    {
        entity.Name = updateDto.Name;
        entity.Description = updateDto.Description;

        entity.ActivityId = updateDto.ActivityId;
        entity.YouthCenterId = updateDto.YouthCenterId;

        entity.Format = updateDto.Format;
        entity.Status = updateDto.Status;

        entity.MaxTeams = updateDto.MaxTeams;
        entity.EntryFee = updateDto.EntryFee;

        entity.RegistrationDeadline = updateDto.RegistrationDeadline;
        entity.StartDate = updateDto.StartDate;

        return entity;
    }
}