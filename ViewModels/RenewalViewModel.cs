using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class RenewalViewModel
    {
        public int RenewalId { get; set; }
        public int? DealId { get; set; }
        public int? LicenseId { get; set; }
        public int? ProjectPipelineId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public DateTime RenewalDate { get; set; }
        public decimal RenewalAmount { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime? LicenseEndDate { get; set; }
        public string? LicenseStatus { get; set; }
        public Deal? Deal { get; set; }
    }
}
