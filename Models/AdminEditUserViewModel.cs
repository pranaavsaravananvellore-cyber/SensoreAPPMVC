using System.ComponentModel.DataAnnotations;

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

        // Patient-specific
        public bool IsPatient { get; set; }
        public bool CompletedRegistration { get; set; }
        public int? ClinitionId { get; set; }
    }
}