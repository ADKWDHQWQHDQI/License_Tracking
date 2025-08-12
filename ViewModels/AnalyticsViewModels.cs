using System;
using System.Collections.Generic;

namespace License_Tracking.ViewModels
{
    // Main Analytics Dashboard ViewModel
    public class AnalyticsDashboardViewModel
    {
        public string CurrentUser { get; set; } = string.Empty;
        public IList<string> UserRoles { get; set; } = new List<string>();
        public string CurrentPeriod { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public CurrentAnalyticsViewModel CurrentAnalytics { get; set; } = new CurrentAnalyticsViewModel();
        public PipelineAnalyticsViewModel PipelineAnalytics { get; set; } = new PipelineAnalyticsViewModel();
    }

    // Current Analytics (Actual Data from Deals)
    public class CurrentAnalyticsViewModel
    {
        public List<CustomerRevenueAnalytics> CustomerRevenue { get; set; } = new List<CustomerRevenueAnalytics>();
        public List<OemExpenditureAnalytics> OemExpenditure { get; set; } = new List<OemExpenditureAnalytics>();
        public CombinedAnalytics Combined { get; set; } = new CombinedAnalytics();
        public MarginAnalytics Margin { get; set; } = new MarginAnalytics();
        public string Period { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }

    // Pipeline Analytics (Future/Projected Data)
    public class PipelineAnalyticsViewModel
    {
        public List<CustomerRevenueAnalytics> CustomerRevenue { get; set; } = new List<CustomerRevenueAnalytics>();
        public List<OemExpenditureAnalytics> OemExpenditure { get; set; } = new List<OemExpenditureAnalytics>();
        public CombinedAnalytics Combined { get; set; } = new CombinedAnalytics();
        public MarginAnalytics Margin { get; set; } = new MarginAnalytics();
        public string Period { get; set; } = string.Empty;
        public DateTime LastUpdated { get; set; } = DateTime.Now;
        public decimal WeightedRevenue { get; set; }
        public decimal WeightedMargin { get; set; }
    }

    // Customer-Revenue Analytics
    public class CustomerRevenueAnalytics
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalDeals { get; set; }
        public decimal AverageRevenue { get; set; }
        public DateTime? LastDealDate { get; set; }
        public decimal RevenueGrowth { get; set; }
        public string GrowthTrend => RevenueGrowth >= 0 ? "positive" : "negative";
        public string GrowthIcon => RevenueGrowth >= 0 ? "fas fa-arrow-up text-success" : "fas fa-arrow-down text-danger";
    }

    // OEM-Expenditure Analytics
    public class OemExpenditureAnalytics
    {
        public string OemName { get; set; } = string.Empty;
        public decimal TotalExpenditure { get; set; }
        public int TotalDeals { get; set; }
        public decimal AverageExpenditure { get; set; }
        public DateTime? LastPurchaseDate { get; set; }
        public decimal ExpenditureGrowth { get; set; }
        public string GrowthTrend => ExpenditureGrowth >= 0 ? "positive" : "negative";
        public string GrowthIcon => ExpenditureGrowth >= 0 ? "fas fa-arrow-up text-warning" : "fas fa-arrow-down text-success";
    }

    // Combined Analytics (OEM and Customer)
    public class CombinedAnalytics
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalExpenditure { get; set; }
        public decimal NetProfit { get; set; }
        public int TotalDeals { get; set; }
        public decimal AverageDealSize { get; set; }
        public string TopPerformingCustomer { get; set; } = string.Empty;
        public string TopSpendingOem { get; set; } = string.Empty;
        public decimal ProfitMarginPercentage => TotalRevenue > 0 ? (NetProfit / TotalRevenue) * 100 : 0;
        public string ProfitTrend => NetProfit >= 0 ? "positive" : "negative";
    }

    // Margin Analytics
    public class MarginAnalytics
    {
        public decimal TotalMargin { get; set; }
        public decimal AverageMarginPercentage { get; set; }
        public string HighestMarginDeal { get; set; } = string.Empty;
        public string LowestMarginDeal { get; set; } = string.Empty;
        public List<decimal> MarginTrend { get; set; } = new List<decimal>();
        public Dictionary<string, decimal> MarginByCategory { get; set; } = new Dictionary<string, decimal>();
        public string MarginStatus => AverageMarginPercentage >= 25 ? "Excellent" :
                                     AverageMarginPercentage >= 15 ? "Good" :
                                     AverageMarginPercentage >= 10 ? "Average" : "Poor";
        public string MarginStatusColor => AverageMarginPercentage >= 25 ? "success" :
                                          AverageMarginPercentage >= 15 ? "primary" :
                                          AverageMarginPercentage >= 10 ? "warning" : "danger";
    }

    // Enhanced Analytics Charts Data
    public class AnalyticsChartsViewModel
    {
        public List<ChartDataPoint> RevenueByMonth { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> ExpenditureByMonth { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> MarginByMonth { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> CustomerDistribution { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> OemDistribution { get; set; } = new List<ChartDataPoint>();
        public List<ChartDataPoint> ProductCategoryPerformance { get; set; } = new List<ChartDataPoint>();
    }

    public class ChartDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public string Color { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
    }

    // Performance Indicators
    public class PerformanceIndicators
    {
        public decimal RevenueGrowthRate { get; set; }
        public decimal MarginImprovementRate { get; set; }
        public decimal CustomerRetentionRate { get; set; }
        public decimal DealConversionRate { get; set; }
        public decimal AverageTimeToClose { get; set; }
        public string OverallHealthScore { get; set; } = string.Empty;
        public List<string> KeyInsights { get; set; } = new List<string>();
        public List<string> Recommendations { get; set; } = new List<string>();
    }

    // Comparative Analytics (Current vs Pipeline)
    public class ComparativeAnalyticsViewModel
    {
        public string ComparisonPeriod { get; set; } = string.Empty;
        public decimal CurrentRevenue { get; set; }
        public decimal PipelineRevenue { get; set; }
        public decimal CurrentMargin { get; set; }
        public decimal PipelineMargin { get; set; }
        public decimal RevenueVariance { get; set; }
        public decimal MarginVariance { get; set; }
        public string PerformanceTrend { get; set; } = string.Empty;
        public List<ComparisonDataPoint> DetailedComparison { get; set; } = new List<ComparisonDataPoint>();
    }

    public class ComparisonDataPoint
    {
        public string Metric { get; set; } = string.Empty;
        public decimal CurrentValue { get; set; }
        public decimal PipelineValue { get; set; }
        public decimal Variance { get; set; }
        public string VarianceType { get; set; } = string.Empty; // "positive", "negative", "neutral"
    }

    // Top Performers
    public class TopPerformersViewModel
    {
        public List<TopCustomer> TopCustomers { get; set; } = new List<TopCustomer>();
        public List<TopOem> TopOems { get; set; } = new List<TopOem>();
        public List<TopProduct> TopProducts { get; set; } = new List<TopProduct>();
        public List<TopDeal> TopDeals { get; set; } = new List<TopDeal>();
    }

    public class TopCustomer
    {
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int DealCount { get; set; }
        public decimal AverageRevenue { get; set; }
        public string LastActivity { get; set; } = string.Empty;
        public string TrendIcon { get; set; } = string.Empty;
    }

    public class TopOem
    {
        public string OemName { get; set; } = string.Empty;
        public decimal TotalExpenditure { get; set; }
        public int DealCount { get; set; }
        public decimal AverageExpenditure { get; set; }
        public string LastPurchase { get; set; } = string.Empty;
        public string PerformanceRating { get; set; } = string.Empty;
    }

    public class TopProduct
    {
        public string ProductName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int SalesCount { get; set; }
        public decimal AverageMargin { get; set; }
        public string PopularityTrend { get; set; } = string.Empty;
    }

    public class TopDeal
    {
        public string DealName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string OemName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public decimal Margin { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CloseDate { get; set; }
    }
}
