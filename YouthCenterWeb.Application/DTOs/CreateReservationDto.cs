using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using AutoMapper.Configuration.Annotations;
using Microsoft.AspNetCore.Mvc;

namespace YouthCenterWeb.Data.DTOs
{
    public class CreateReservationDto
    {
        [Required]
        public DateTime Date { get; set; }

        [Required]
        public TimeOnly StartTime { get; set; }

        [Required]
        public TimeOnly EndTime { get; set; }


        [Required]
        public int YouthCenterActivityId { get; set; }

        public int? UserId { get; set; }

        public decimal? TotalPrice { get; set; }



    }
}