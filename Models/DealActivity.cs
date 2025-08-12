using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class DealCollaborationActivity
    {
        [Key]
        public int ActivityId { get; set; }

        [Required]
        public int DealId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Activity Type")]
        public string ActivityType { get; set; } = string.Empty; // "Comment", "Status_Change", "Assignment", "Invoice_Generated", "Payment_Received", "Email_Sent", "Call_Made", "Meeting_Scheduled"

        [Required]
        [StringLength(200)]
        [Display(Name = "Activity Title")]
        public string ActivityTitle { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Activity Description")]
        public string? ActivityDescription { get; set; }

        [StringLength(100)]
        [Display(Name = "Performed By")]
        public string? PerformedBy { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        [Display(Name = "Activity Date")]
        public DateTime ActivityDate { get; set; } = DateTime.Now;

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [StringLength(50)]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Critical"

        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // "Pending", "In_Progress", "Completed", "Cancelled"

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Display(Name = "Is Internal")]
        public bool IsInternal { get; set; } = false; // true = internal team only, false = visible to customer

        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Last Modified Date")]
        public DateTime LastModifiedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Last Modified By")]
        public string? LastModifiedBy { get; set; }

        // Navigation Properties
        [ForeignKey("DealId")]
        public virtual Deal Deal { get; set; } = null!;

        // Computed Properties
        [NotMapped]
        public bool IsOverdue => DueDate.HasValue && DueDate < DateTime.Now && Status != "Completed";

        [NotMapped]
        public string ActivityIcon
        {
            get => ActivityType switch
            {
                "Comment" => "fas fa-comment",
                "Status_Change" => "fas fa-exchange-alt",
                "Assignment" => "fas fa-user-tag",
                "Invoice_Generated" => "fas fa-file-invoice",
                "Payment_Received" => "fas fa-money-check-alt",
                "Email_Sent" => "fas fa-envelope",
                "Call_Made" => "fas fa-phone",
                "Meeting_Scheduled" => "fas fa-calendar-alt",
                _ => "fas fa-info-circle"
            };
        }

        [NotMapped]
        public string ActivityColor
        {
            get => ActivityType switch
            {
                "Comment" => "primary",
                "Status_Change" => "info",
                "Assignment" => "warning",
                "Invoice_Generated" => "success",
                "Payment_Received" => "success",
                "Email_Sent" => "secondary",
                "Call_Made" => "info",
                "Meeting_Scheduled" => "primary",
                _ => "secondary"
            };
        }

        [NotMapped]
        public string PriorityColor
        {
            get => Priority switch
            {
                "Low" => "success",
                "Medium" => "warning",
                "High" => "danger",
                "Critical" => "dark",
                _ => "secondary"
            };
        }

        [NotMapped]
        public string StatusColor
        {
            get => Status switch
            {
                "Pending" => "warning",
                "In_Progress" => "info",
                "Completed" => "success",
                "Cancelled" => "danger",
                _ => "secondary"
            };
        }
    }
}
