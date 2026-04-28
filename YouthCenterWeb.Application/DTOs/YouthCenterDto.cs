using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;

namespace YouthCenterWeb.Data.DTOs
{
    public class YouthCenterDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Mobile { get; set; } = string.Empty;

        public string? Location { get; set; }


        public string? Description { get; set; }

        public List<int> ActivitiesIds { get; set; } = new List<int>();
        public List<string> ActivitiesNames { get; set; } = new List<string>();

        public List<Activity>? Activities { get; set; }

        public bool IsActive { get; set; } = true;
    }
}