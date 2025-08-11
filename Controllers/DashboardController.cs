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

            // Get all licenses, handling potential null values
            var allLicenses = await _context.Deals
                .Include(l => l.Product)
                .Include(l => l.Oem)
                .Include(l => l.Company)
                .Where(l => l.Product != null && l.Oem != null && l.Company != null)
                .ToListAsync();

            // Get pipeline projects data
            var allPipelineProjects = await _context.ProjectPipelines.ToListAsync();
            var activePipelineProjects = allPipelineProjects.Where(p => p.ProjectStatus != "Lost" && p.ConvertedToLicenseId == null).ToList();

            var dashboardData = new DashboardViewModel
            {
                UserEmail = currentUser.Email ?? "Unknown",
                UserRoles = userRoles.ToList(),
                TotalLicenses = allLicenses.Count,
                ActiveLicenses = allLicenses.Count(l => l.LicenseStatus == "Active"),
                ExpiringLicenses = allLicenses.Count(l => l.ExpiryDate <= DateTime.Now.AddDays(30) && l.LicenseStatus == "Active"),
                TotalRevenue = allLicenses.Sum(l => l.AmountReceived ?? 0),
                TotalMargin = allLicenses.Sum(l => l.Margin), // Ensure Margin is computed correctly
                // Pipeline data
                TotalPipelineProjects = allPipelineProjects.Count,
                ActivePipelineProjects = activePipelineProjects.Count,
                ProjectedPipelineRevenue = activePipelineProjects.Sum(p => p.ExpectedAmountToReceive * (p.SuccessProbability / 100.0m)),
                ProjectedPipelineMargin = activePipelineProjects.Sum(p => (p.ExpectedAmountToReceive - p.ExpectedAmountToPay) * (p.SuccessProbability / 100.0m))
            };

            if (User.IsInRole("Admin"))
            {
                dashboardData.PendingPayments = allLicenses.Count(l => l.PaymentStatus != "Completed");
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
                ActiveLicenses = await _context.Deals.CountAsync(l => l.LicenseDeliveryStatus == "Active"),
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
