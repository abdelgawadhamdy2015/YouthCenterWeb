using System.ComponentModel.DataAnnotations;

namespace YouthCenterWeb.YouthCenterWeb.Domain.Entities;

public class YouthCenter
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? City { get; set; }

    public string Email { get; set; } = string.Empty;

    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public double? Latitude { get; set; }
    [Required]
    public double? Longitude { get; set; }
    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }



    public List<YouthCenterActivity> YouthCenterActivities { get; set; } = new();




    public List<Reservation> Reservations { get; set; } = new List<Reservation>();
}