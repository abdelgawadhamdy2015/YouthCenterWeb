using YouthCenterWeb.YouthCenterWeb.Application.Common.Enums;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs.Cups;

public class CupListQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public CupStatus? Status { get; set; }
    public int? YouthCenterId { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}