using System.ComponentModel.DataAnnotations;
using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class DealCreateViewModel
    {
        [Required]
        [Display(Name = "Deal/Project Name")]
        public string DealName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Customer/Company")]
        public int CompanyId { get; set; }

        [Required]
        [Display(Name = "OEM Partner")]
        public int OemId { get; set; }

        [Required]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Display(Name = "Contact Person")]
        public int? ContactId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        [Display(Name = "Deal Type")]
        public string? DealType { get; set; }

        [Display(Name = "Expected Close Date")]
        public DateTime? ExpectedCloseDate { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        [Display(Name = "Deal Probability")]
        [Range(0, 1, ErrorMessage = "Probability must be between 0 and 1")]
        public decimal? DealProbability { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Navigation Properties for Dropdowns
        public List<Company> Companies { get; set; } = new List<Company>();
        public List<Oem> Oems { get; set; } = new List<Oem>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<ContactPerson> ContactPersons { get; set; } = new List<ContactPerson>();

        // Dropdown options
        public static readonly string[] DealTypeOptions = { "New", "Renewal", "Upgrade" };
        public static readonly string[] DealStageOptions = { "Lead", "Quoted", "Negotiation", "Won", "Lost" };
    }
}
