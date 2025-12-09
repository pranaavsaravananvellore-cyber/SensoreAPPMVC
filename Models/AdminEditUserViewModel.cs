using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SensoreAPPMVC.Models
{
    public class AdminEditUserViewModel
    {
        public int UserId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public DateOnly DOB { get; set; }

        public bool IsPatient { get; set; }
        public bool CompletedRegistration { get; set; }

        public int? ClinicianId { get; set; }

        public List<SelectListItem> ClinicianList { get; set; } = new();
    }
}