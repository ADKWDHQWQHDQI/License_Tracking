using System;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    // Renamed from LicenseViewModel to DealViewModel to align with CBMS B2B2B CRM approach
    public class DealViewModel
    {
        public int DealId { get; set; }

        [Display(Name = "Deal Name")]
        [StringLength(200)]
        public string? DealName { get; set; }

        [Display(Name = "Product Name")]
        [StringLength(100)]
        public string? ProductName { get; set; }

        [Display(Name = "OEM Name")]
        [StringLength(100)]
        public string? OemName { get; set; }

        [Display(Name = "Customer Name")]
        [StringLength(100)]
        public string? CustomerName { get; set; }

        [Required]
        [Display(Name = "Deal Date")]
        [DataType(DataType.Date)]
        public DateTime DealDate { get; set; }

        [Display(Name = "License Start Date")]
        [DataType(DataType.Date)]
        public DateTime? LicenseStartDate { get; set; }

        [Display(Name = "License End Date")]
        [DataType(DataType.Date)]
        public DateTime? LicenseEndDate { get; set; }

        [Display(Name = "Customer PO Number")]
        [StringLength(50)]
        public string? CustomerPoNumber { get; set; }

        [Display(Name = "Customer PO Item Description")]
        [StringLength(200)]
        public string? CustomerPoItemDescription { get; set; }

        [Display(Name = "Customer Invoice Amount")]
        [DataType(DataType.Currency)]
        public decimal? CustomerInvoiceAmount { get; set; }

        [Display(Name = "OEM PO Number")]
        [StringLength(50)]
        public string? OemPoNumber { get; set; }

        [Display(Name = "OEM PO Item Description")]
        [StringLength(200)]
        public string? OemPoItemDescription { get; set; }

        [Display(Name = "OEM Quote Amount")]
        [DataType(DataType.Currency)]
        public decimal? OemQuoteAmount { get; set; }

        [Display(Name = "Deal Stage")]
        [StringLength(50)]
        public string? DealStage { get; set; } = "Lead";

        [Display(Name = "Deal Type")]
        [StringLength(50)]
        public string? DealType { get; set; } = "New";

        [Display(Name = "Quantity")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Estimated Margin")]
        [DataType(DataType.Currency)]
        public decimal? EstimatedMargin { get; set; }

        [Display(Name = "Customer Payment Status")]
        [StringLength(50)]
        public string? CustomerPaymentStatus { get; set; } = "Pending";

        [Display(Name = "OEM Payment Status")]
        [StringLength(50)]
        public string? OemPaymentStatus { get; set; } = "Pending";

        [Display(Name = "License Delivery Status")]
        [StringLength(50)]
        public string? LicenseDeliveryStatus { get; set; } = "Pending";

        [Display(Name = "Assigned To")]
        [StringLength(100)]
        public string? AssignedTo { get; set; }

        [Display(Name = "Notes")]
        [StringLength(1000)]
        public string? Notes { get; set; }

        // Alert and tracking properties
        [Display(Name = "Alert Days Before")]
        public int AlertDaysBefore { get; set; } = 30;

        [Display(Name = "Alert Status")]
        [StringLength(50)]
        public string? AlertStatus { get; set; }

        [Display(Name = "Margin Input By")]
        [StringLength(100)]
        public string? MarginInputBy { get; set; }

        [Display(Name = "Margin Last Updated")]
        public DateTime? MarginLastUpdated { get; set; }

        [Display(Name = "Is Project Pipeline")]
        public bool IsProjectPipeline { get; set; }

        [Display(Name = "Margin Notes")]
        [StringLength(500)]
        public string? MarginNotes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;

        // Legacy compatibility properties for backward compatibility
        public int LicenseId => DealId;
        public string? ClientName => CustomerName;
        public DateTime LicenseDate => DealDate;
        public DateTime ExpiryDate => LicenseEndDate ?? DealDate.AddYears(1);
        public decimal CustomerPoAmount => CustomerInvoiceAmount ?? 0;
        public decimal OemPoAmount => OemQuoteAmount ?? 0;
        public string? LicenseStatus => LicenseDeliveryStatus;
        public decimal AmountReceived => CustomerInvoiceAmount ?? 0;
        public decimal AmountPaid => OemQuoteAmount ?? 0;
        public decimal Margin => (CustomerInvoiceAmount ?? 0) - (OemQuoteAmount ?? 0);
        public decimal? ManualMarginInput => EstimatedMargin;
        public string? PaymentStatus => CustomerPaymentStatus;
        public string? InvoiceNumber => CustomerPoNumber;
        public string? OemInvoiceNumber => OemPoNumber;

        // Computed properties
        public decimal ProfitMargin
        {
            get
            {
                if (EstimatedMargin.HasValue)
                    return EstimatedMargin.Value;

                var revenue = CustomerInvoiceAmount ?? 0;
                var cost = OemQuoteAmount ?? 0;

                return revenue > 0 ? ((revenue - cost) / revenue) * 100 : 0;
            }
        }

        public int DaysUntilExpiry
        {
            get
            {
                if (LicenseEndDate.HasValue)
                    return (LicenseEndDate.Value - DateTime.Today).Days;
                return (DealDate.AddYears(1) - DateTime.Today).Days;
            }
        }

        public bool IsExpiringSoon => DaysUntilExpiry <= 45 && DaysUntilExpiry > 0;
        public bool IsExpired => DaysUntilExpiry < 0;
    }
}
