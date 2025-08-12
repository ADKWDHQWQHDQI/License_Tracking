using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using License_Tracking.Services;
using License_Tracking.Models;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IAlertService _alertService;
        private readonly AppDbContext _context;

        public NotificationController(IAlertService alertService, AppDbContext context)
        {
            _alertService = alertService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Redirect to Dashboard for better UX
            return RedirectToAction("Dashboard");
        }

        [HttpGet]
        public async Task<IActionResult> GetPendingCount()
        {
            var count = await _alertService.GetPendingAlertCountAsync();
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificationStats()
        {
            var pendingAlerts = await _context.Alerts
                .Where(a => a.Status == "Pending")
                .ToListAsync();

            var stats = new
            {
                Total = pendingAlerts.Count,
                Critical = pendingAlerts.Count(a => a.Priority == AlertPriority.Critical),
                High = pendingAlerts.Count(a => a.Priority == AlertPriority.High),
                Medium = pendingAlerts.Count(a => a.Priority == AlertPriority.Medium),
                Low = pendingAlerts.Count(a => a.Priority == AlertPriority.Low),
                Renewals = pendingAlerts.Count(a => a.AlertType == AlertType.Renewal),
                Pipeline = pendingAlerts.Count(a => a.AlertType == AlertType.PipelineReminder),
                Overdue = pendingAlerts.Count(a => a.IsOverdue)
            };

            return Json(stats);
        }

        [HttpGet]
        public async Task<IActionResult> GetRecentAlerts(int take = 5)
        {
            try
            {
                var alerts = await _context.Alerts
                    .Include(a => a.Deal)
                        .ThenInclude(d => d != null ? d.Product : null)
                    .Include(a => a.Deal)
                        .ThenInclude(d => d != null ? d.Company : null)
                    .Include(a => a.ProjectPipeline)
                    .Where(a => a.Status == "Pending" || a.Status == "Sent")
                    .OrderByDescending(a => a.Priority)
                    .ThenByDescending(a => a.CreatedDate)
                    .Take(take)
                    .Select(a => new
                    {
                        a.AlertId,
                        a.Title,
                        a.AlertMessage,
                        a.Priority,
                        a.AlertType,
                        a.CreatedDate,
                        a.IsOverdue,
                        a.Status,
                        a.AlertDate,
                        // Safe navigation for Deal-related properties
                        LicenseName = a.Deal != null && a.Deal.Product != null ? a.Deal.Product.ProductName : null,
                        ProjectName = a.ProjectPipeline != null ? a.ProjectPipeline.ProductName : null,
                        ClientName = a.Deal != null && a.Deal.Company != null ? a.Deal.Company.CompanyName :
                                    a.ProjectPipeline != null ? a.ProjectPipeline.ClientName : null,
                        DealId = a.Deal != null ? a.Deal.DealId : (int?)null,
                        ProjectPipelineId = a.ProjectPipeline != null ? a.ProjectPipeline.ProjectPipelineId : (int?)null
                    })
                    .ToListAsync();

                return Json(alerts);
            }
            catch (Exception ex)
            {
                // Log the error and return an empty result
                return Json(new { error = ex.Message, alerts = new object[0] });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DismissAlert(int alertId)
        {
            try
            {
                await _alertService.DismissAlertAsync(alertId);
                return Json(new { success = true, message = "Alert dismissed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error dismissing alert: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int alertId)
        {
            try
            {
                await _alertService.MarkAlertAsSentAsync(alertId);
                return Json(new { success = true, message = "Alert marked as read" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error marking alert as read: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var pendingAlerts = await _context.Alerts
                    .Include(a => a.Deal)
                    .Include(a => a.ProjectPipeline)
                    .Where(a => a.Status == "Pending")
                    .OrderByDescending(a => a.Priority)
                    .ThenBy(a => a.AlertDate)
                    .ToListAsync();

                var alertStats = new
                {
                    TotalPending = pendingAlerts.Count,
                    Critical = pendingAlerts.Count(a => a.Priority == AlertPriority.Critical),
                    High = pendingAlerts.Count(a => a.Priority == AlertPriority.High),
                    Medium = pendingAlerts.Count(a => a.Priority == AlertPriority.Medium),
                    Low = pendingAlerts.Count(a => a.Priority == AlertPriority.Low),
                    Overdue = pendingAlerts.Count(a => a.IsOverdue),
                    RenewalAlerts = pendingAlerts.Count(a => a.AlertType == AlertType.Renewal),
                    PipelineAlerts = pendingAlerts.Count(a => a.AlertType == AlertType.PipelineReminder)
                };

                ViewBag.AlertStats = alertStats;
                return View(pendingAlerts);
            }
            catch (Exception)
            {
                // Log error and return empty view
                return View(new List<Alert>());
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Operations")]
        public async Task<IActionResult> ProcessEmailAlerts()
        {
            try
            {
                await _alertService.ProcessPendingEmailAlertsAsync();
                return Json(new { success = true, message = "Email alerts processed successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error processing email alerts: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GenerateRenewalAlerts()
        {
            try
            {
                await _alertService.GenerateRenewalAlertsAsync();
                return Json(new { success = true, message = "Renewal alerts generated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating renewal alerts: {ex.Message}" });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GeneratePaymentAlerts()
        {
            try
            {
                await _alertService.GeneratePaymentAlertsAsync();
                return Json(new { success = true, message = "Payment alerts generated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error generating payment alerts: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetLicenseExpiryData()
        {
            try
            {
                var today = DateTime.Today;
                var licenses = await _context.Deals
                    .Include(l => l.Product)
                    .Include(l => l.Company)
                    .Where(l => l.LicenseDeliveryStatus == "Active" && l.IsProjectPipeline != true)
                    .Select(l => new
                    {
                        l.DealId,
                        ProductName = l.Product != null ? l.Product.ProductName : "Unknown Product",
                        ClientName = l.Company != null ? l.Company.CompanyName : "Unknown Client",
                        ExpiryDate = l.LicenseEndDate ?? l.LicenseStartDate ?? DateTime.Now.AddYears(1),
                        AlertDaysBefore = 45, // Default alert days
                        DaysUntilExpiry = (l.LicenseEndDate ?? l.LicenseStartDate ?? DateTime.Now.AddYears(1)).Subtract(today).Days,
                        l.CustomerInvoiceAmount,
                        l.OemQuoteAmount,
                        l.CustomerPaymentStatus,
                        OutstandingAmount = (l.CustomerInvoiceAmount ?? 0) - (l.AmountReceived ?? 0)
                    })
                    .ToListAsync();

                var data = new
                {
                    TotalLicenses = licenses.Count,
                    ExpiringIn7Days = licenses.Count(l => l.DaysUntilExpiry <= 7 && l.DaysUntilExpiry >= 0),
                    ExpiringIn30Days = licenses.Count(l => l.DaysUntilExpiry <= 30 && l.DaysUntilExpiry >= 0),
                    ExpiringIn45Days = licenses.Count(l => l.DaysUntilExpiry <= 45 && l.DaysUntilExpiry >= 0),
                    ExpiredLicenses = licenses.Count(l => l.DaysUntilExpiry < 0),
                    PendingPayments = licenses.Count(l => l.CustomerPaymentStatus == "Pending" && l.OutstandingAmount > 0),
                    TotalOutstanding = licenses.Where(l => l.CustomerPaymentStatus == "Pending").Sum(l => l.OutstandingAmount),
                    Licenses = licenses.OrderBy(l => l.DaysUntilExpiry).Take(10) // Top 10 expiring soon
                };

                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error retrieving license data: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAlertsByType(string alertType)
        {
            if (!Enum.TryParse<AlertType>(alertType, out var type))
            {
                return BadRequest("Invalid alert type");
            }

            var alerts = await _context.Alerts
                .Include(a => a.License)
                .Include(a => a.ProjectPipeline)
                .Where(a => a.AlertType == type && (a.Status == "Pending" || a.Status == "Sent"))
                .OrderByDescending(a => a.Priority)
                .ThenBy(a => a.AlertDate)
                .ToListAsync();

            return PartialView("_AlertsList", alerts);
        }

        [HttpGet]
        public async Task<IActionResult> GetAlertsByPriority(string priority)
        {
            if (!Enum.TryParse<AlertPriority>(priority, out var priorityLevel))
            {
                return BadRequest("Invalid priority level");
            }

            var alerts = await _context.Alerts
                .Include(a => a.License)
                .Include(a => a.ProjectPipeline)
                .Where(a => a.Priority == priorityLevel && (a.Status == "Pending" || a.Status == "Sent"))
                .OrderBy(a => a.AlertDate)
                .ToListAsync();

            return PartialView("_AlertsList", alerts);
        }
    }
}
