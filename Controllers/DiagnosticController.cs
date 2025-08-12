using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Services;
using License_Tracking.Models;

namespace License_Tracking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DiagnosticController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAlertService _alertService;

        public DiagnosticController(
            AppDbContext context,
            IAlertService alertService)
        {
            _context = context;
            _alertService = alertService;
        }

        [HttpGet]
        public async Task<JsonResult> TestDatabaseTables()
        {
            var results = new Dictionary<string, object>();

            try
            {
                // Test basic tables
                results["Licenses"] = new
                {
                    exists = true,
                    count = await _context.Deals.CountAsync(),
                    sample = await _context.Deals
                        .Include(l => l.Product)
                        .Include(l => l.Oem)
                        .Include(l => l.Company)
                        .Take(1)
                        .Select(l => new
                        {
                            ProductName = l.Product != null ? l.Product.ProductName : null,
                            OemName = l.Oem != null ? l.Oem.OemName : null,
                            ClientName = l.Company != null ? l.Company.CompanyName : null
                        })
                        .FirstOrDefaultAsync()
                };
            }
            catch (Exception ex)
            {
                results["Licenses"] = new { exists = false, error = ex.Message };
            }

            try
            {
                results["ProjectPipelines"] = new
                {
                    exists = true,
                    count = await _context.ProjectPipelines.CountAsync()
                };
            }
            catch (Exception ex)
            {
                results["ProjectPipelines"] = new { exists = false, error = ex.Message };
            }
            try
            {
                results["Alerts"] = new
                {
                    exists = true,
                    count = await _context.Alerts.CountAsync()
                };
            }
            catch (Exception ex)
            {
                results["Alerts"] = new { exists = false, error = ex.Message };
            }

            return Json(results);
        }

        [HttpGet]
        public async Task<JsonResult> TestLicenseDataForAnalytics()
        {
            var results = new Dictionary<string, object>();

            try
            {
                // Get all licenses with relevant fields
                var allLicenses = await _context.Deals
                    .Include(l => l.Product)
                    .Include(l => l.Oem)
                    .Include(l => l.Company)
                    .Select(l => new
                    {
                        l.DealId,
                        ProductName = l.Product != null ? l.Product.ProductName : null,
                        OemName = l.Oem != null ? l.Oem.OemName : null,
                        ClientName = l.Company != null ? l.Company.CompanyName : null,
                        LicenseDate = l.LicenseStartDate ?? l.CreatedDate,
                        ExpiryDate = l.LicenseEndDate ?? l.LicenseStartDate,
                        l.AmountReceived,
                        l.AmountPaid,
                        Margin = l.AmountReceived - l.AmountPaid
                    })
                    .OrderBy(l => l.LicenseDate)
                    .ToListAsync();

                results["TotalLicenses"] = allLicenses.Count;
                results["AllLicenseData"] = allLicenses;

                // Monthly breakdown for the last 12 months
                var startDate = DateTime.Now.AddMonths(-12).Date;
                var endDate = DateTime.Now.Date;

                var monthlyBreakdown = allLicenses
                    .Where(l => l.LicenseDate >= startDate && l.LicenseDate <= endDate)
                    .GroupBy(l => new { l.LicenseDate.Year, l.LicenseDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        MonthName = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy"),
                        Count = g.Count(),
                        TotalRevenue = g.Sum(l => l.AmountReceived ?? 0),
                        TotalCosts = g.Sum(l => l.AmountPaid ?? 0),
                        TotalMargin = g.Sum(l => l.Margin ?? 0),
                        Licenses = g.Select(l => new { l.ProductName, l.AmountReceived, l.AmountPaid, l.LicenseDate })
                    })
                    .OrderBy(m => m.Year).ThenBy(m => m.Month)
                    .ToList();

                results["MonthlyBreakdown"] = monthlyBreakdown;
                results["MonthlyBreakdownCount"] = monthlyBreakdown.Count;

                // Date range analysis
                results["DateRangeUsed"] = new { startDate, endDate };
                results["LicensesInDateRange"] = allLicenses.Count(l => l.LicenseDate >= startDate && l.LicenseDate <= endDate);

            }
            catch (Exception ex)
            {
                results["Error"] = ex.Message;
                results["StackTrace"] = ex.StackTrace ?? "No stack trace available";
            }

            return Json(results);
        }

        [HttpGet]
        public async Task<JsonResult> CreateSampleLicenseData()
        {
            var results = new Dictionary<string, object>();

            try
            {
                // Check current license count
                var currentCount = await _context.Deals.CountAsync();
                results["ExistingLicenseCount"] = currentCount;

                if (currentCount < 5) // Only add sample data if we have very few licenses
                {
                    var sampleDeals = new List<Deal>();
                    var random = new Random();

                    // Create deals for the past 6 months to provide better analytics data
                    for (int monthsBack = 6; monthsBack >= 0; monthsBack--)
                    {
                        var dealDate = DateTime.Now.AddMonths(-monthsBack).Date;
                        var expiryDate = dealDate.AddYears(1);

                        var deal = new Deal
                        {
                            DealName = $"Sample Deal {monthsBack + 1}",
                            CompanyId = 1, // Assuming there's at least one company
                            OemId = 1, // Assuming there's at least one OEM
                            ProductId = 1, // Assuming there's at least one product
                            LicenseStartDate = dealDate,
                            LicenseEndDate = expiryDate,
                            CustomerPoNumber = $"PO-{1000 + monthsBack}",
                            CustomerInvoiceAmount = random.Next(50000, 100000),
                            OemQuoteAmount = random.Next(20000, 40000),
                            CustomerInvoiceNumber = $"INV-{3000 + monthsBack}",
                            CustomerPaymentStatus = "Paid",
                            OemPaymentStatus = "Paid",
                            Quantity = 1,
                            DealStage = "Won",
                            Notes = $"Sample deal created for month {dealDate:MMM yyyy}"
                        };

                        sampleDeals.Add(deal);
                    }

                    _context.Deals.AddRange(sampleDeals);
                    await _context.SaveChangesAsync();

                    results["SampleLicensesCreated"] = sampleDeals.Count;
                    results["SampleLicenseData"] = sampleDeals.Select(l => new
                    {
                        DealName = l.DealName,
                        LicenseDate = l.LicenseStartDate,
                        AmountReceived = l.CustomerInvoiceAmount,
                        AmountPaid = l.OemQuoteAmount,
                        Margin = l.CustomerInvoiceAmount - l.OemQuoteAmount
                    });
                }
                else
                {
                    results["Message"] = "Sample data not needed - sufficient licenses already exist";
                }

                // Show updated count
                results["TotalLicenseCount"] = await _context.Deals.CountAsync();
            }
            catch (Exception ex)
            {
                results["Error"] = ex.Message;
                results["StackTrace"] = ex.StackTrace ?? "No stack trace available";
            }

            return Json(results);
        }

        [HttpGet]
        public async Task<JsonResult> TestServices()
        {
            var results = new Dictionary<string, object>();

            // Analytics Service removed - moved to Phase 6 implementation

            // Test Alert Service
            try
            {
                var alertCount = await _alertService.GetPendingAlertCountAsync();
                results["AlertService"] = new
                {
                    working = true,
                    pendingAlerts = alertCount
                };
            }
            catch (Exception ex)
            {
                results["AlertService"] = new { working = false, error = ex.Message, stackTrace = ex.StackTrace };
            }

            return Json(results);
        }

        [HttpGet]
        public async Task<JsonResult> TestBasicQueries()
        {
            var results = new Dictionary<string, object>();

            try
            {
                // Test basic license query
                var licenseData = await _context.Deals
                    .Where(l => l.LicenseDeliveryStatus == "Active" && l.IsProjectPipeline != true)
                    .Select(l => new
                    {
                        l.ProductName,
                        l.OemName,
                        l.ClientName,
                        l.AmountPaid,
                        l.AmountReceived,
                        Margin = l.AmountReceived - l.AmountPaid
                    })
                    .Take(5)
                    .ToListAsync();

                results["BasicLicenseQuery"] = new
                {
                    working = true,
                    count = licenseData.Count,
                    data = licenseData
                };
            }
            catch (Exception ex)
            {
                results["BasicLicenseQuery"] = new { working = false, error = ex.Message };
            }

            try
            {
                // Test OEM grouping
                var oemData = await _context.Deals
                    .Where(l => l.LicenseDeliveryStatus == "Active" && l.IsProjectPipeline != true)
                    .GroupBy(l => l.OemName)
                    .Select(g => new
                    {
                        OemName = g.Key,
                        Count = g.Count(),
                        TotalCost = g.Sum(l => l.AmountPaid)
                    })
                    .Take(3)
                    .ToListAsync();

                results["OemGrouping"] = new
                {
                    working = true,
                    count = oemData.Count,
                    data = oemData
                };
            }
            catch (Exception ex)
            {
                results["OemGrouping"] = new { working = false, error = ex.Message };
            }

            return Json(results);
        }
    }
}
