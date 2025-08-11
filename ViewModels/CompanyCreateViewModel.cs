using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    public class CompanyCreateViewModel
    {
        [Required]
        [Display(Name = "Company Name")]
        public string CompanyName { get; set; } = string.Empty;

        [Display(Name = "Industry")]
        public string? Industry { get; set; }

        [Display(Name = "Company Size")]
        public string? CompanySize { get; set; }

        [Display(Name = "Annual Revenue")]
        public decimal? AnnualRevenue { get; set; }

        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "Headquarters")]
        public string? Headquarters { get; set; }

        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [EmailAddress]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Display(Name = "Website")]
        public string? Website { get; set; }

        [Display(Name = "Company Type")]
        public string? CompanyType { get; set; }

        [Display(Name = "Payment Terms")]
        public string? PaymentTerms { get; set; }

        [Display(Name = "Primary Business")]
        public string? PrimaryBusiness { get; set; }

        [Display(Name = "Technology Stack")]
        public string? TechnologyStack { get; set; }

        [Display(Name = "Current Vendors")]
        public string? CurrentVendors { get; set; }

        // Contact Person Details
        [Required]
        [Display(Name = "Primary Contact Name")]
        public string PrimaryContactName { get; set; } = string.Empty;

        [EmailAddress]
        [Display(Name = "Primary Contact Email")]
        public string? PrimaryContactEmail { get; set; }

        [Display(Name = "Primary Contact Number")]
        public string? PrimaryContactNumber { get; set; }

        [Display(Name = "Primary Contact Designation")]
        public string? PrimaryContactDesignation { get; set; }

        [Display(Name = "Primary Contact Department")]
        public string? PrimaryContactDepartment { get; set; }

        // Dropdown options
        public static readonly string[] CompanySizeOptions = { "1-50", "51-200", "201-1000", "1000+" };
        public static readonly string[] CompanyTypeOptions = { "Prospect", "Customer", "Partner" };
        public static readonly string[] PaymentTermsOptions = { "Net 30", "Net 45", "Immediate", "Net 60" };
        public static readonly string[] DecisionMakerLevels = { "Primary", "Secondary", "Influencer" };
    }
}
