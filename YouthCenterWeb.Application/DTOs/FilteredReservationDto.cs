namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class FilteredReservationDto
    {

        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }

        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public int? YouthCenterId { get; set; }

        public int? ActivityId { get; set; }

        public int? UserId { get; set; }

        public ReservationStatus? Status { get; set; }



    }
}