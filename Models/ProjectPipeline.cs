using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class ProjectPipeline
    {
        public int ProjectPipelineId { get; set; }

        [Required]
        [StringLength(100)]
        public required string ProductName { get; set; }

        [Required]
        [StringLength(100)]
        public required string OemName { get; set; }

        [Required]
        [StringLength(100)]
        public required string ClientName { get; set; }

        [StringLength(200)]
        public string? ClientContactEmail { get; set; }

        [StringLength(50)]
        public string? ClientContactPhone { get; set; }

        [Required]
        public DateTime ExpectedLicenseDate { get; set; }

        [Required]
        public DateTime ExpectedExpiryDate { get; set; }

        // Pipeline Stage Management (Week 10 Enhancement)
        [StringLength(50)]
        public string PipelineStage { get; set; } = "Lead"; // Lead, Qualified, Proposal, Negotiation, Closing, Won, Lost

        [Display(Name = "Stage Confidence Level")]
        [Range(1, 5)]
        public int StageConfidenceLevel { get; set; } = 3;

        [Display(Name = "Expected Close Date")]
        public DateTime? ExpectedCloseDate { get; set; }

        [Display(Name = "Last Activity Date")]
        public DateTime? LastActivityDate { get; set; }

        [Display(Name = "Next Follow-up Date")]
        public DateTime? NextFollowupDate { get; set; }

        // Revenue and Margin Estimations (Week 10 Core Feature)
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Revenue (INR)")]
        public decimal EstimatedRevenue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Cost (INR)")]
        public decimal EstimatedCost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Margin (INR)")]
        public decimal EstimatedMargin
        {
            get => EstimatedRevenue - EstimatedCost;
        }

        [Display(Name = "Estimated Margin (%)")]
        public decimal EstimatedMarginPercentage
        {
            get => EstimatedRevenue > 0 ? (EstimatedMargin / EstimatedRevenue) * 100 : 0;
        }

        [Display(Name = "Success Probability (%)")]
        [Range(0, 100)]
        public int SuccessProbability { get; set; } = 50;

        [Display(Name = "Weighted Revenue")]
        public decimal WeightedRevenue
        {
            get => EstimatedRevenue * (SuccessProbability / 100.0m);
        }

        // Customer PO Details
        [StringLength(50)]
        public string? CustomerPoNumber { get; set; }

        [StringLength(200)]
        public string? CustomerPoItemDescription { get; set; }

        public decimal ExpectedCustomerPoAmount { get; set; }

        // OEM PO Details
        [StringLength(50)]
        public string? OemPoNumber { get; set; }

        [StringLength(200)]
        public string? OemPoItemDescription { get; set; }

        public decimal ExpectedOemPoAmount { get; set; }

        // Expected amounts
        public decimal ExpectedAmountToReceive { get; set; }
        public decimal ExpectedAmountToPay { get; set; }

        // Invoice and Payment Tracking for Pipeline Projects
        [StringLength(50)]
        public string? ExpectedInvoiceNumber { get; set; }

        [StringLength(50)]
        public string PaymentStatus { get; set; } = "Pending"; // Pending, Partial, Paid, Overdue

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountReceived { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? AmountPaid { get; set; }

        // OEM Type Classification
        [StringLength(50)]
        public string OemType { get; set; } = "Other"; // Microsoft, Adobe, VMware, Oracle, Other

        public bool IsMicrosoftOem => OemType == "Microsoft" || OemName?.ToLower().Contains("microsoft") == true;

        // Enhanced Customer Profiling
        [StringLength(100)]
        public string? CustomerType { get; set; } // Enterprise, SMB, Government, Education

        [StringLength(100)]
        public string? CustomerIndustry { get; set; } // Technology, Finance, Healthcare, Manufacturing, etc.

        public int? CustomerEmployeeCount { get; set; }

        [StringLength(200)]
        public string? CustomerWebsite { get; set; }

        // Relationship with OEM
        [StringLength(100)]
        public string? OemRelationshipType { get; set; } // Direct, Partner, Reseller, Distributor

        public DateTime? LastCustomerContact { get; set; }

        [StringLength(500)]
        public string? CustomerNotes { get; set; }

        // Margin management
        public decimal? ProjectedMarginInput { get; set; }

        [StringLength(500)]
        public string? MarginNotes { get; set; }

        [StringLength(100)]
        public string? MarginInputBy { get; set; }

        public DateTime? MarginLastUpdated { get; set; }

        // Alert configuration
        public int AlertDaysBefore { get; set; } = 45;
        public bool AlertsEnabled { get; set; } = true;

        // Project status
        [StringLength(50)]
        public string ProjectStatus { get; set; } = "Pipeline"; // Pipeline, In Progress, Converted, Cancelled

        [StringLength(200)]
        public string? ShipToAddress { get; set; }

        [StringLength(200)]
        public string? BillToAddress { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? LastUpdated { get; set; }

        // Navigation properties for when converted to actual deal
        public int? ConvertedToLicenseId { get; set; } // Reusing this field for deal ID
        public virtual Deal? ConvertedToDeal { get; set; }

        // Legacy calculated properties (maintained for compatibility)
        [DataType(DataType.Currency)]
        public decimal ProjectedMargin
        {
            get => ProjectedMarginInput ?? EstimatedMargin;
        }

        public decimal ProjectedProfitMargin
        {
            get => EstimatedRevenue > 0 ? (EstimatedMargin / EstimatedRevenue) * 100 : 0;
        }

        // Revenue projection for analytics
        public decimal ProjectedRevenue
        {
            get => WeightedRevenue;
        }
    }
}
