using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    public class ReportDashboardViewModel
    {
        public int TotalDeals { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCosts { get; set; }
        public decimal TotalMargin { get; set; }
        public List<CustomerProfitabilityViewModel> TopCustomers { get; set; } = new();
        public List<OemEfficiencyViewModel> TopOEMs { get; set; } = new();
        public List<string> RecentReports { get; set; } = new();

        // Legacy compatibility properties
        public int TotalLicenses => TotalDeals;
    }

    public class MarginBreakupViewModel
    {
        public int DealId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "OEM Name")]
        public string OemName { get; set; } = string.Empty;

        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [Display(Name = "Amount Received")]
        [DataType(DataType.Currency)]
        public decimal AmountReceived { get; set; }

        [Display(Name = "Amount Paid")]
        [DataType(DataType.Currency)]
        public decimal AmountPaid { get; set; }

        [Display(Name = "Calculated Margin")]
        [DataType(DataType.Currency)]
        public decimal CalculatedMargin { get; set; }

        [Display(Name = "Negotiated Margin")]
        [DataType(DataType.Currency)]
        public decimal NegotiatedMargin { get; set; }

        [Display(Name = "Actual Margin")]
        [DataType(DataType.Currency)]
        public decimal ActualMargin { get; set; }

        [Display(Name = "Margin Type")]
        public string MarginType { get; set; } = string.Empty;

        [Display(Name = "Profit Percentage")]
        public decimal ProfitPercentage { get; set; }

        [Display(Name = "Deal Date")]
        [DataType(DataType.Date)]
        public DateTime DealDate { get; set; }

        // Legacy compatibility properties
        public int LicenseId => DealId;
        public DateTime LicenseDate => DealDate;
        public decimal ManualMarginInput => ActualMargin;
    }

    public class RevenueBreakdownViewModel
    {
        [Display(Name = "OEM Name")]
        public string OemName { get; set; } = string.Empty;

        [Display(Name = "Client Name")]
        public string ClientName { get; set; } = string.Empty;

        [Display(Name = "Total Revenue")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Costs")]
        [DataType(DataType.Currency)]
        public decimal TotalCosts { get; set; }

        [Display(Name = "OEM Fixed Costs")]
        [DataType(DataType.Currency)]
        public decimal OEMFixedCosts { get; set; }

        [Display(Name = "Customer Negotiated Revenue")]
        [DataType(DataType.Currency)]
        public decimal CustomerNegotiatedRevenue { get; set; }

        [Display(Name = "Total Margin")]
        [DataType(DataType.Currency)]
        public decimal TotalMargin { get; set; }

        [Display(Name = "Deal Count")]
        public int DealCount { get; set; }

        [Display(Name = "Average Margin per Deal")]
        [DataType(DataType.Currency)]
        public decimal AvgMarginPerDeal { get; set; }

        [Display(Name = "Profit Margin %")]
        public decimal ProfitMarginPercentage { get; set; }

        // Legacy compatibility properties
        public int LicenseCount => DealCount;
        public decimal AvgMarginPerLicense => AvgMarginPerDeal;
    }

    public class ReportProfitAnalysisViewModel
    {
        [Display(Name = "Group Name")]
        public string GroupName { get; set; } = string.Empty;

        [Display(Name = "Group Type")]
        public string GroupType { get; set; } = string.Empty;

        [Display(Name = "Total Revenue")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Costs")]
        [DataType(DataType.Currency)]
        public decimal TotalCosts { get; set; }

        [Display(Name = "Total Profit")]
        [DataType(DataType.Currency)]
        public decimal TotalProfit { get; set; }

        [Display(Name = "Deal Count")]
        public int DealCount { get; set; }

        [Display(Name = "Average Profit per Deal")]
        [DataType(DataType.Currency)]
        public decimal AvgProfitPerDeal { get; set; }

        [Display(Name = "Profit Margin %")]
        public decimal ProfitMarginPercentage { get; set; }

        [Display(Name = "ROI %")]
        public decimal ROI { get; set; }

        // Legacy compatibility properties
        public int LicenseCount => DealCount;
        public decimal AvgProfitPerLicense => AvgProfitPerDeal;
    }

    public class CustomerProfitabilityViewModel
    {
        public int Rank { get; set; }

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Empty;

        [Display(Name = "Total Revenue")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Costs")]
        [DataType(DataType.Currency)]
        public decimal TotalCosts { get; set; }

        [Display(Name = "Total Profit")]
        [DataType(DataType.Currency)]
        public decimal TotalProfit { get; set; }

        [Display(Name = "Deal Count")]
        public int DealCount { get; set; }

        [Display(Name = "Average Revenue per Deal")]
        [DataType(DataType.Currency)]
        public decimal AvgRevenuePerDeal { get; set; }

        [Display(Name = "Average Profit per Deal")]
        [DataType(DataType.Currency)]
        public decimal AvgProfitPerDeal { get; set; }

        [Display(Name = "Profit Margin %")]
        public decimal ProfitMarginPercentage { get; set; }

        [Display(Name = "Last Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime LastPurchaseDate { get; set; }

        [Display(Name = "Most Purchased Product")]
        public string MostPurchasedProduct { get; set; } = string.Empty;

        // Legacy compatibility properties
        public int LicenseCount => DealCount;
        public decimal AvgRevenuePerLicense => AvgRevenuePerDeal;
        public decimal AvgProfitPerLicense => AvgProfitPerDeal;
    }

    public class OemEfficiencyViewModel
    {
        public int EfficiencyRank { get; set; }

        [Display(Name = "OEM Name")]
        public string OemName { get; set; } = string.Empty;

        [Display(Name = "Total Costs")]
        [DataType(DataType.Currency)]
        public decimal TotalCosts { get; set; }

        [Display(Name = "Total Revenue")]
        [DataType(DataType.Currency)]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Total Margin")]
        [DataType(DataType.Currency)]
        public decimal TotalMargin { get; set; }

        [Display(Name = "Deal Count")]
        public int DealCount { get; set; }

        [Display(Name = "Average Cost per Deal")]
        [DataType(DataType.Currency)]
        public decimal AvgCostPerDeal { get; set; }

        [Display(Name = "Average Margin per Deal")]
        [DataType(DataType.Currency)]
        public decimal AvgMarginPerDeal { get; set; }

        [Display(Name = "Cost Efficiency Ratio")]
        public decimal CostEfficiencyRatio { get; set; }

        [Display(Name = "Margin Percentage")]
        public decimal MarginPercentage { get; set; }

        [Display(Name = "Last Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime LastPurchaseDate { get; set; }

        [Display(Name = "Top Product")]
        public string TopProduct { get; set; } = string.Empty;

        // Legacy compatibility properties
        public int LicenseCount => DealCount;
        public decimal AvgCostPerLicense => AvgCostPerDeal;
        public decimal AvgMarginPerLicense => AvgMarginPerDeal;
    }
}
