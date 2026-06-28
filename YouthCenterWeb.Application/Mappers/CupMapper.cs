using AutoMapper;
using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Mappers;

public class CupProfile : Profile
{
    public CupProfile()
    {
        CreateMap<CreateCupDto, Cup>();

        CreateMap<UpdateCupDto, Cup>();

        CreateMap<Cup, CupDto>()
            .ForMember(
                d => d.ActivityName,
                o => o.MapFrom(s => s.YouthCenterActivity.Activity != null ? s.YouthCenterActivity.Activity.NameAr : string.Empty))
            .ForMember(
                d => d.YouthCenterName,
                o => o.MapFrom(s => s.YouthCenterActivity.YouthCenter != null ? s.YouthCenterActivity.YouthCenter.NameAr : string.Empty))
            .ForMember(
                d => d.CreatedByAdminName,
                o => o.MapFrom(s => s.CreatedByAdmin != null ? s.CreatedByAdmin.Name : string.Empty));

        CreateMap<Cup, CupDetailDto>();

        CreateMap<CupTeam, CupTeamDto>()
            .ForMember(
                d => d.CaptainName,
                o => o.MapFrom(s => s.CaptainUser != null ? s.CaptainUser.Name : string.Empty))
            .ForMember(
                d => d.GroupName,
                o => o.MapFrom(s => s.Group != null ? s.Group.Name : string.Empty))
            .ForMember(
                d => d.GoalDifference,
                o => o.MapFrom(s => s.GoalsFor - s.GoalsAgainst))
            .ForMember(
                d => d.PlayedMatches,
                o => o.MapFrom(s => s.Wins + s.Draws + s.Losses));

        CreateMap<CupTeam, CupStandingDto>()
            .ForMember(
                d => d.TeamId,
                o => o.MapFrom(s => s.Id))
            .ForMember(
                d => d.TeamName,
                o => o.MapFrom(s => s.TeamName))
            .ForMember(
                d => d.GoalDifference,
                o => o.MapFrom(s => s.GoalsFor - s.GoalsAgainst))
            .ForMember(
                d => d.PlayedMatches,
                o => o.MapFrom(s => s.Wins + s.Draws + s.Losses));

        CreateMap<CupGroup, CupGroupDto>();

        CreateMap<CupMatch, CupMatchDto>()
            .ForMember(
                d => d.GroupName,
                o => o.MapFrom(s => s.Group != null ? s.Group.Name : string.Empty))
            .ForMember(
                d => d.TeamAName,
                o => o.MapFrom(s => s.TeamA != null ? s.TeamA.TeamName : string.Empty))
            .ForMember(
                d => d.TeamBName,
                o => o.MapFrom(s => s.TeamB != null ? s.TeamB.TeamName : string.Empty))
            .ForMember(
                d => d.WinnerName,
                o => o.MapFrom(s => s.Winner != null ? s.Winner.TeamName : string.Empty));
    }
}