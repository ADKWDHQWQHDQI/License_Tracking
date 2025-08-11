using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class ProfitAnalysisViewModel
    {
        public decimal CurrentMonthRevenue { get; set; }
        public decimal CurrentMonthCost { get; set; }
        public decimal CurrentMonthProfit { get; set; }
        public decimal ProfitMarginPercentage { get; set; }
        public decimal RevenueGrowthRate { get; set; }
        public decimal ProfitGrowthRate { get; set; }
        public List<MonthlyDataPoint> QuarterlyTrends { get; set; } = new();
        public List<CustomerProfitData> TopProfitableCustomers { get; set; } = new();
        public List<OemCostData> TopCostlyOEMs { get; set; } = new();
        public decimal AverageProfitMargin { get; set; }
        public decimal TotalYearlyRevenue { get; set; }
        public class MonthlyDataPoint
        {
            public string Month { get; set; } = string.Empty;
            public decimal Revenue { get; set; }
            public decimal Cost { get; set; }
            public decimal Profit { get; set; }
        }
    }

    public class CustomerProfitData
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitMargin { get; set; }
    }

    public class OemCostData
    {
        public string OemName { get; set; } = string.Empty;
        public decimal TotalCost { get; set; }
        public int LicenseCount { get; set; }
        public decimal AverageCost { get; set; }
    }
}
