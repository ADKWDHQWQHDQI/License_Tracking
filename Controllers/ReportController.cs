using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using License_Tracking.Data;
using License_Tracking.Models;
using License_Tracking.Services;
using License_Tracking.ViewModels;
using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

using OfficeOpenXml;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text.Json;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ReportService _reportService;

        public ReportController(AppDbContext context, ReportService reportService)
        {
            _context = context;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index()
        {
            var userRole = User.IsInRole("Admin") ? "Admin" :
                          User.IsInRole("Finance") ? "Finance" :
                          User.IsInRole("Operations") ? "Operations" :
                          User.IsInRole("Management") ? "Management" : "Sales";

            // Get basic license data for dashboard calculations
            var deals = await _context.Deals.Include(d => d.Company).Include(d => d.Oem).Include(d => d.Product).ToListAsync();

            // Calculate dashboard metrics
            var totalRevenue = deals.Sum(d => d.CustomerInvoiceAmount ?? 0);
            var totalCosts = deals.Sum(d => d.OemQuoteAmount ?? 0);
            var totalMargin = deals.Sum(d => d.EstimatedMargin ?? 0);

            // Get top customers by profit
            var topCustomers = deals
                .GroupBy(d => d.Company.CompanyName)
                .Select(g => new CustomerProfitabilityViewModel
                {
                    CustomerName = g.Key,
                    TotalRevenue = g.Sum(l => l.CustomerInvoiceAmount ?? 0),
                    TotalCosts = g.Sum(l => l.OemInvoiceAmount ?? 0),
                    TotalProfit = g.Sum(l => l.EstimatedMargin ?? ((l.CustomerInvoiceAmount ?? 0) - (l.OemInvoiceAmount ?? 0))),
                    DealCount = g.Count(),
                    AvgRevenuePerDeal = g.Count() > 0 ? g.Sum(l => l.CustomerInvoiceAmount ?? 0) / g.Count() : 0,
                    ProfitMarginPercentage = g.Sum(l => l.CustomerInvoiceAmount ?? 0) > 0 ? (g.Sum(l => l.EstimatedMargin ?? ((l.CustomerInvoiceAmount ?? 0) - (l.OemInvoiceAmount ?? 0))) / g.Sum(l => l.CustomerInvoiceAmount ?? 0)) * 100 : 0
                })
                .OrderByDescending(c => c.TotalProfit)
                .Take(5)
                .ToList();

            // Get top OEMs by efficiency
            var topOEMs = deals
                .Where(l => l.OemName != null)
                .GroupBy(l => l.OemName!)
                .Select(g => new OemEfficiencyViewModel
                {
                    OemName = g.Key,
                    TotalCosts = g.Sum(l => l.OemInvoiceAmount ?? 0),
                    TotalRevenue = g.Sum(l => l.CustomerInvoiceAmount ?? 0),
                    TotalMargin = g.Sum(l => l.EstimatedMargin ?? ((l.CustomerInvoiceAmount ?? 0) - (l.OemInvoiceAmount ?? 0))),
                    DealCount = g.Count(),
                    AvgCostPerDeal = g.Count() > 0 ? g.Sum(l => l.OemInvoiceAmount ?? 0) / g.Count() : 0,
                    AvgMarginPerDeal = g.Count() > 0 ? g.Sum(l => l.EstimatedMargin ?? ((l.CustomerInvoiceAmount ?? 0) - (l.OemInvoiceAmount ?? 0))) / g.Count() : 0,
                    CostEfficiencyRatio = g.Sum(l => l.OemInvoiceAmount ?? 0) > 0 ? g.Sum(l => l.EstimatedMargin ?? ((l.CustomerInvoiceAmount ?? 0) - (l.OemInvoiceAmount ?? 0))) / g.Sum(l => l.OemInvoiceAmount ?? 0) : 0,
                    MarginPercentage = g.Sum(l => l.CustomerInvoiceAmount ?? 0) > 0 ? (g.Sum(l => l.EstimatedMargin ?? ((l.CustomerInvoiceAmount ?? 0) - (l.OemInvoiceAmount ?? 0))) / g.Sum(l => l.CustomerInvoiceAmount ?? 0)) * 100 : 0
                })
                .OrderByDescending(o => o.CostEfficiencyRatio)
                .Take(5)
                .ToList();

            // Get recent reports
            var recentReports = await _context.Reports
                .Where(r => r.GeneratedByRole == userRole)
                .OrderByDescending(r => r.GeneratedDate)
                .Take(5)
                .Select(r => $"{r.ReportType} - {r.GeneratedDate:MMM dd, yyyy}")
                .ToListAsync();

            var dashboardModel = new ReportDashboardViewModel
            {
                TotalDeals = deals.Count,
                TotalRevenue = totalRevenue,
                TotalCosts = totalCosts,
                TotalMargin = totalMargin,
                TopCustomers = topCustomers,
                TopOEMs = topOEMs,
                RecentReports = recentReports
            };

            return View(dashboardModel);
        }

        [HttpGet]
        public async Task<IActionResult> Generate(string reportType, DateTime? startDate, DateTime? endDate, string? filter = null)
        {
            Report? report = null;
            var userRole = User.IsInRole("Admin") ? "Admin" :
                          User.IsInRole("Finance") ? "Finance" :
                          User.IsInRole("Operations") ? "Operations" :
                          User.IsInRole("Management") ? "Management" : "Sales";

            // Safely extract periodType if needed
            var periodType = "Monthly";
            if (!string.IsNullOrEmpty(reportType) && reportType.Contains("_"))
            {
                var parts = reportType.Split('_');
                if (parts.Length > 1)
                    periodType = parts[1];
            }

            switch (reportType)
            {
                case "CustomerSummary":
                    if (User.IsInRole("Admin") || User.IsInRole("Sales"))
                        report = await _reportService.GenerateCustomerSummaryReport(startDate, endDate, filter);
                    break;
                case "ProductBusiness":
                    if (User.IsInRole("Admin") || User.IsInRole("Sales"))
                        report = await _reportService.GenerateProductBusinessReport(startDate, endDate, filter);
                    break;
                case "OemProcurementCost":
                    if (User.IsInRole("Admin") || User.IsInRole("Operations"))
                        report = await _reportService.GenerateOemProcurementCostReport(startDate, endDate, filter);
                    break;
                case "FinancialSummaryMonthly":
                case "FinancialSummaryQuarterly":
                case "FinancialSummaryYearly":
                    if (User.IsInRole("Admin") || User.IsInRole("Finance"))
                        report = await _reportService.GenerateFinancialSummaryReport(startDate, endDate, periodType);
                    break;
                case "ExpiryRenewal":
                    if (User.IsInRole("Admin"))
                        report = await _reportService.GenerateExpiryRenewalReport(startDate, endDate, filter);
                    break;
                case "MarginAnalysis":
                    if (User.IsInRole("Admin") || User.IsInRole("Management"))
                        report = await _reportService.GenerateMarginAnalysisReport(startDate, endDate, filter);
                    break;
                default:
                    return NotFound();
            }

            if (report != null)
            {
                return RedirectToAction("ViewReport", new { id = report.ReportId });
            }
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Finance,Operations")]
        public IActionResult ViewReport(int id)
        {
            var report = _context.Reports.Find(id);
            if (report == null || report.GeneratedByRole != (User.IsInRole("Admin") ? "Admin" :
                                                           User.IsInRole("Finance") ? "Finance" :
                                                           User.IsInRole("Operations") ? "Operations" :
                                                           User.IsInRole("Management") ? "Management" : "Sales"))
                return Forbid();

            ViewBag.Report = report;
            return View();
        }

        [Authorize(Roles = "Admin,Finance,Operations")]
        public IActionResult ExportExcel(int id)
        {
            var report = _context.Reports.Find(id);
            if (report == null)
                return NotFound();
            var stream = _reportService.ExportToExcel(report);
            stream.Position = 0;
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Report_{report.ReportType}_{report.ReportId}.xlsx");
        }

        [Authorize(Roles = "Admin,Finance,Operations")]
        public IActionResult ExportPdf(int id)
        {
            var report = _context.Reports.Find(id);
            if (report == null)
                return NotFound();
            var stream = _reportService.ExportToPdf(report);
            stream.Position = 0;
            return File(stream, "application/pdf", $"Report_{report.ReportType}_{report.ReportId}.pdf");
        }

        // New report methods for enhanced functionality
        [HttpGet]
        [Authorize(Roles = "Admin,Finance,Management")]
        public async Task<IActionResult> MarginBreakup(DateTime? startDate = null, DateTime? endDate = null, string? oemFilter = null, string? clientFilter = null)
        {
            var query = _context.Deals.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.LicenseStartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(l => l.LicenseStartDate <= endDate.Value);
            if (!string.IsNullOrEmpty(oemFilter))
                query = query.Include(l => l.Oem).Where(l => l.Oem.OemName.Contains(oemFilter));
            if (!string.IsNullOrEmpty(clientFilter))
                query = query.Include(l => l.Company).Where(l => l.Company.CompanyName.Contains(clientFilter));

            var licenses = await query.ToListAsync();

            var marginBreakups = licenses.Select(l => new MarginBreakupViewModel
            {
                DealId = l.DealId,
                ProductName = l.ProductName ?? string.Empty,
                OemName = l.OemName ?? string.Empty,
                ClientName = l.ClientName ?? string.Empty,
                AmountReceived = l.AmountReceived ?? 0,
                AmountPaid = l.AmountPaid ?? 0,
                CalculatedMargin = (l.AmountReceived ?? 0) - (l.AmountPaid ?? 0),
                NegotiatedMargin = l.ManualMarginInput ?? 0,
                ActualMargin = l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0)),
                MarginType = l.ManualMarginInput.HasValue ? "Negotiated" : "Calculated",
                ProfitPercentage = (l.AmountReceived ?? 0) > 0 ? ((l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / (l.AmountReceived ?? 0)) * 100 : 0,
                DealDate = l.LicenseStartDate ?? DateTime.Now
            }).ToList();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.OemFilter = oemFilter;
            ViewBag.ClientFilter = clientFilter;

            return View(marginBreakups);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Finance,Management")]
        public async Task<IActionResult> ExportMarginBreakupExcel(DateTime? startDate = null, DateTime? endDate = null, string? oemFilter = null, string? clientFilter = null)
        {
            // Configure EPPlus license for this operation
            try
            {
                var query = _context.Deals.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(l => l.LicenseStartDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(l => l.LicenseStartDate <= endDate.Value);
                if (!string.IsNullOrEmpty(oemFilter))
                    query = query.Include(l => l.Oem).Where(l => l.Oem.OemName.Contains(oemFilter));
                if (!string.IsNullOrEmpty(clientFilter))
                    query = query.Include(l => l.Company).Where(l => l.Company.CompanyName.Contains(clientFilter));

                var licenses = await query.ToListAsync();

                var marginBreakups = licenses.Select(l => new MarginBreakupViewModel
                {
                    DealId = l.DealId,
                    ProductName = l.ProductName ?? string.Empty,
                    OemName = l.OemName ?? string.Empty,
                    ClientName = l.ClientName ?? string.Empty,
                    AmountReceived = l.AmountReceived ?? 0,
                    AmountPaid = l.AmountPaid ?? 0,
                    CalculatedMargin = (l.AmountReceived ?? 0) - (l.AmountPaid ?? 0),
                    NegotiatedMargin = l.ManualMarginInput ?? 0,
                    ActualMargin = l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0)),
                    MarginType = l.ManualMarginInput.HasValue ? "Negotiated" : "Calculated",
                    ProfitPercentage = (l.AmountReceived ?? 0) > 0 ? ((l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / (l.AmountReceived ?? 0)) * 100 : 0,
                    DealDate = l.LicenseStartDate ?? DateTime.Now
                }).ToList();

                // Create Excel package - EPPlus 8 compatible
#pragma warning disable CS0618 // Temporarily suppress obsolete warning
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Margin Breakup");

                // Headers
                worksheet.Cells[1, 1].Value = "License ID";
                worksheet.Cells[1, 2].Value = "Product Name";
                worksheet.Cells[1, 3].Value = "OEM Name";
                worksheet.Cells[1, 4].Value = "Client Name";
                worksheet.Cells[1, 5].Value = "Amount Received";
                worksheet.Cells[1, 6].Value = "Amount Paid";
                worksheet.Cells[1, 7].Value = "Calculated Margin";
                worksheet.Cells[1, 8].Value = "Negotiated Margin";
                worksheet.Cells[1, 9].Value = "Actual Margin";
                worksheet.Cells[1, 10].Value = "Margin Type";
                worksheet.Cells[1, 11].Value = "Profit Percentage";
                worksheet.Cells[1, 12].Value = "License Date";

                // Data
                for (int i = 0; i < marginBreakups.Count; i++)
                {
                    var item = marginBreakups[i];
                    var row = i + 2;

                    worksheet.Cells[row, 1].Value = item.DealId;
                    worksheet.Cells[row, 2].Value = item.ProductName;
                    worksheet.Cells[row, 3].Value = item.OemName;
                    worksheet.Cells[row, 4].Value = item.ClientName;
                    worksheet.Cells[row, 5].Value = item.AmountReceived;
                    worksheet.Cells[row, 6].Value = item.AmountPaid;
                    worksheet.Cells[row, 7].Value = item.CalculatedMargin;
                    worksheet.Cells[row, 8].Value = item.NegotiatedMargin;
                    worksheet.Cells[row, 9].Value = item.ActualMargin;
                    worksheet.Cells[row, 10].Value = item.MarginType;
                    worksheet.Cells[row, 11].Value = item.ProfitPercentage;
                    worksheet.Cells[row, 12].Value = item.DealDate.ToString("yyyy-MM-dd");
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Format header
                using (var range = worksheet.Cells[1, 1, 1, 12])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"MarginBreakup_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating Excel file: {ex.Message}";
                return RedirectToAction("MarginBreakup", new { startDate, endDate, oemFilter, clientFilter });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Finance,Management")]
        public async Task<IActionResult> CustomerProfitability(DateTime? startDate = null, DateTime? endDate = null, string? customerFilter = null)
        {
            var query = _context.Deals.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.LicenseStartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(l => l.LicenseStartDate <= endDate.Value);
            if (!string.IsNullOrEmpty(customerFilter))
                query = query.Include(l => l.Company).Where(l => l.Company.CompanyName.Contains(customerFilter));

            var licenses = await query.ToListAsync();

            var customerProfitability = licenses
                .GroupBy(l => l.ClientName)
                .Select(g => new CustomerProfitabilityViewModel
                {
                    CustomerName = g.Key,
                    TotalRevenue = g.Sum(l => l.AmountReceived ?? 0),
                    TotalCosts = g.Sum(l => l.AmountPaid ?? 0),
                    TotalProfit = g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))),
                    DealCount = g.Count(),
                    AvgRevenuePerDeal = g.Count() > 0 ? g.Sum(l => l.AmountReceived ?? 0) / g.Count() : 0,
                    ProfitMarginPercentage = g.Sum(l => l.AmountReceived ?? 0) > 0 ? (g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / g.Sum(l => l.AmountReceived ?? 0)) * 100 : 0
                })
                .OrderByDescending(c => c.TotalProfit)
                .ToList();

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.CustomerFilter = customerFilter;

            return View(customerProfitability);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Finance,Management")]
        public async Task<IActionResult> ExportCustomerProfitabilityExcel(DateTime? startDate = null, DateTime? endDate = null, string? customerFilter = null)
        {
            try
            {
                var query = _context.Deals.AsQueryable();

                if (startDate.HasValue)
                    query = query.Where(l => l.LicenseStartDate >= startDate.Value);
                if (endDate.HasValue)
                    query = query.Where(l => l.LicenseStartDate <= endDate.Value);
                if (!string.IsNullOrEmpty(customerFilter))
                    query = query.Include(l => l.Company).Where(l => l.Company.CompanyName.Contains(customerFilter));

                var licenses = await query.ToListAsync();

                var customerProfitability = licenses
                    .GroupBy(l => l.ClientName)
                    .Select(g => new CustomerProfitabilityViewModel
                    {
                        CustomerName = g.Key ?? "Unknown",
                        TotalRevenue = g.Sum(l => l.AmountReceived ?? 0),
                        TotalCosts = g.Sum(l => l.AmountPaid ?? 0),
                        TotalProfit = g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))),
                        DealCount = g.Count(),
                        AvgRevenuePerDeal = g.Count() > 0 ? g.Sum(l => l.AmountReceived ?? 0) / g.Count() : 0,
                        ProfitMarginPercentage = g.Sum(l => l.AmountReceived ?? 0) > 0 ? (g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / g.Sum(l => l.AmountReceived ?? 0)) * 100 : 0
                    })
                    .OrderByDescending(c => c.TotalProfit)
                    .ToList();

                // Create Excel package - EPPlus 8 compatible
#pragma warning disable CS0618 // Temporarily suppress obsolete warning
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
#pragma warning restore CS0618
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("Customer Profitability");

                // Headers
                worksheet.Cells[1, 1].Value = "Customer Name";
                worksheet.Cells[1, 2].Value = "Total Revenue";
                worksheet.Cells[1, 3].Value = "Total Costs";
                worksheet.Cells[1, 4].Value = "Total Profit";
                worksheet.Cells[1, 5].Value = "License Count";
                worksheet.Cells[1, 6].Value = "Avg Revenue Per License";
                worksheet.Cells[1, 7].Value = "Profit Margin %";

                // Data
                for (int i = 0; i < customerProfitability.Count; i++)
                {
                    var item = customerProfitability[i];
                    var row = i + 2;

                    worksheet.Cells[row, 1].Value = item.CustomerName;
                    worksheet.Cells[row, 2].Value = item.TotalRevenue;
                    worksheet.Cells[row, 3].Value = item.TotalCosts;
                    worksheet.Cells[row, 4].Value = item.TotalProfit;
                    worksheet.Cells[row, 5].Value = item.LicenseCount;
                    worksheet.Cells[row, 6].Value = item.AvgRevenuePerLicense;
                    worksheet.Cells[row, 7].Value = item.ProfitMarginPercentage;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Format header
                using (var range = worksheet.Cells[1, 1, 1, 7])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                var fileName = $"CustomerProfitability_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating Excel file: {ex.Message}";
                return RedirectToAction("CustomerProfitability", new { startDate, endDate, customerFilter });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Finance,Management")]
        public async Task<IActionResult> RevenueBreakdown(DateTime? startDate = null, DateTime? endDate = null, string groupBy = "OEM")
        {
            var query = _context.Deals.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.LicenseStartDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(l => l.LicenseStartDate <= endDate.Value);

            var licenses = await query.ToListAsync();

            IEnumerable<RevenueBreakdownViewModel> breakdown;

            if (groupBy == "Client")
            {
                breakdown = licenses
                    .GroupBy(l => l.ClientName)
                    .Select(g => new RevenueBreakdownViewModel
                    {
                        ClientName = g.Key ?? "Unknown",
                        TotalRevenue = g.Sum(l => l.AmountReceived ?? 0),
                        TotalCosts = g.Sum(l => l.AmountPaid ?? 0),
                        TotalMargin = g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))),
                        DealCount = g.Count(),
                        AvgMarginPerDeal = g.Count() > 0 ? g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / g.Count() : 0,
                        ProfitMarginPercentage = g.Sum(l => l.AmountReceived ?? 0) > 0 ? (g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / g.Sum(l => l.AmountReceived ?? 0)) * 100 : 0
                    });
            }
            else
            {
                breakdown = licenses
                    .GroupBy(l => l.OemName)
                    .Select(g => new RevenueBreakdownViewModel
                    {
                        OemName = g.Key ?? "Unknown",
                        TotalRevenue = g.Sum(l => l.AmountReceived ?? 0),
                        TotalCosts = g.Sum(l => l.AmountPaid ?? 0),
                        TotalMargin = g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))),
                        DealCount = g.Count(),
                        AvgMarginPerDeal = g.Count() > 0 ? g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / g.Count() : 0,
                        ProfitMarginPercentage = g.Sum(l => l.AmountReceived ?? 0) > 0 ? (g.Sum(l => l.ManualMarginInput ?? ((l.AmountReceived ?? 0) - (l.AmountPaid ?? 0))) / g.Sum(l => l.AmountReceived ?? 0)) * 100 : 0
                    });
            }

            ViewBag.StartDate = startDate;
            ViewBag.EndDate = endDate;
            ViewBag.GroupBy = groupBy;

            return View(breakdown.OrderByDescending(r => r.TotalMargin).ToList());
        }

        // Sales Report Action
        [Authorize(Roles = "Admin,Management,Sales")]
        public IActionResult SalesReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Redirect to existing CustomerProfitability report which is essentially a sales report
            return RedirectToAction("CustomerProfitability", new { startDate, endDate });
        }

        [Authorize(Roles = "Admin,Management,Finance,Sales")]
        public async Task<IActionResult> MonthlyRevenueAnalytics(string period = "last24months", string viewType = "chart")
        {
            var deals = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .Where(d => d.CustomerInvoiceAmount.HasValue && d.CustomerInvoiceAmount.Value > 0 &&
                           (d.DealStage == "Won" || d.CustomerPaymentStatus == "Paid" || d.CustomerPaymentStatus == "Pending") &&
                           (d.CustomerPaymentDate.HasValue || d.ActualCloseDate.HasValue || d.CustomerPoDate.HasValue))
                .ToListAsync();

            var currentDate = DateTime.Now;
            List<MonthlyRevenueDataPoint> revenueData = new List<MonthlyRevenueDataPoint>();

            // Helper function to get the appropriate revenue date for a deal
            Func<Deal, DateTime?> getRevenueDate = (deal) =>
            {
                if (deal.CustomerPaymentDate.HasValue)
                    return deal.CustomerPaymentDate.Value;
                if (deal.ActualCloseDate.HasValue)
                    return deal.ActualCloseDate.Value;
                if (deal.CustomerPoDate.HasValue)
                    return deal.CustomerPoDate.Value;
                return deal.CreatedDate; // Fallback to creation date
            };

            switch (period.ToLower())
            {
                case "last24months":
                    // Generate last 24 months of data
                    for (int i = 23; i >= 0; i--)
                    {
                        var monthDate = currentDate.AddMonths(-i);
                        var monthDeals = deals.Where(d =>
                        {
                            var revenueDate = getRevenueDate(d);
                            return revenueDate.HasValue &&
                                   revenueDate.Value.Year == monthDate.Year &&
                                   revenueDate.Value.Month == monthDate.Month;
                        }).ToList();

                        var monthRevenue = monthDeals.Sum(d => d.CustomerInvoiceAmount ?? 0);

                        revenueData.Add(new MonthlyRevenueDataPoint
                        {
                            Period = monthDate.ToString("MMM yyyy"),
                            Revenue = monthRevenue,
                            DealCount = monthDeals.Count,
                            Month = monthDate.Month,
                            Year = monthDate.Year
                        });
                    }
                    break;

                case "qoq": // Quarter over Quarter
                    var quarters = new[]
                    {
                        new { Name = "Q1", StartMonth = 1, Months = new[] { 1, 2, 3 } },
                        new { Name = "Q2", StartMonth = 4, Months = new[] { 4, 5, 6 } },
                        new { Name = "Q3", StartMonth = 7, Months = new[] { 7, 8, 9 } },
                        new { Name = "Q4", StartMonth = 10, Months = new[] { 10, 11, 12 } }
                    };

                    for (int yearOffset = 1; yearOffset >= 0; yearOffset--)
                    {
                        var targetYear = currentDate.Year - yearOffset;
                        foreach (var quarter in quarters)
                        {
                            var quarterDeals = deals.Where(d =>
                            {
                                var revenueDate = getRevenueDate(d);
                                return revenueDate.HasValue &&
                                       revenueDate.Value.Year == targetYear &&
                                       quarter.Months.Contains(revenueDate.Value.Month);
                            }).ToList();

                            var quarterRevenue = quarterDeals.Sum(d => d.CustomerInvoiceAmount ?? 0);

                            revenueData.Add(new MonthlyRevenueDataPoint
                            {
                                Period = $"{quarter.Name} {targetYear}",
                                Revenue = quarterRevenue,
                                DealCount = quarterDeals.Count,
                                Month = quarter.StartMonth,
                                Year = targetYear
                            });
                        }
                    }
                    break;

                case "yoy": // Year over Year
                    for (int i = 4; i >= 0; i--)
                    {
                        var targetYear = currentDate.Year - i;
                        var yearDeals = deals.Where(d =>
                        {
                            var revenueDate = getRevenueDate(d);
                            return revenueDate.HasValue && revenueDate.Value.Year == targetYear;
                        }).ToList();

                        var yearRevenue = yearDeals.Sum(d => d.CustomerInvoiceAmount ?? 0);

                        revenueData.Add(new MonthlyRevenueDataPoint
                        {
                            Period = targetYear.ToString(),
                            Revenue = yearRevenue,
                            DealCount = yearDeals.Count,
                            Month = 1,
                            Year = targetYear
                        });
                    }
                    break;
            }

            // Calculate growth percentages
            for (int i = 1; i < revenueData.Count; i++)
            {
                var current = revenueData[i];
                var previous = revenueData[i - 1];

                if (previous.Revenue > 0)
                {
                    current.GrowthPercentage = ((current.Revenue - previous.Revenue) / previous.Revenue) * 100;
                }
            }

            // Calculate summary statistics
            var totalRevenue = revenueData.Sum(r => r.Revenue);
            var averageRevenue = revenueData.Count > 0 ? revenueData.Average(r => r.Revenue) : 0;
            var maxRevenue = revenueData.Count > 0 ? revenueData.Max(r => r.Revenue) : 0;
            var totalDeals = revenueData.Sum(r => r.DealCount);

            var viewModel = new MonthlyRevenueAnalyticsViewModel
            {
                RevenueData = revenueData,
                SelectedPeriod = period,
                SelectedViewType = viewType,
                TotalRevenue = totalRevenue,
                AverageRevenue = averageRevenue,
                MaxRevenue = maxRevenue,
                TotalDeals = totalDeals,
                AvailablePeriods = new Dictionary<string, string>
                {
                    { "last24months", "Last 24 Months" },
                    { "qoq", "Quarter over Quarter" },
                    { "yoy", "Year over Year" }
                },
                AvailableViewTypes = new Dictionary<string, string>
                {
                    { "chart", "Chart View" },
                    { "table", "Table View" },
                    { "both", "Chart & Table" }
                }
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin,Management,Finance,Sales")]
        public async Task<IActionResult> ExportRevenueAnalytics(string period = "last24months")
        {
            try
            {
                var deals = await _context.Deals
                    .Include(d => d.Company)
                    .Include(d => d.Product)
                    .Include(d => d.Oem)
                    .Where(d => d.CustomerInvoiceAmount.HasValue && d.CustomerInvoiceAmount.Value > 0 &&
                               (d.DealStage == "Won" || d.CustomerPaymentStatus == "Paid" || d.CustomerPaymentStatus == "Pending") &&
                               (d.CustomerPaymentDate.HasValue || d.ActualCloseDate.HasValue || d.CustomerPoDate.HasValue))
                    .ToListAsync();

                var currentDate = DateTime.Now;
                List<MonthlyRevenueDataPoint> revenueData = new List<MonthlyRevenueDataPoint>();

                // Helper function to get the appropriate revenue date for a deal
                Func<Deal, DateTime?> getRevenueDate = (deal) =>
                {
                    if (deal.CustomerPaymentDate.HasValue)
                        return deal.CustomerPaymentDate.Value;
                    if (deal.ActualCloseDate.HasValue)
                        return deal.ActualCloseDate.Value;
                    if (deal.CustomerPoDate.HasValue)
                        return deal.CustomerPoDate.Value;
                    return deal.CreatedDate; // Fallback to creation date
                };

                // Generate revenue data based on period (reuse logic from MonthlyRevenueAnalytics)
                switch (period.ToLower())
                {
                    case "last24months":
                        for (int i = 23; i >= 0; i--)
                        {
                            var monthDate = currentDate.AddMonths(-i);
                            var monthDeals = deals.Where(d =>
                            {
                                var revenueDate = getRevenueDate(d);
                                return revenueDate.HasValue &&
                                       revenueDate.Value.Year == monthDate.Year &&
                                       revenueDate.Value.Month == monthDate.Month;
                            }).ToList();

                            var monthRevenue = monthDeals.Sum(d => d.CustomerInvoiceAmount ?? 0);

                            revenueData.Add(new MonthlyRevenueDataPoint
                            {
                                Period = monthDate.ToString("MMM yyyy"),
                                Revenue = monthRevenue,
                                DealCount = monthDeals.Count,
                                Month = monthDate.Month,
                                Year = monthDate.Year
                            });
                        }
                        break;
                }

                // Calculate growth percentages
                for (int i = 1; i < revenueData.Count; i++)
                {
                    var current = revenueData[i];
                    var previous = revenueData[i - 1];

                    if (previous.Revenue > 0)
                    {
                        current.GrowthPercentage = ((current.Revenue - previous.Revenue) / previous.Revenue) * 100;
                    }
                }

                // Generate Excel file
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Monthly Revenue Analytics");

                    // Headers
                    worksheet.Cells[1, 1].Value = "Period";
                    worksheet.Cells[1, 2].Value = "Revenue";
                    worksheet.Cells[1, 3].Value = "Deal Count";
                    worksheet.Cells[1, 4].Value = "Growth %";
                    worksheet.Cells[1, 5].Value = "Avg per Deal";

                    // Header styling
                    var headerRange = worksheet.Cells[1, 1, 1, 5];
                    headerRange.Style.Font.Bold = true;

                    // Data
                    for (int i = 0; i < revenueData.Count; i++)
                    {
                        var row = i + 2;
                        var data = revenueData[i];

                        worksheet.Cells[row, 1].Value = data.Period;
                        worksheet.Cells[row, 2].Value = data.Revenue;
                        worksheet.Cells[row, 3].Value = data.DealCount;
                        worksheet.Cells[row, 4].Value = data.GrowthPercentage;
                        worksheet.Cells[row, 5].Value = data.DealCount > 0 ? data.Revenue / data.DealCount : 0;
                    }

                    // Format currency columns
                    worksheet.Cells[2, 2, revenueData.Count + 1, 2].Style.Numberformat.Format = "$#,##0";
                    worksheet.Cells[2, 5, revenueData.Count + 1, 5].Style.Numberformat.Format = "$#,##0";
                    worksheet.Cells[2, 4, revenueData.Count + 1, 4].Style.Numberformat.Format = "0.0%";

                    // Auto-fit columns
                    worksheet.Cells.AutoFitColumns();

                    var fileName = $"MonthlyRevenueAnalytics_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                    return File(package.GetAsByteArray(), contentType, fileName);
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error generating export: {ex.Message}";
                return RedirectToAction("MonthlyRevenueAnalytics", new { period });
            }
        }

        // Financial Report Action  
        [Authorize(Roles = "Admin,Management,Finance")]
        public IActionResult FinancialReport(DateTime? startDate = null, DateTime? endDate = null)
        {
            // Redirect to existing MarginBreakup report which is essentially a financial report
            return RedirectToAction("MarginBreakup", new { startDate, endDate });
        }
    }
}
