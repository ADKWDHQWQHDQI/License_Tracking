using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.ViewModels;
using License_Tracking.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class AnalyticsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public AnalyticsController(AppDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Main Analytics Dashboard - Bigin.com Style
        [Authorize(Roles = "Admin,Management,Sales,Finance")]
        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = currentUser != null ? await _userManager.GetRolesAsync(currentUser) : new List<string>();

            var viewModel = new AnalyticsDashboardViewModel
            {
                CurrentUser = currentUser?.Email ?? "Unknown",
                UserRoles = userRoles,
                CurrentPeriod = DateTime.Now.ToString("MMMM yyyy"),
                IsAdmin = User.IsInRole("Admin") || User.IsInRole("Management")
            };

            // Load Current Analytics (Actual Data)
            viewModel.CurrentAnalytics = await GetCurrentAnalyticsAsync();

            // Load Pipeline Analytics (Future/Projected Data)
            viewModel.PipelineAnalytics = await GetPipelineAnalyticsAsync();

            return View(viewModel);
        }

        // Current Analytics - Actual Data from Deals
        private async Task<CurrentAnalyticsViewModel> GetCurrentAnalyticsAsync()
        {
            var currentYear = DateTime.Now.Year;

            // Get all completed/active deals
            var deals = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Where(d => d.LicenseStartDate.HasValue && d.LicenseStartDate.Value.Year == currentYear)
                .ToListAsync();

            // Customer-Revenue Analytics
            var customerRevenue = deals
                .GroupBy(d => d.Company.CompanyName)
                .Select(g => new CustomerRevenueAnalytics
                {
                    CustomerName = g.Key,
                    TotalRevenue = g.Sum(d => d.CustomerInvoiceAmount ?? 0),
                    TotalDeals = g.Count(),
                    AverageRevenue = g.Average(d => d.CustomerInvoiceAmount ?? 0),
                    LastDealDate = g.Max(d => d.CustomerPoDate),
                    RevenueGrowth = CalculateRevenueGrowth(g.Key, currentYear, currentYear - 1)
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();

            // OEM-Expenditure Analytics
            var oemExpenditure = deals
                .GroupBy(d => d.Oem.OemName)
                .Select(g => new OemExpenditureAnalytics
                {
                    OemName = g.Key,
                    TotalExpenditure = g.Sum(d => d.OemQuoteAmount ?? 0),
                    TotalDeals = g.Count(),
                    AverageExpenditure = g.Average(d => d.OemQuoteAmount ?? 0),
                    LastPurchaseDate = g.Max(d => d.CanarysPoDate),
                    ExpenditureGrowth = CalculateExpenditureGrowth(g.Key, currentYear, currentYear - 1)
                })
                .OrderByDescending(o => o.TotalExpenditure)
                .ToList();

            // Combined Analytics
            var combinedAnalytics = new CombinedAnalytics
            {
                TotalRevenue = customerRevenue.Sum(c => c.TotalRevenue),
                TotalExpenditure = oemExpenditure.Sum(o => o.TotalExpenditure),
                NetProfit = customerRevenue.Sum(c => c.TotalRevenue) - oemExpenditure.Sum(o => o.TotalExpenditure),
                TotalDeals = deals.Count,
                AverageDealSize = deals.Any() ? deals.Average(d => d.CustomerInvoiceAmount ?? 0) : 0,
                TopPerformingCustomer = customerRevenue.FirstOrDefault()?.CustomerName ?? "N/A",
                TopSpendingOem = oemExpenditure.FirstOrDefault()?.OemName ?? "N/A"
            };

            // Margin Analytics
            var marginAnalytics = new MarginAnalytics
            {
                TotalMargin = deals.Sum(d => d.EstimatedMargin ?? 0),
                AverageMarginPercentage = deals.Any() ? deals.Where(d => d.CustomerInvoiceAmount > 0)
                    .Average(d => ((d.EstimatedMargin ?? 0) / (d.CustomerInvoiceAmount ?? 1)) * 100) : 0,
                HighestMarginDeal = deals.OrderByDescending(d => d.EstimatedMargin).FirstOrDefault()?.DealName ?? "N/A",
                LowestMarginDeal = deals.OrderBy(d => d.EstimatedMargin).FirstOrDefault()?.DealName ?? "N/A",
                MarginTrend = CalculateMarginTrend(deals),
                MarginByCategory = GetMarginByCategory(deals)
            };

            return new CurrentAnalyticsViewModel
            {
                CustomerRevenue = customerRevenue,
                OemExpenditure = oemExpenditure,
                Combined = combinedAnalytics,
                Margin = marginAnalytics,
                Period = $"{DateTime.Now:MMMM yyyy}",
                LastUpdated = DateTime.Now
            };
        }

        // Pipeline Analytics - Future/Projected Data from ProjectPipeline
        private async Task<PipelineAnalyticsViewModel> GetPipelineAnalyticsAsync()
        {
            var currentDate = DateTime.Now;

            // Get active pipeline projects
            var pipelineProjects = await _context.ProjectPipelines
                .Where(p => p.ProjectStatus == "Pipeline" && p.ExpectedCloseDate >= currentDate)
                .ToListAsync();

            // Customer-Revenue Analytics (Pipeline)
            var pipelineCustomerRevenue = pipelineProjects
                .GroupBy(p => p.ClientName)
                .Select(g => new CustomerRevenueAnalytics
                {
                    CustomerName = g.Key,
                    TotalRevenue = g.Sum(p => p.ExpectedAmountToReceive),
                    TotalDeals = g.Count(),
                    AverageRevenue = g.Average(p => p.ExpectedAmountToReceive),
                    LastDealDate = g.Max(p => p.ExpectedCloseDate),
                    RevenueGrowth = CalculatePipelineGrowth(g.Key, "Customer")
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ToList();

            // OEM-Expenditure Analytics (Pipeline)
            var pipelineOemExpenditure = pipelineProjects
                .GroupBy(p => p.OemName)
                .Select(g => new OemExpenditureAnalytics
                {
                    OemName = g.Key,
                    TotalExpenditure = g.Sum(p => p.ExpectedAmountToPay),
                    TotalDeals = g.Count(),
                    AverageExpenditure = g.Average(p => p.ExpectedAmountToPay),
                    LastPurchaseDate = g.Max(p => p.ExpectedCloseDate),
                    ExpenditureGrowth = CalculatePipelineGrowth(g.Key, "OEM")
                })
                .OrderByDescending(o => o.TotalExpenditure)
                .ToList();

            // Combined Pipeline Analytics
            var pipelineCombined = new CombinedAnalytics
            {
                TotalRevenue = pipelineCustomerRevenue.Sum(c => c.TotalRevenue),
                TotalExpenditure = pipelineOemExpenditure.Sum(o => o.TotalExpenditure),
                NetProfit = pipelineCustomerRevenue.Sum(c => c.TotalRevenue) - pipelineOemExpenditure.Sum(o => o.TotalExpenditure),
                TotalDeals = pipelineProjects.Count,
                AverageDealSize = pipelineProjects.Any() ? pipelineProjects.Average(p => p.ExpectedAmountToReceive) : 0,
                TopPerformingCustomer = pipelineCustomerRevenue.FirstOrDefault()?.CustomerName ?? "N/A",
                TopSpendingOem = pipelineOemExpenditure.FirstOrDefault()?.OemName ?? "N/A"
            };

            // Pipeline Margin Analytics
            var pipelineMargin = new MarginAnalytics
            {
                TotalMargin = pipelineProjects.Sum(p => p.ExpectedAmountToReceive - p.ExpectedAmountToPay),
                AverageMarginPercentage = pipelineProjects.Any() ? pipelineProjects
                    .Where(p => p.ExpectedAmountToReceive > 0)
                    .Average(p => ((p.ExpectedAmountToReceive - p.ExpectedAmountToPay) / p.ExpectedAmountToReceive) * 100) : 0,
                HighestMarginDeal = pipelineProjects
                    .OrderByDescending(p => p.ExpectedAmountToReceive - p.ExpectedAmountToPay)
                    .FirstOrDefault()?.ProductName ?? "N/A",
                LowestMarginDeal = pipelineProjects
                    .OrderBy(p => p.ExpectedAmountToReceive - p.ExpectedAmountToPay)
                    .FirstOrDefault()?.ProductName ?? "N/A",
                MarginTrend = CalculatePipelineMarginTrend(pipelineProjects),
                MarginByCategory = GetPipelineMarginByCategory(pipelineProjects)
            };

            return new PipelineAnalyticsViewModel
            {
                CustomerRevenue = pipelineCustomerRevenue,
                OemExpenditure = pipelineOemExpenditure,
                Combined = pipelineCombined,
                Margin = pipelineMargin,
                Period = "Pipeline Forecast",
                LastUpdated = DateTime.Now,
                WeightedRevenue = pipelineProjects.Sum(p => p.ExpectedAmountToReceive * (p.SuccessProbability / 100.0m)),
                WeightedMargin = pipelineProjects.Sum(p => (p.ExpectedAmountToReceive - p.ExpectedAmountToPay) * (p.SuccessProbability / 100.0m))
            };
        }

        // API Endpoints for AJAX calls
        [HttpGet]
        public async Task<JsonResult> GetCurrentAnalyticsData()
        {
            try
            {
                var analytics = await GetCurrentAnalyticsAsync();
                return Json(analytics);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetPipelineAnalyticsData()
        {
            try
            {
                var analytics = await GetPipelineAnalyticsAsync();
                return Json(analytics);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> GetCombinedAnalytics()
        {
            try
            {
                var currentAnalytics = await GetCurrentAnalyticsAsync();
                var pipelineAnalytics = await GetPipelineAnalyticsAsync();

                var combined = new
                {
                    Current = currentAnalytics,
                    Pipeline = pipelineAnalytics,
                    TotalCombinedRevenue = currentAnalytics.Combined.TotalRevenue + pipelineAnalytics.Combined.TotalRevenue,
                    TotalCombinedMargin = currentAnalytics.Margin.TotalMargin + pipelineAnalytics.Margin.TotalMargin,
                    OverallPerformance = CalculateOverallPerformance(currentAnalytics, pipelineAnalytics)
                };

                return Json(combined);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Helper Methods
        private decimal CalculateRevenueGrowth(string customerName, int currentYear, int previousYear)
        {
            var currentYearRevenue = _context.Deals
                .Where(d => d.Company.CompanyName == customerName &&
                           d.LicenseStartDate.HasValue &&
                           d.LicenseStartDate.Value.Year == currentYear)
                .Sum(d => d.CustomerInvoiceAmount ?? 0);

            var previousYearRevenue = _context.Deals
                .Where(d => d.Company.CompanyName == customerName &&
                           d.LicenseStartDate.HasValue &&
                           d.LicenseStartDate.Value.Year == previousYear)
                .Sum(d => d.CustomerInvoiceAmount ?? 0);

            if (previousYearRevenue == 0) return currentYearRevenue > 0 ? 100 : 0;
            return ((currentYearRevenue - previousYearRevenue) / previousYearRevenue) * 100;
        }

        private decimal CalculateExpenditureGrowth(string oemName, int currentYear, int previousYear)
        {
            var currentYearExpenditure = _context.Deals
                .Where(d => d.Oem.OemName == oemName &&
                           d.LicenseStartDate.HasValue &&
                           d.LicenseStartDate.Value.Year == currentYear)
                .Sum(d => d.OemQuoteAmount ?? 0);

            var previousYearExpenditure = _context.Deals
                .Where(d => d.Oem.OemName == oemName &&
                           d.LicenseStartDate.HasValue &&
                           d.LicenseStartDate.Value.Year == previousYear)
                .Sum(d => d.OemQuoteAmount ?? 0);

            if (previousYearExpenditure == 0) return currentYearExpenditure > 0 ? 100 : 0;
            return ((currentYearExpenditure - previousYearExpenditure) / previousYearExpenditure) * 100;
        }

        private decimal CalculatePipelineGrowth(string entityName, string type)
        {
            // Compare current pipeline with previous month
            var currentMonth = DateTime.Now.Month;
            var previousMonth = currentMonth == 1 ? 12 : currentMonth - 1;
            var year = currentMonth == 1 ? DateTime.Now.Year - 1 : DateTime.Now.Year;

            if (type == "Customer")
            {
                var currentValue = _context.ProjectPipelines
                    .Where(p => p.ClientName == entityName && p.CreatedDate.Month == currentMonth)
                    .Sum(p => p.ExpectedAmountToReceive);

                var previousValue = _context.ProjectPipelines
                    .Where(p => p.ClientName == entityName &&
                               p.CreatedDate.Month == previousMonth &&
                               p.CreatedDate.Year == year)
                    .Sum(p => p.ExpectedAmountToReceive);

                if (previousValue == 0) return currentValue > 0 ? 100 : 0;
                return ((currentValue - previousValue) / previousValue) * 100;
            }
            else
            {
                var currentValue = _context.ProjectPipelines
                    .Where(p => p.OemName == entityName && p.CreatedDate.Month == currentMonth)
                    .Sum(p => p.ExpectedAmountToPay);

                var previousValue = _context.ProjectPipelines
                    .Where(p => p.OemName == entityName &&
                               p.CreatedDate.Month == previousMonth &&
                               p.CreatedDate.Year == year)
                    .Sum(p => p.ExpectedAmountToPay);

                if (previousValue == 0) return currentValue > 0 ? 100 : 0;
                return ((currentValue - previousValue) / previousValue) * 100;
            }
        }

        private List<decimal> CalculateMarginTrend(List<Deal> deals)
        {
            return deals.GroupBy(d => d.LicenseStartDate?.Month ?? DateTime.Now.Month)
                       .OrderBy(g => g.Key)
                       .Select(g => g.Sum(d => d.EstimatedMargin ?? 0))
                       .ToList();
        }

        private List<decimal> CalculatePipelineMarginTrend(List<ProjectPipeline> projects)
        {
            return projects.GroupBy(p => p.ExpectedCloseDate?.Month ?? DateTime.Now.Month)
                          .OrderBy(g => g.Key)
                          .Select(g => g.Sum(p => p.ExpectedAmountToReceive - p.ExpectedAmountToPay))
                          .ToList();
        }

        private Dictionary<string, decimal> GetMarginByCategory(List<Deal> deals)
        {
            return deals.GroupBy(d => d.Product?.ProductCategory ?? "Unknown")
                       .ToDictionary(g => g.Key, g => g.Sum(d => d.EstimatedMargin ?? 0));
        }

        private Dictionary<string, decimal> GetPipelineMarginByCategory(List<ProjectPipeline> projects)
        {
            return projects.GroupBy(p => p.OemType ?? "Unknown")
                          .ToDictionary(g => g.Key,
                                      g => g.Sum(p => p.ExpectedAmountToReceive - p.ExpectedAmountToPay));
        }

        private object CalculateOverallPerformance(CurrentAnalyticsViewModel current, PipelineAnalyticsViewModel pipeline)
        {
            return new
            {
                PerformanceScore = CalculatePerformanceScore(current, pipeline),
                GrowthPotential = CalculateGrowthPotential(current, pipeline),
                RiskAssessment = CalculateRiskAssessment(pipeline),
                Recommendations = GenerateRecommendations(current, pipeline)
            };
        }

        private decimal CalculatePerformanceScore(CurrentAnalyticsViewModel current, PipelineAnalyticsViewModel pipeline)
        {
            var revenueScore = current.Combined.TotalRevenue > 0 ? Math.Min(100, (current.Combined.TotalRevenue / 1000000) * 100) : 0;
            var marginScore = current.Margin.AverageMarginPercentage;
            var pipelineScore = pipeline.Combined.TotalRevenue > 0 ? Math.Min(100, (pipeline.Combined.TotalRevenue / 500000) * 100) : 0;

            return (revenueScore + marginScore + pipelineScore) / 3;
        }

        private decimal CalculateGrowthPotential(CurrentAnalyticsViewModel current, PipelineAnalyticsViewModel pipeline)
        {
            if (current.Combined.TotalRevenue == 0) return pipeline.Combined.TotalRevenue > 0 ? 100 : 0;
            return (pipeline.Combined.TotalRevenue / current.Combined.TotalRevenue) * 100;
        }

        private string CalculateRiskAssessment(PipelineAnalyticsViewModel pipeline)
        {
            var avgSuccessRate = pipeline.WeightedRevenue / Math.Max(pipeline.Combined.TotalRevenue, 1) * 100;

            if (avgSuccessRate >= 70) return "Low Risk";
            if (avgSuccessRate >= 50) return "Medium Risk";
            return "High Risk";
        }

        private List<string> GenerateRecommendations(CurrentAnalyticsViewModel current, PipelineAnalyticsViewModel pipeline)
        {
            var recommendations = new List<string>();

            if (current.Margin.AverageMarginPercentage < 20)
                recommendations.Add("Consider improving margin by negotiating better rates with OEMs");

            if (pipeline.Combined.TotalDeals < 5)
                recommendations.Add("Focus on building a stronger sales pipeline");

            if (current.CustomerRevenue.Count < 10)
                recommendations.Add("Diversify customer base to reduce dependency risk");

            return recommendations;
        }
    }
}
