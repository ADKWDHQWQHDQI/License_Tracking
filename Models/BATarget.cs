using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class BATarget
    {
        [Key]
        public int TargetId { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Target Type")]
        public string TargetType { get; set; } = string.Empty; // "Revenue", "Customer_Acquisition", "Deal_Count"

        [Required]
        [StringLength(100)]
        [Display(Name = "Assigned To")]
        public string AssignedTo { get; set; } = string.Empty; // BA/Sales person

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Target Value")]
        public decimal TargetValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Actual Value")]
        public decimal ActualValue { get; set; } = 0;

        [Required]
        [StringLength(20)]
        [Display(Name = "Target Period")]
        public string TargetPeriod { get; set; } = string.Empty; // "2025-Q3", "2025-08", "2025"

        [Required]
        [StringLength(20)]
        [Display(Name = "Period Type")]
        public string PeriodType { get; set; } = string.Empty; // "Monthly", "Quarterly", "Annual"

        [Required]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateTime EndDate { get; set; }

        [Display(Name = "Achievement Percentage")]
        [NotMapped]
        public decimal AchievementPercentage => TargetValue > 0 ? (ActualValue / TargetValue) * 100 : 0;

        [StringLength(50)]
        public string Status { get; set; } = "Active"; // "Active", "Achieved", "Missed", "Inactive"

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? CreatedBy { get; set; }
    }
}
