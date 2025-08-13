using License_Tracking.Models;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    // Simple Bigin.com-style Invoice Management ViewModel
    public class InvoiceManagementViewModel
    {
        public List<CbmsInvoice> Invoices { get; set; } = new List<CbmsInvoice>();

        // Simple filter properties
        public string CurrentPhase { get; set; } = "all";
        public string CurrentStatus { get; set; } = "all";
        public string SearchTerm { get; set; } = "";

        // Enhanced filter properties
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public string CustomerFilter { get; set; } = "";
        public string OemFilter { get; set; } = "";
        public string SortBy { get; set; } = "date";
        public bool SortDesc { get; set; } = true;

        // Summary statistics
        public int TotalInvoices { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int OverdueCount { get; set; }

        // Phase-specific counts
        public int Phase1Count { get; set; }
        public int Phase4Count { get; set; }
    }

    // Simple Payment Recording ViewModel
    public class PaymentRecordViewModel
    {
        public int InvoiceId { get; set; }
        public CbmsInvoice Invoice { get; set; } = new CbmsInvoice();
        public decimal OutstandingAmount { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal PaymentAmount { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [StringLength(50)]
        public string PaymentMethod { get; set; } = "Bank Transfer";

        [StringLength(100)]
        public string? ReferenceNumber { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        // Attachment support for payment receipts
        public IFormFile? ReceiptFile { get; set; }
        public string? ExistingReceiptPath { get; set; }
        public string? ExistingReceiptFileName { get; set; }
    }

    // Bulk Actions ViewModel
    public class BulkActionViewModel
    {
        public List<int> SelectedInvoiceIds { get; set; } = new List<int>();
        public string ActionType { get; set; } = ""; // "delete", "export", "mark_paid"
        public decimal? BulkPaymentAmount { get; set; }
        public DateTime? BulkPaymentDate { get; set; }
        public string? BulkPaymentMethod { get; set; }
    }
}
