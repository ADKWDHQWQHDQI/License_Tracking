using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public enum InvoiceType
    {
        Customer,
        OEM
    }

    public class Invoice
    {
        [Key]
        public int InvoiceId { get; set; }

        [Required]
        [Column("LicenseId")]
        public int DealId { get; set; }

        [Required]
        [StringLength(50)]
        public required string InvoiceNumber { get; set; }

        [Required]
        public InvoiceType InvoiceType { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [DataType(DataType.Currency)]
        public decimal AmountReceived { get; set; } = 0;

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? InvoiceDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        // Additional fields for enhanced invoice tracking
        [StringLength(100)]
        public string? VendorName { get; set; } // OEM name for OEM invoices, Customer name for Customer invoices

        [StringLength(200)]
        public string? Description { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        public Deal? Deal { get; set; }

        // Legacy compatibility properties
        [NotMapped]
        public int LicenseId => DealId;

        [NotMapped]
        public Deal? License => Deal;

        // Calculated property for aging analysis
        public int AgingDays
        {
            get => PaymentStatus != "Completed" ? (DateTime.Now - DueDate).Days : 0;
        }
    }
}