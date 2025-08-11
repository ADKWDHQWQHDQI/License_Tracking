using System;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    public class DealMarginUpdateViewModel
    {
        public int DealId { get; set; }

        [Display(Name = "Deal Name")]
        public string? DealName { get; set; }

        [Display(Name = "Product Name")]
        public string? ProductName { get; set; }

        [Display(Name = "OEM Name")]
        public string? OemName { get; set; }

        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        [Display(Name = "Deal Date")]
        [DataType(DataType.Date)]
        public DateTime DealDate { get; set; }

        [Display(Name = "Customer Invoice Amount")]
        [DataType(DataType.Currency)]
        public decimal? CustomerInvoiceAmount { get; set; }

        [Display(Name = "OEM Quote Amount")]
        [DataType(DataType.Currency)]
        public decimal? OemQuoteAmount { get; set; }

        [Display(Name = "Current Estimated Margin")]
        [DataType(DataType.Currency)]
        public decimal? CurrentEstimatedMargin { get; set; }

        [Required]
        [Display(Name = "New Estimated Margin")]
        [DataType(DataType.Currency)]
        public decimal NewEstimatedMargin { get; set; }

        [Display(Name = "Margin Notes")]
        [StringLength(500)]
        public string? MarginNotes { get; set; }

        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.Now;

        // Legacy compatibility properties
        public int LicenseId => DealId;
        public string? ClientName => CustomerName;
        public DateTime LicenseDate => DealDate;
        public decimal? AmountReceived => CustomerInvoiceAmount;
        public decimal? AmountPaid => OemQuoteAmount;
        public decimal? ManualMarginInput => NewEstimatedMargin;
        public decimal CustomerPoAmount => CustomerInvoiceAmount ?? 0;
        public decimal OemPoAmount => OemQuoteAmount ?? 0;
        public decimal CurrentMargin => CurrentEstimatedMargin ?? 0;
        public decimal AutoCalculatedMargin => (CustomerInvoiceAmount ?? 0) - (OemQuoteAmount ?? 0);
        public string? MarginInputBy => UpdatedBy;
        public DateTime? MarginLastUpdated => LastUpdated;
    }

    public class DealMarginHistoryViewModel
    {
        public int DealId { get; set; }
        public string? DealName { get; set; }
        public decimal OldMargin { get; set; }
        public decimal NewMargin { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string? Notes { get; set; }

        // Legacy compatibility properties
        public int LicenseId => DealId;
    }

    public class BulkDealMarginUpdateViewModel
    {
        public List<int> SelectedDealIds { get; set; } = new();

        [Required]
        [Display(Name = "Margin Update Type")]
        public string MarginUpdateType { get; set; } = "Percentage"; // "Percentage", "Fixed Amount"

        [Required]
        [Display(Name = "Update Value")]
        public decimal UpdateValue { get; set; }

        [Display(Name = "Update Notes")]
        [StringLength(500)]
        public string? UpdateNotes { get; set; }

        [Display(Name = "Apply to All Selected")]
        public bool ApplyToAll { get; set; } = true;

        // Legacy compatibility properties
        public List<int> SelectedLicenseIds => SelectedDealIds;
    }

    public class DealMarginAnalysisViewModel
    {
        public int DealId { get; set; }
        public string? DealName { get; set; }
        public string? ProductName { get; set; }
        public string? OemName { get; set; }
        public string? CustomerName { get; set; }
        public decimal CustomerInvoiceAmount { get; set; }
        public decimal OemQuoteAmount { get; set; }
        public decimal EstimatedMargin { get; set; }
        public decimal MarginPercentage { get; set; }
        public DateTime DealDate { get; set; }

        // Legacy compatibility properties
        public int LicenseId => DealId;
        public string? ClientName => CustomerName;
        public DateTime LicenseDate => DealDate;
        public decimal AmountReceived => CustomerInvoiceAmount;
        public decimal AmountPaid => OemQuoteAmount;
        public decimal ManualMarginInput => EstimatedMargin;
    }

    public class DealMarginComparisonViewModel
    {
        public int DealId { get; set; }
        public string? DealName { get; set; }
        public decimal CalculatedMargin { get; set; }
        public decimal EstimatedMargin { get; set; }
        public decimal VarianceAmount { get; set; }
        public decimal VariancePercentage { get; set; }
        public string? VarianceReason { get; set; }
        public DateTime LastUpdated { get; set; }

        // Legacy compatibility properties
        public int LicenseId => DealId;
    }

    // Legacy LicenseMarginUpdateViewModel for backward compatibility
    public class LicenseMarginUpdateViewModel : DealMarginUpdateViewModel
    {
        // Additional properties expected by legacy views
        public decimal AutoCalculatedMarginPercentage
        {
            get
            {
                var received = CustomerInvoiceAmount ?? 0;
                var paid = OemQuoteAmount ?? 0;
                return received > 0 ? ((received - paid) / received) * 100 : 0;
            }
        }

        public bool HasManualOverride => NewEstimatedMargin != AutoCalculatedMargin;

        public string MarginStatus => HasManualOverride ? "Manual" : "Auto";

        public decimal MarginPercentage
        {
            get
            {
                var received = CustomerInvoiceAmount ?? 0;
                return received > 0 ? (NewEstimatedMargin / received) * 100 : 0;
            }
        }

        public new decimal AutoCalculatedMargin
        {
            get
            {
                return (CustomerInvoiceAmount ?? 0) - (OemQuoteAmount ?? 0);
            }
        }
    }

    // Legacy LicenseMarginHistoryViewModel for backward compatibility
    public class LicenseMarginHistoryViewModel : DealMarginHistoryViewModel
    {
        // This class exists for legacy code compatibility
    }

    // Legacy BulkLicenseMarginUpdateViewModel for backward compatibility
    public class BulkLicenseMarginUpdateViewModel : BulkDealMarginUpdateViewModel
    {
        // This class exists for legacy code compatibility
    }

    // Legacy LicenseMarginAnalysisViewModel for backward compatibility
    public class LicenseMarginAnalysisViewModel : DealMarginAnalysisViewModel
    {
        // This class exists for legacy code compatibility
    }

    // Legacy LicenseMarginComparisonViewModel for backward compatibility
    public class LicenseMarginComparisonViewModel : DealMarginComparisonViewModel
    {
        // This class exists for legacy code compatibility
    }
}
