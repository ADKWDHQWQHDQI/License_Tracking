using License_Tracking.Models;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    public class PipelineViewModel
    {
        public List<Deal> Deals { get; set; } = new List<Deal>();
        public PipelineFilters Filters { get; set; } = new PipelineFilters();
        public PipelineSummary Summary { get; set; } = new PipelineSummary();
        public string ViewType { get; set; } = "List"; // List, Sheet, Kanban
    }

    public class PipelineFilters
    {
        [Display(Name = "Deal Stage")]
        public string? DealStage { get; set; }

        [Display(Name = "Assigned To")]
        public string? AssignedTo { get; set; }

        [Display(Name = "OEM Partner")]
        public int? OemId { get; set; }

        [Display(Name = "Customer")]
        public int? CompanyId { get; set; }

        [Display(Name = "Date From")]
        public DateTime? DateFrom { get; set; }

        [Display(Name = "Date To")]
        public DateTime? DateTo { get; set; }

        // Available options for dropdowns
        public List<Oem> Oems { get; set; } = new List<Oem>();
        public List<Company> Companies { get; set; } = new List<Company>();
        public List<string> AssignedUsers { get; set; } = new List<string>();
    }

    public class PipelineSummary
    {
        public int TotalDeals { get; set; }
        public decimal TotalValue { get; set; }
        public decimal WeightedValue { get; set; }
        public Dictionary<string, int> DealsByStage { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, decimal> ValueByStage { get; set; } = new Dictionary<string, decimal>();
    }

    public class KanbanBoardViewModel
    {
        public Dictionary<string, List<Deal>> DealsByStage { get; set; } = new Dictionary<string, List<Deal>>();
        public List<string> Stages { get; set; } = new List<string> { "Lead", "Quoted", "Negotiation", "Won", "Lost" };
        public PipelineSummary Summary { get; set; } = new PipelineSummary();
    }
}
