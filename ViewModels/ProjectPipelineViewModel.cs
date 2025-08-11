using System;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    public class ProjectPipelineViewModel
    {
        public int ProjectPipelineId { get; set; }

        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "OEM Name")]
        public string OemName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [Display(Name = "Client Email")]
        [EmailAddress]
        public string? ClientContactEmail { get; set; }

        [Display(Name = "Client Phone")]
        [Phone]
        public string? ClientContactPhone { get; set; }

        [Required]
        [Display(Name = "Expected License Date")]
        [DataType(DataType.Date)]
        public DateTime ExpectedLicenseDate { get; set; }

        [Required]
        [Display(Name = "Expected Expiry Date")]
        [DataType(DataType.Date)]
        public DateTime ExpectedExpiryDate { get; set; }

        [Display(Name = "Customer PO Number")]
        public string? CustomerPoNumber { get; set; }

        [Display(Name = "Customer PO Description")]
        public string? CustomerPoItemDescription { get; set; }

        [Display(Name = "Expected Customer PO Amount")]
        [DataType(DataType.Currency)]
        public decimal ExpectedCustomerPoAmount { get; set; }

        [Display(Name = "OEM PO Number")]
        public string? OemPoNumber { get; set; }

        [Display(Name = "OEM PO Description")]
        public string? OemPoItemDescription { get; set; }

        [Display(Name = "Expected OEM PO Amount")]
        [DataType(DataType.Currency)]
        public decimal ExpectedOemPoAmount { get; set; }

        [Display(Name = "Expected Amount to Receive")]
        [DataType(DataType.Currency)]
        public decimal ExpectedAmountToReceive { get; set; }

        [Display(Name = "Expected Amount to Pay")]
        [DataType(DataType.Currency)]
        public decimal ExpectedAmountToPay { get; set; }

        // Invoice and Payment Tracking
        [Display(Name = "Expected Invoice Number")]
        public string? ExpectedInvoiceNumber { get; set; }

        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; } = "Pending";

        [Display(Name = "Amount Received")]
        [DataType(DataType.Currency)]
        public decimal? AmountReceived { get; set; }

        [Display(Name = "Amount Paid")]
        [DataType(DataType.Currency)]
        public decimal? AmountPaid { get; set; }

        // OEM Type Classification
        [Display(Name = "OEM Type")]
        public string OemType { get; set; } = "Other";

        // Enhanced Customer Profiling
        [Display(Name = "Customer Type")]
        public string? CustomerType { get; set; }

        [Display(Name = "Customer Industry")]
        public string? CustomerIndustry { get; set; }

        [Display(Name = "Customer Employee Count")]
        public int? CustomerEmployeeCount { get; set; }

        [Display(Name = "Customer Website")]
        [Url]
        public string? CustomerWebsite { get; set; }

        // Relationship with OEM
        [Display(Name = "OEM Relationship Type")]
        public string? OemRelationshipType { get; set; }

        [Display(Name = "Last Customer Contact")]
        [DataType(DataType.Date)]
        public DateTime? LastCustomerContact { get; set; }

        [Display(Name = "Customer Notes")]
        public string? CustomerNotes { get; set; }

        [Display(Name = "Projected Margin Input")]
        [DataType(DataType.Currency)]
        public decimal? ProjectedMarginInput { get; set; }

        [Display(Name = "Margin Notes")]
        public string? MarginNotes { get; set; }

        [Display(Name = "Alert Days Before")]
        [Range(1, 365, ErrorMessage = "Alert days must be between 1 and 365")]
        public int AlertDaysBefore { get; set; } = 45;

        [Display(Name = "Alerts Enabled")]
        public bool AlertsEnabled { get; set; } = true;

        [Display(Name = "Project Status")]
        public string ProjectStatus { get; set; } = "Pipeline";

        [Display(Name = "Success Probability (%)")]
        [Range(0, 100, ErrorMessage = "Probability must be between 0 and 100")]
        public int SuccessProbability { get; set; } = 50;

        [Display(Name = "Ship To Address")]
        public string? ShipToAddress { get; set; }

        [Display(Name = "Bill To Address")]
        public string? BillToAddress { get; set; }

        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        // Read-only calculated properties
        public decimal ProjectedMargin => ProjectedMarginInput ?? (ExpectedAmountToReceive - ExpectedAmountToPay);

        public decimal ProjectedProfitMargin => ExpectedAmountToReceive > 0 ? (ProjectedMargin / ExpectedAmountToReceive) * 100 : 0;

        public decimal ProjectedRevenue => ExpectedAmountToReceive * (SuccessProbability / 100.0m);

        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? LastUpdated { get; set; }

        // For conversion tracking
        public int? ConvertedToLicenseId { get; set; }
        public bool IsConverted => ConvertedToLicenseId.HasValue;
    }

    public class ProjectPipelineListViewModel
    {
        public List<ProjectPipelineViewModel> Projects { get; set; } = new();
        public int TotalCount { get; set; }
        public decimal TotalProjectedRevenue { get; set; }
        public decimal TotalProjectedMargin { get; set; }
        public string StatusFilter { get; set; } = string.Empty;
        public string OemFilter { get; set; } = string.Empty;
        public string CustomerFilter { get; set; } = string.Empty;
    }
}
