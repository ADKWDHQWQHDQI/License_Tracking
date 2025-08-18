using System.ComponentModel.DataAnnotations;

namespace License_Tracking.Models
{
    public class RegisterModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Role is required")]
        [StringLength(50)]
        public required string Role { get; set; } // Admin, Sales, Finance, Operations, Management
    }
}
