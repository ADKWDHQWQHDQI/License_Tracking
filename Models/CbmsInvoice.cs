using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class CbmsInvoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public int DealId { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Invoice Type")]
        public string InvoiceType { get; set; } = string.Empty; // "Customer_To_Canarys", "Canarys_To_OEM", "OEM_To_Canarys"

        [Required]
        [StringLength(100)]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tax Amount")]
        public decimal TaxAmount { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        [StringLength(50)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Unpaid"; // "Unpaid", "Paid", "Overdue", "Partial"

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Payment Received")]
        public decimal? PaymentReceived { get; set; } = 0;

        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [StringLength(100)]
        [Display(Name = "Payment Reference")]
        public string? PaymentReference { get; set; }

        [StringLength(500)]
        [Display(Name = "Reference")]
        public string? Reference { get; set; }

        [Required]
        [Display(Name = "Business Phase")]
        public int BusinessPhase { get; set; } // 1, 2, 3, 4

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation Properties
        [ForeignKey("DealId")]
        public virtual Deal Deal { get; set; } = null!;
    }
}
