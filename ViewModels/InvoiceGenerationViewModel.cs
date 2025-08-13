using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class InvoiceGenerationViewModel
    {
        public int DealId { get; set; }

        [Required]
        [Display(Name = "Invoice Type")]
        public string InvoiceType { get; set; } = string.Empty; // "Customer_To_Canarys" or "OEM_To_Canarys"

        public int BusinessPhase { get; set; } // 1 for Customer, 4 for OEM

        [Required]
        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Invoice Date")]
        [DataType(DataType.Date)]
        public DateTime InvoiceDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Description")]
        [StringLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Attachment")]
        public IFormFile? AttachmentFile { get; set; }

        // Deal information for display
        public Deal? Deal { get; set; }

        // Display properties
        public string CustomerName => Deal?.Company?.CompanyName ?? "";
        public string OemName => Deal?.Oem?.OemName ?? "";
        public string ProductName => Deal?.Product?.ProductName ?? "";
        public string DealName => Deal?.DealName ?? "";
    }
}
