using YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.YouthCenterWeb.Application.Interfaces;

public interface ICupService : IGenericService<Cup, CupDto, CreateCupDto>
{

}