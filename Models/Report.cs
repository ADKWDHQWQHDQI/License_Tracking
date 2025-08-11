using System;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.Models
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        [Required]
        [StringLength(50)]
        public required string ReportType { get; set; } // e.g., CustomerSummary, ProductBusiness

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime GeneratedDate { get; set; }

        [Required]
        [StringLength(2000)]
        public required string Data { get; set; } // JSON-serialized report data

        [Required]
        [StringLength(50)]
        public required string GeneratedByRole { get; set; } // Role that generated the report
    }
}
