using YouthCenterWeb.Data.DTOs;

namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class UpdateReservationDto
    {


        public DateTime? Date { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public int? YouthCenterActivityId { get; set; }

        public decimal? TotalPrice { get; set; }

        public string? SpecialRequest { get; set; }


    }
}