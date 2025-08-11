using System.Diagnostics;
using License_Tracking.Models;
using Microsoft.AspNetCore.Mvc;
using License_Tracking.Data;
using License_Tracking.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace License_Tracking.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;
        private readonly IAlertService _alertService;

        public HomeController(ILogger<HomeController> logger, AppDbContext context, IAlertService alertService)
        {
            _logger = logger;
            _context = context;
            _alertService = alertService;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return View();
            }
            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateDemoNotifications()
        {
            try
            {
                // Generate renewal and payment alerts from existing license data
                await _alertService.GenerateRenewalAlertsAsync();
                await _alertService.GeneratePaymentAlertsAsync();

                TempData["SuccessMessage"] = "Notification alerts have been generated based on existing license data. The system will automatically monitor licenses and create alerts as needed.";
                return RedirectToAction("Dashboard", "Notification");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating notifications");
                TempData["ErrorMessage"] = "Error generating notifications: " + ex.Message;
                return RedirectToAction("Index");
            }
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
