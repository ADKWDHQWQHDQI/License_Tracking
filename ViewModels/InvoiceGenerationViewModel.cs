using System.ComponentModel.DataAnnotations;
using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class InvoiceGenerationViewModel
    {
        public int DealId { get; set; }

        public InvoiceType InvoiceType { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? InvoiceDate { get; set; } = DateTime.Now;

        [DataType(DataType.Date)]
        public DateTime? PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending";

        [StringLength(500)]
        public string? Notes { get; set; }

        // Deal information for display
        public Deal? Deal { get; set; }

        // Display properties
        public string CustomerName => Deal?.Company?.CompanyName ?? "";
        public string OemName => Deal?.Oem?.OemName ?? "";
        public string ProductName => Deal?.Product?.ProductName ?? "";
    }
}
