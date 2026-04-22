using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace YouthCenterWeb.Models
{
    [PrimaryKey("Id")]

    public class Role
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;


    }
}