using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class Deal
    {
        [Key]
        public int DealId { get; set; }

        [Required(ErrorMessage = "Company is required")]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "OEM is required")]
        public int OemId { get; set; }

        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        public int? ContactId { get; set; }

        [Required(ErrorMessage = "Deal Name is required")]
        [StringLength(200)]
        [Display(Name = "Deal Name")]
        public string DealName { get; set; } = string.Empty; // Deal/Project Name

        [StringLength(50)]
        [Display(Name = "Deal Stage")]
        public string DealStage { get; set; } = "Lead"; // "Lead", "Quoted", "Negotiation", "Won", "Lost"

        [StringLength(50)]
        [Display(Name = "Current Phase")]
        public string CurrentPhase { get; set; } = "Pre-Phase"; // "Pre-Phase", "Phase 1", "Phase 2", "Phase 3", "Phase 4", "Lost"

        [StringLength(50)]
        [Display(Name = "Deal Type")]
        public string? DealType { get; set; } // "New", "Renewal", "Upgrade"

        [Required(ErrorMessage = "Quantity is required")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }

        // Phase 1 - Customer Engagement
        [StringLength(100)]
        [Display(Name = "Customer PO Number")]
        public string? CustomerPoNumber { get; set; }

        [Display(Name = "Customer PO Date")]
        public DateTime? CustomerPoDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Customer Invoice Number")]
        public string? CustomerInvoiceNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Customer Invoice Amount")]
        public decimal? CustomerInvoiceAmount { get; set; }

        [StringLength(50)]
        [Display(Name = "Customer Payment Status")]
        public string? CustomerPaymentStatus { get; set; } // "Pending", "Paid", "Overdue"

        [Display(Name = "Customer Payment Date")]
        public DateTime? CustomerPaymentDate { get; set; }

        // Phase 2 - OEM Procurement
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "OEM Quote Amount")]
        public decimal? OemQuoteAmount { get; set; }

        [StringLength(100)]
        [Display(Name = "Canarys PO Number")]
        public string? CanarysPoNumber { get; set; }

        [Display(Name = "Canarys PO Date")]
        public DateTime? CanarysPoDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Margin")]
        public decimal? EstimatedMargin { get; set; }

        // Phase 3 - License Delivery
        [Display(Name = "License Start Date")]
        public DateTime? LicenseStartDate { get; set; }

        [Display(Name = "License End Date")]
        public DateTime? LicenseEndDate { get; set; }

        [StringLength(50)]
        [Display(Name = "License Delivery Status")]
        public string? LicenseDeliveryStatus { get; set; } // "Pending", "Delivered", "Active"

        // Phase 4 - OEM Settlement
        [StringLength(100)]
        [Display(Name = "OEM Invoice Number")]
        public string? OemInvoiceNumber { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "OEM Invoice Amount")]
        public decimal? OemInvoiceAmount { get; set; }

        [StringLength(50)]
        [Display(Name = "OEM Payment Status")]
        public string? OemPaymentStatus { get; set; } // "Pending", "Paid"

        [Display(Name = "OEM Payment Date")]
        public DateTime? OemPaymentDate { get; set; }

        [Display(Name = "Expected Close Date")]
        public DateTime? ExpectedCloseDate { get; set; }

        [Display(Name = "Actual Close Date")]
        public DateTime? ActualCloseDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; } // BA/Sales person

        [Column(TypeName = "decimal(3,2)")]
        [Display(Name = "Deal Probability")]
        public decimal? DealProbability { get; set; } // 0.00 to 1.00

        [StringLength(50)]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Alert Configuration  
        public bool? AlertsEnabled { get; set; } = true;
        public bool? IsProjectPipeline { get; set; } = false;

        // System Fields
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? LastModifiedBy { get; set; }

        public bool IsActive { get; set; } = true;

        // Alert Configuration
        public int? AlertConfigurationId { get; set; }

        // Legacy compatibility properties (computed from relationships)
        [NotMapped]
        public string? ClientName => Company?.CompanyName;

        [NotMapped]
        public string? ProductName => Product?.ProductName;

        [NotMapped]
        public string? OemName => Oem?.OemName;

        [NotMapped]
        public DateTime? LicenseDate => LicenseStartDate;

        [NotMapped]
        public int LicenseId => DealId; // Legacy compatibility

        [NotMapped]
        public int ProjectId => DealId; // Compatibility property

        [NotMapped]
        public string? Resource => Product?.ProductName; // Compatibility property

        [NotMapped]
        public string? LicenseStatus => LicenseDeliveryStatus; // Legacy compatibility

        [NotMapped]
        public DateTime ExpiryDate => LicenseEndDate ?? LicenseStartDate?.AddYears(1) ?? DateTime.Now.AddYears(1);

        [NotMapped]
        public decimal Margin
        {
            get
            {
                decimal customerAmount = CustomerInvoiceAmount ?? 0;
                decimal oemAmount = OemQuoteAmount ?? 0;
                return customerAmount - oemAmount;
            }
        }

        [NotMapped]
        public decimal? AmountReceived => CustomerInvoiceAmount;

        [NotMapped]
        public decimal? AmountPaid => OemInvoiceAmount;

        [NotMapped]
        public decimal? ManualMarginInput => EstimatedMargin;

        [NotMapped]
        public decimal? CustomerPoAmount => CustomerInvoiceAmount;

        [NotMapped]
        public decimal? OemPoAmount => OemQuoteAmount;

        [NotMapped]
        public string? PaymentStatus => CustomerPaymentStatus;

        // Navigation Properties
        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; } = null!;

        [ForeignKey("OemId")]
        public virtual Oem Oem { get; set; } = null!;

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } = null!;

        [ForeignKey("ContactId")]
        public virtual ContactPerson? ContactPerson { get; set; }

        [ForeignKey("AlertConfigurationId")]
        public virtual AlertConfiguration? AlertConfiguration { get; set; }

        public virtual ICollection<CbmsInvoice> Invoices { get; set; } = new List<CbmsInvoice>();

        // Week 8 Enhancement: Deal Activities for Collaboration
        public virtual ICollection<DealCollaborationActivity> Activities { get; set; } = new List<DealCollaborationActivity>();

        // Business Logic Methods for Phase Management
        [NotMapped]
        public bool CanProgressToPhase2 =>
            !string.IsNullOrEmpty(CustomerPoNumber) &&
            CustomerPoDate.HasValue &&
            !string.IsNullOrEmpty(CustomerInvoiceNumber) &&
            CustomerInvoiceAmount.HasValue;

        [NotMapped]
        public bool CanProgressToPhase3 =>
            CanProgressToPhase2 &&
            !string.IsNullOrEmpty(CanarysPoNumber) &&
            CanarysPoDate.HasValue &&
            OemQuoteAmount.HasValue;

        [NotMapped]
        public bool CanProgressToPhase4 =>
            CanProgressToPhase3 &&
            LicenseStartDate.HasValue &&
            LicenseEndDate.HasValue &&
            LicenseDeliveryStatus == "Delivered";

        [NotMapped]
        public bool IsPhase4Complete =>
            CanProgressToPhase4 &&
            !string.IsNullOrEmpty(OemInvoiceNumber) &&
            OemInvoiceAmount.HasValue &&
            OemPaymentStatus == "Paid";

        // Auto-determine current phase based on completed data
        public void UpdateCurrentPhase()
        {
            if (IsPhase4Complete)
                CurrentPhase = "Phase 4";
            else if (CanProgressToPhase4)
                CurrentPhase = "Phase 4";
            else if (CanProgressToPhase3)
                CurrentPhase = "Phase 3";
            else if (CanProgressToPhase2)
                CurrentPhase = "Phase 2";
            else
                CurrentPhase = "Phase 1";
        }

        // Get phase display with status
        [NotMapped]
        public string PhaseStatus
        {
            get
            {
                return CurrentPhase switch
                {
                    "Phase 1" => CanProgressToPhase2 ? "Phase 1 - Complete" : "Phase 1 - In Progress",
                    "Phase 2" => CanProgressToPhase3 ? "Phase 2 - Complete" : "Phase 2 - In Progress",
                    "Phase 3" => CanProgressToPhase4 ? "Phase 3 - Complete" : "Phase 3 - In Progress",
                    "Phase 4" => IsPhase4Complete ? "Phase 4 - Complete" : "Phase 4 - In Progress",
                    _ => "Phase 1 - In Progress"
                };
            }
        }
    }
}
