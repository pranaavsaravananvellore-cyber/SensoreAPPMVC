using System.ComponentModel.DataAnnotations;
using SensoreAPPMVC.Models;
namespace SensoreAPPMVC.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }
}