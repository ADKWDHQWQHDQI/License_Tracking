using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class CustomerOemProduct
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        public int OemId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [StringLength(100)]
        [Display(Name = "Relationship Type")]
        public string? RelationshipType { get; set; } // "Active", "Prospect", "Past"

        [Display(Name = "Relationship Start Date")]
        public DateTime? RelationshipStartDate { get; set; }

        [Display(Name = "Assigned Date")]
        public DateTime AssignedDate { get; set; } = DateTime.Now;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Unit Price")]
        public decimal? UnitPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Value")]
        public decimal? TotalValue => (UnitPrice ?? 0) * Quantity;

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation Properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("OemId")]
        public virtual Oem Oem { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;
    }
}
