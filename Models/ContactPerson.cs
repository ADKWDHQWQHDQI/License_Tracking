using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class ContactPerson
    {
        [Key]
        public int ContactId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty; // Combined First + Last Name

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Number { get; set; } // Renamed for consistency

        [StringLength(100)]
        public string? Designation { get; set; }

        [StringLength(100)]
        public string? Department { get; set; }

        [StringLength(50)]
        [Display(Name = "Decision Maker Level")]
        public string? DecisionMakerLevel { get; set; } // "Primary", "Secondary", "Influencer"

        [Display(Name = "Primary Contact")]
        public bool IsPrimaryContact { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
    }
}
