namespace YouthCenterWeb.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string? Mobile { get; set; }

        public string? ImageUrl { get; set; }

        public int YouthCenterId { get; set; }

        public int RoleId { get; set; }

        public int ReservationCount { get; set; }
    }
}