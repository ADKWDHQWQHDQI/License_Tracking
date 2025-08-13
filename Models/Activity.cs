using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class Activity
    {
        [Key]
        public int ActivityId { get; set; }

        // CRM-style Activity Fields (Phase 3 Enhancement)
        [Required]
        [StringLength(50)]
        [Display(Name = "Activity Type")]
        public string Type { get; set; } = string.Empty; // "Call", "Email", "Meeting", "Task", "Demo", "Follow-up", "Quote", "Negotiation", "Contract", "Other"

        [Required]
        [StringLength(200)]
        [Display(Name = "Subject")]
        public string Subject { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Scheduled Date")]
        public DateTime ScheduledDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime? DueDate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Pending"; // "Pending", "In Progress", "Completed", "Cancelled", "Deferred"

        [Required]
        [StringLength(20)]
        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium"; // "Low", "Medium", "High", "Urgent"

        // Entity Association (optional)
        [StringLength(50)]
        [Display(Name = "Entity Type")]
        public string? EntityType { get; set; } // "Deal", "Company", "Contact"

        [Display(Name = "Entity ID")]
        public int? EntityId { get; set; }

        [StringLength(100)]
        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        // Audit Fields
        [Required]
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        [Display(Name = "Updated Date")]
        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Updated By")]
        public string? UpdatedBy { get; set; }

        [Display(Name = "Completed Date")]
        public DateTime? CompletedDate { get; set; }

        [StringLength(100)]
        [Display(Name = "Completed By")]
        public string? CompletedBy { get; set; }

        // Legacy fields for compatibility
        [StringLength(50)]
        [Display(Name = "Related Entity Type")]
        public string RelatedEntityType
        {
            get => EntityType ?? string.Empty;
            set => EntityType = value;
        }

        [Display(Name = "Related Entity ID")]
        public int RelatedEntityId
        {
            get => EntityId ?? 0;
            set => EntityId = value;
        }

        [StringLength(50)]
        [Display(Name = "Activity Type")]
        public string ActivityType
        {
            get => Type;
            set => Type = value;
        }

        [Display(Name = "Activity Date")]
        public DateTime ActivityDate
        {
            get => ScheduledDate;
            set => ScheduledDate = value;
        }

        // Navigation Properties - Polymorphic relationships (no explicit ForeignKey)
        // These are populated manually based on RelatedEntityType and RelatedEntityId
        [NotMapped]
        public virtual Deal? Deal { get; set; }

        [NotMapped]
        public virtual Company? Company { get; set; }

        [NotMapped]
        public virtual ContactPerson? ContactPerson { get; set; }
    }
}
