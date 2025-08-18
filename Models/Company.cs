using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class Company
    {
        [Key]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Company Name is required")]
        [StringLength(200)]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Industry { get; set; }

        [StringLength(50)]
        [Display(Name = "Company Size")]
        public string? CompanySize { get; set; } // "1-50", "51-200", "201-1000", "1000+"

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Annual Revenue")]
        public decimal? AnnualRevenue { get; set; }

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? Headquarters { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; } // Renamed for consistency

        [StringLength(200)]
        [EmailAddress]
        public string? Email { get; set; } // Renamed for consistency

        [StringLength(200)]
        public string? Website { get; set; }

        [StringLength(50)]
        [Display(Name = "Company Type")]
        public string? CompanyType { get; set; } // "Prospect", "Customer", "Partner"

        [StringLength(100)]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; } // "Net 30", "Net 45", "Immediate"

        [StringLength(300)]
        [Display(Name = "Primary Business")]
        public string? PrimaryBusiness { get; set; }

        [StringLength(500)]
        [Display(Name = "Technology Stack")]
        public string? TechnologyStack { get; set; }

        [StringLength(500)]
        [Display(Name = "Current Vendors")]
        public string? CurrentVendors { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();
        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public virtual ICollection<CustomerOemProduct> CustomerOemProducts { get; set; } = new List<CustomerOemProduct>();
    }
}
