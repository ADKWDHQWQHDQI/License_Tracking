using System.ComponentModel.DataAnnotations;

namespace License_Tracking.Models
{
    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public required string Password { get; set; }

        [Required]
        [StringLength(50)]
        public required string Role { get; set; } // Admin, Sales, Finance, Operations, Management
    }
}
