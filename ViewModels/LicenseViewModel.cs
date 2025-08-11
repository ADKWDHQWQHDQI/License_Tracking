using System;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    // Legacy LicenseViewModel for backward compatibility
    public class LicenseViewModel : DealViewModel
    {
        // Additional properties expected by legacy views only
        public string? ShipToAddress { get; set; }
        public string? BillToAddress { get; set; }
        public string? Remarks { get; set; }
    }
}