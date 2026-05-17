namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class FilterReservationDashboardDto
    {


        public DateTime? DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public ReservationStatus? Status { get; set; }

        public int? ActivityId { get; set; }


    }
}