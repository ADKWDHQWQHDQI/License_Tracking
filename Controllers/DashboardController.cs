using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.ViewModels;
using License_Tracking.Models;
using License_Tracking.Services;
using System.Threading.Tasks;
using System.Linq;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IAlertService _alertService;
        private readonly IProjectPipelineService _projectPipelineService;

        public DashboardController(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            IAlertService alertService,
            IProjectPipelineService projectPipelineService)
        {
            _context = context;
            _userManager = userManager;
            _alertService = alertService;
            _projectPipelineService = projectPipelineService;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var userRoles = await _userManager.GetRolesAsync(currentUser);

            // Get all deals data
            var allDeals = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .Include(d => d.Company)
                .Where(d => d.Product != null && d.Oem != null && d.Company != null)
                .ToListAsync();

            // Get companies data
            var totalCompanies = await _context.Companies.CountAsync();
            var activeCustomers = await _context.Companies
                .Where(c => _context.Deals.Any(d => d.CompanyId == c.CompanyId))
                .CountAsync();

            // Get pipeline projects data
            var allPipelineProjects = await _context.ProjectPipelines.ToListAsync();
            var activePipelineProjects = allPipelineProjects.Where(p => p.ProjectStatus != "Lost" && p.ConvertedToLicenseId == null).ToList();

            // Get invoice data
            var pendingInvoices = await _context.CbmsInvoices
                .CountAsync(i => i.PaymentStatus != "Paid");

            // Get recent activities
            var recentActivities = await _context.Activities
                .OrderByDescending(a => a.CreatedDate)
                .Take(6)
                .ToListAsync();

            // Calculate deal stage distribution
            var dealsByStage = allDeals
                .GroupBy(d => d.DealStage ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            // Calculate current month revenue (based on actual payments received this month)
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            // Calculate monthly revenue from deals with customer invoice amounts (including pending payments)
            var currentMonthRevenue = allDeals
                .Where(d => d.CustomerInvoiceAmount.HasValue && d.CustomerInvoiceAmount.Value > 0 &&
                           (d.CustomerPaymentDate.HasValue && d.CustomerPaymentDate.Value.Month == currentMonth && d.CustomerPaymentDate.Value.Year == currentYear) ||
                           (d.ActualCloseDate.HasValue && d.ActualCloseDate.Value.Month == currentMonth && d.ActualCloseDate.Value.Year == currentYear) ||
                           (d.CustomerPoDate.HasValue && d.CustomerPoDate.Value.Month == currentMonth && d.CustomerPoDate.Value.Year == currentYear))
                .Sum(d => d.CustomerInvoiceAmount ?? 0);

            // Calculate last month revenue for comparison
            var lastMonthDate = DateTime.Now.AddMonths(-1);
            var lastMonthRevenue = allDeals
                .Where(d => d.CustomerInvoiceAmount.HasValue && d.CustomerInvoiceAmount.Value > 0 &&
                           (d.CustomerPaymentDate.HasValue && d.CustomerPaymentDate.Value.Month == lastMonthDate.Month && d.CustomerPaymentDate.Value.Year == lastMonthDate.Year) ||
                           (d.ActualCloseDate.HasValue && d.ActualCloseDate.Value.Month == lastMonthDate.Month && d.ActualCloseDate.Value.Year == lastMonthDate.Year) ||
                           (d.CustomerPoDate.HasValue && d.CustomerPoDate.Value.Month == lastMonthDate.Month && d.CustomerPoDate.Value.Year == lastMonthDate.Year))
                .Sum(d => d.CustomerInvoiceAmount ?? 0);

            // Calculate month-over-month growth percentage
            var monthlyGrowthPercentage = lastMonthRevenue > 0 ?
                ((currentMonthRevenue - lastMonthRevenue) / lastMonthRevenue) * 100 : 0;

            var dashboardData = new DashboardViewModel
            {
                UserEmail = currentUser.Email ?? "Unknown",
                UserRoles = userRoles.ToList(),

                // New dashboard metrics
                TotalCompanies = totalCompanies,
                TotalDeals = allDeals.Count,
                TotalActiveDeals = allDeals.Count(d => d.DealStage == "Lead" || d.DealStage == "Quoted" || d.DealStage == "Negotiation"),
                ActiveCustomers = activeCustomers,
                PendingInvoices = pendingInvoices,

                // Monthly Revenue with accurate calculation
                MonthlyRevenue = currentMonthRevenue,
                MonthlyGrowthPercentage = monthlyGrowthPercentage,

                // Legacy metrics for backward compatibility
                TotalLicenses = allDeals.Count,
                ActiveLicenses = allDeals.Count(d => d.LicenseDeliveryStatus == "Delivered" || d.LicenseDeliveryStatus == "Activated"),
                ExpiringLicenses = allDeals.Count(d => d.LicenseEndDate <= DateTime.Now.AddDays(30) && (d.LicenseDeliveryStatus == "Delivered" || d.LicenseDeliveryStatus == "Activated")),
                TotalRevenue = allDeals.Sum(d => d.AmountReceived ?? 0),
                TotalMargin = allDeals.Sum(d => d.Margin),

                // Deal stage distribution
                DealsByStage = dealsByStage,

                // Recent activities
                RecentActivities = recentActivities,

                // Pipeline data
                TotalPipelineProjects = allPipelineProjects.Count,
                ActivePipelineProjects = activePipelineProjects.Count,
                ProjectedPipelineRevenue = activePipelineProjects.Sum(p => p.ExpectedAmountToReceive * (p.SuccessProbability / 100.0m)),
                ProjectedPipelineMargin = activePipelineProjects.Sum(p => (p.ExpectedAmountToReceive - p.ExpectedAmountToPay) * (p.SuccessProbability / 100.0m))
            };

            if (User.IsInRole("Admin"))
            {
                dashboardData.PendingPayments = allDeals.Count(d => d.PaymentStatus != "Completed");
                dashboardData.TotalUsers = await _userManager.Users.CountAsync();
            }

            return View(dashboardData);
        }

        [Authorize(Roles = "Admin,Finance,Management")]
        public async Task<IActionResult> Financial()
        {
            var licenses = await _context.Deals.ToListAsync();
            var financialData = licenses
                .GroupBy(l => l.PaymentStatus)
                .Select(g => new
                {
                    Status = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(l => l.AmountReceived),
                    TotalMargin = g.Sum(l => l.Margin)
                })
                .ToList();

            return View(financialData);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserManagementViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userViewModels.Add(new UserManagementViewModel
                {
                    Id = user.Id,
                    Email = user.Email ?? "Unknown",
                    UserName = user.UserName ?? "Unknown",
                    Roles = roles.ToList()
                });
            }

            return View(userViewModels);
        }

        [Authorize(Roles = "Admin,Operations,Management")]
        public async Task<IActionResult> Operations()
        {
            var today = DateTime.Now;
            var operationsData = new OperationsViewModel
            {
                TotalLicenses = await _context.Deals.CountAsync(),
                ActiveLicenses = await _context.Deals.CountAsync(l => l.LicenseDeliveryStatus == "Delivered" || l.LicenseDeliveryStatus == "Activated"),
                ExpiredLicenses = await _context.Deals.CountAsync(l => l.LicenseEndDate < today),
                ExpiringIn30Days = await _context.Deals.CountAsync(l => l.LicenseEndDate <= today.AddDays(30) && l.LicenseEndDate > today && l.LicenseDeliveryStatus == "Active"),
                ExpiringIn60Days = await _context.Deals.CountAsync(l => l.LicenseEndDate <= today.AddDays(60) && l.LicenseEndDate > today.AddDays(30) && l.LicenseDeliveryStatus == "Active"),
                ExpiringIn90Days = await _context.Deals.CountAsync(l => l.LicenseEndDate <= today.AddDays(90) && l.LicenseEndDate > today.AddDays(60) && l.LicenseDeliveryStatus == "Active")
            };

            return View(operationsData);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id == id)
            {
                // Prevent self-deletion
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Users));
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }
            return RedirectToAction(nameof(Users));
        }

        // Analytics section removed - moved to Phase 6 implementation

        // API endpoints for AJAX calls from dashboard
        [HttpGet]
        public async Task<JsonResult> GetAlertSummaryData()
        {
            try
            {
                var pendingCount = await _alertService.GetPendingAlertCountAsync();
                var alerts = await _alertService.GetPendingAlertsAsync();
                var criticalCount = alerts.Count(a => a.Priority == Models.AlertPriority.Critical);

                return Json(new
                {
                    pendingAlerts = pendingCount,
                    criticalAlerts = criticalCount
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    error = "Failed to load alert summary data",
                    details = ex.Message,
                    innerException = ex.InnerException?.Message
                });
            }
        }

        // Testing endpoints for Phase 2 validation
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> TestAlertService()
        {
            try
            {
                await _alertService.GenerateRenewalAlertsAsync();
                var alertCount = await _alertService.GetPendingAlertCountAsync();
                return Json(new { success = true, alertCount = alertCount, message = "Alert service working correctly" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}
