using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class AlertConfiguration
    {
        public int AlertConfigurationId { get; set; }

        [Required(ErrorMessage = "Configuration Name is required")]
        [StringLength(100)]
        public required string ConfigurationName { get; set; }

        [Required(ErrorMessage = "Alert Thresholds are required")]
        [StringLength(500)]
        public required string AlertThresholds { get; set; } // JSON string: "[45,30,15,7,3,1]"

        [Required(ErrorMessage = "Is Default setting is required")]
        public bool IsDefault { get; set; } = false;

        [Required(ErrorMessage = "Is Active setting is required")]
        public bool IsActive { get; set; } = true;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Created Date is required")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? ModifiedDate { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        // Navigation properties
        public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();

        // Legacy compatibility property
        [NotMapped]
        public virtual ICollection<Deal> Licenses => Deals;

        // Helper methods
        public List<int> GetThresholds()
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<int>>(AlertThresholds) ?? new List<int> { 45, 30, 15, 7, 3, 1 };
            }
            catch
            {
                return new List<int> { 45, 30, 15, 7, 3, 1 };
            }
        }

        public void SetThresholds(List<int> thresholds)
        {
            AlertThresholds = System.Text.Json.JsonSerializer.Serialize(thresholds.OrderByDescending(t => t).ToList());
        }
    }
}
