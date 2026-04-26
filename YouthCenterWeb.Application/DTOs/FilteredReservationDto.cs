namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class FilteredReservationDto
    {

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }




    }
}