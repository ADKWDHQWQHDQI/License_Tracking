using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class Oem
    {
        [Key]
        public int OemId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "OEM Name")]
        public string OemName { get; set; } = string.Empty;

        [StringLength(200)]
        [EmailAddress]
        [Display(Name = "Contact Email")]
        public string? ContactEmail { get; set; }

        [StringLength(20)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; } // Renamed for consistency

        [StringLength(100)]
        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; } // "Net 30", "Net 45", "Immediate"

        [StringLength(50)]
        [Display(Name = "Service Level")]
        public string? ServiceLevel { get; set; } // "Gold", "Silver", "Bronze"

        [Range(1.0, 5.0)]
        [Column(TypeName = "decimal(3,2)")]
        [Display(Name = "Performance Rating")]
        public decimal? PerformanceRating { get; set; } // 1.00 to 5.00

        [StringLength(500)]
        public string? Address { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public virtual ICollection<CustomerOemProduct> CustomerOemProducts { get; set; } = new List<CustomerOemProduct>();
    }
}
