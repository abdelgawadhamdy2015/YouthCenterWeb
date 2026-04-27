using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using YouthCenterWeb.Models;
using YouthCenterWeb.YouthCenterWeb.Domain.Entities;
namespace YouthCenterWeb.Models
{
    [PrimaryKey("Id")]
    public class Activity
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!; // Football, PlayStation

        public string? Description { get; set; }
        // 🔗 Youth Center Activities



    }

}