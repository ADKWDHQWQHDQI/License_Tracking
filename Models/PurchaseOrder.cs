using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class PurchaseOrder
    {
        [Key]
        public int PurchaseOrderId { get; set; }

        [Required(ErrorMessage = "Deal ID is required")]
        [Column("LicenseId")]
        public int DealId { get; set; }

        [StringLength(50)]
        public string OemPoNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "OEM PO Amount is required")]
        [DataType(DataType.Currency)]
        public decimal OemPoAmount { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(50)]
        public string OemInvoiceNumber { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal AmountPaid { get; set; }

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [StringLength(100)]
        public string? PaymentTerms { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? VendorContactEmail { get; set; }

        [StringLength(50)]
        public string? VendorContactPhone { get; set; }

        // Navigation property
        public virtual Deal Deal { get; set; } = null!;

        [Required(ErrorMessage = "Created Date is required")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [DataType(DataType.DateTime)]
        public DateTime? UpdatedDate { get; set; }
    }
}