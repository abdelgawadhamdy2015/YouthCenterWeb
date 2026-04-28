namespace YouthCenterWeb.YouthCenterWeb.Application.DTOs
{
    public class CreateYouthCenterActivityDto
    {

        public int YouthCenterId { get; set; }

        public int ActivityId { get; set; }

        public bool IsAvailable { get; set; } = true;

        public int MaxCapacity { get; set; }

        public decimal Price { get; set; }
    }
}