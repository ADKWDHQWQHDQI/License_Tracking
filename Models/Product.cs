using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public int OemId { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Product Category")]
        public string? ProductCategory { get; set; } // "Software", "Cloud", "Security", etc.

        [StringLength(1000)]
        [Display(Name = "Product Description")]
        public string? ProductDescription { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal? UnitPrice { get; set; }

        [StringLength(100)]
        [Display(Name = "License Type")]
        public string? LicenseType { get; set; } // "Subscription", "Perpetual", "Trial"

        [Display(Name = "Minimum Quantity")]
        public int MinimumQuantity { get; set; } = 1;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("OemId")]
        public virtual Oem Oem { get; set; } = null!;

        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
        public virtual ICollection<CustomerOemProduct> CustomerOemProducts { get; set; } = new List<CustomerOemProduct>();
    }
}
