namespace YouthCenterWeb.Data.DTOs
{
    public class ActivityDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public int YouthCenterId { get; set; }
    }
}