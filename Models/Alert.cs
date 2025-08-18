using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public enum AlertType
    {
        Renewal,
        Payment,
        Approval,
        PipelineReminder,
        MarginUpdate
    }

    public enum AlertPriority
    {
        Low,
        Medium,
        High,
        Critical
    }

    public class Alert
    {
        public int AlertId { get; set; }

        [Column("LicenseId")]
        public int? DealId { get; set; }
        public int? ProjectPipelineId { get; set; }

        [Required(ErrorMessage = "Alert Type is required")]
        public AlertType AlertType { get; set; }

        [Required(ErrorMessage = "Priority is required")]
        public AlertPriority Priority { get; set; } = AlertPriority.Medium;

        [Required(ErrorMessage = "Alert Title is required")]
        [StringLength(200)]
        public required string Title { get; set; }

        [StringLength(1000)]
        public string? AlertMessage { get; set; }

        [Required(ErrorMessage = "Alert Date is required")]
        public DateTime AlertDate { get; set; }

        [Required(ErrorMessage = "Created Date is required")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? SentDate { get; set; }

        public DateTime? DismissedDate { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Pending"; // Pending, Sent, Dismissed, Acknowledged

        [StringLength(100)]
        public string? AssignedTo { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Email notification settings
        public bool EmailSent { get; set; } = false;
        public DateTime? EmailSentDate { get; set; }

        [StringLength(500)]
        public string? EmailRecipients { get; set; }

        // Days before expiry (for renewal alerts)
        public int? DaysBeforeExpiry { get; set; }

        // Navigation properties
        public Deal? Deal { get; set; }
        public ProjectPipeline? ProjectPipeline { get; set; }

        // Legacy compatibility property
        [NotMapped]
        public Deal? License => Deal;

        // Calculated properties
        public bool IsOverdue => DateTime.Now > AlertDate && Status == "Pending";

        public int DaysUntilAlert => (AlertDate - DateTime.Now).Days;
    }
}
