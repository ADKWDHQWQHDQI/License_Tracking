using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using License_Tracking.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace License_Tracking.Controllers
{
    [Authorize(Roles = "Admin,Operations,Management")]
    public class RenewalController : Controller
    {
        private readonly AppDbContext _context;

        public RenewalController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var renewalsList = new List<RenewalViewModel>();

            // Get regular license renewals with proper navigation properties
            var renewals = await _context.Renewals
                .Include(r => r.Deal)
                .ThenInclude(d => d!.Product)
                .Include(r => r.Deal)
                .ThenInclude(d => d!.Company)
                .Where(r => r.Deal != null) // Ensure deal exists
                .ToListAsync();

            foreach (var r in renewals)
            {
                renewalsList.Add(new RenewalViewModel
                {
                    RenewalId = r.RenewalId,
                    DealId = r.DealId,
                    LicenseId = r.DealId,
                    ProjectPipelineId = null,
                    ProductName = r.Deal?.Product?.ProductName ?? "Unknown Product",
                    ClientName = r.Deal?.Company?.CompanyName ?? "Unknown Client",
                    RenewalDate = r.RenewalDate,
                    RenewalAmount = r.RenewalAmount,
                    Remarks = r.Remarks ?? "No remarks",
                    Status = "Pending",
                    Type = "License",
                    LicenseEndDate = r.Deal?.LicenseEndDate,
                    LicenseStatus = r.Deal?.LicenseDeliveryStatus,
                    Deal = r.Deal
                });
            }

            // Get deals with licenses that are approaching expiry but don't have renewal reminders yet
            var expiringDeals = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Company)
                .Where(d => (d.LicenseDeliveryStatus == "Delivered" || d.LicenseDeliveryStatus == "Activated") &&
                           d.LicenseEndDate.HasValue &&
                           d.LicenseEndDate.Value <= DateTime.Now.AddMonths(6) && // Show deals expiring in next 6 months
                           !_context.Renewals.Any(r => r.DealId == d.DealId)) // No renewal reminder configured
                .ToListAsync();

            foreach (var d in expiringDeals)
            {
                renewalsList.Add(new RenewalViewModel
                {
                    RenewalId = 0,
                    DealId = d.DealId,
                    LicenseId = d.DealId,
                    ProjectPipelineId = null,
                    ProductName = d.Product?.ProductName ?? "Unknown Product",
                    ClientName = d.Company?.CompanyName ?? "Unknown Client",
                    RenewalDate = d.LicenseEndDate ?? DateTime.Now.AddDays(30), // Safe fallback
                    RenewalAmount = d.CustomerInvoiceAmount ?? 0,
                    Remarks = $"Suggested renewal - License expires on {d.LicenseEndDate?.ToString("yyyy-MM-dd") ?? "Unknown date"}",
                    Status = "Suggested",
                    Type = "License Expiry",
                    LicenseEndDate = d.LicenseEndDate,
                    LicenseStatus = d.LicenseDeliveryStatus,
                    Deal = d
                });
            }

            // Get pipeline project renewals (projects approaching expected license dates)
            var pipelineProjects = await _context.ProjectPipelines
                .Where(p => p.ExpectedLicenseDate.AddMonths(12) <= DateTime.Now.AddMonths(3) &&
                           p.ProjectStatus == "Confirmed" &&
                           !p.ConvertedToLicenseId.HasValue)
                .ToListAsync();

            foreach (var p in pipelineProjects)
            {
                renewalsList.Add(new RenewalViewModel
                {
                    RenewalId = 0,
                    DealId = null,
                    LicenseId = null,
                    ProjectPipelineId = p.ProjectPipelineId,
                    ProductName = p.ProductName ?? "Unknown Product",
                    ClientName = p.ClientName ?? "Unknown Client",
                    RenewalDate = p.ExpectedLicenseDate.AddMonths(12),
                    RenewalAmount = p.ExpectedAmountToReceive,
                    Remarks = $"Pipeline project renewal - Original project: {p.Remarks}",
                    Status = "Pipeline",
                    Type = "Pipeline Project",
                    LicenseEndDate = null,
                    LicenseStatus = "Pipeline",
                    Deal = null
                });
            }

            var allRenewals = renewalsList.OrderBy(r => r.RenewalDate).ToList();
            return View(allRenewals);
        }

        [HttpGet]
        public async Task<IActionResult> ConfigureReminders()
        {
            // Get only deals that have delivered or activated licenses and are eligible for renewal
            var eligibleDeals = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Company)
                .Where(d => d.LicenseDeliveryStatus == "Delivered" || d.LicenseDeliveryStatus == "Activated")
                .ToListAsync();

            ViewBag.Deals = eligibleDeals.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Value = d.DealId.ToString(),
                Text = $"{d.Product?.ProductName ?? "Unknown"} - {d.Company?.CompanyName ?? "Unknown"} " +
                       $"(Deal #{d.DealId}) - License Expires: {d.LicenseEndDate?.ToString("yyyy-MM-dd") ?? "N/A"}"
            }).ToList();

            // Add statistics for the sidebar
            ViewBag.TotalActiveDeals = eligibleDeals.Count;
            ViewBag.ConfiguredRenewals = await _context.Renewals.CountAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigureReminders(int dealId, DateTime renewalDate, decimal renewalAmount, string remarks)
        {
            // Basic validation
            if (dealId <= 0)
            {
                ModelState.AddModelError("dealId", "Please select a valid deal.");
            }
            if (renewalDate <= DateTime.Now)
            {
                ModelState.AddModelError("renewalDate", "Renewal date must be in the future.");
            }
            if (renewalAmount <= 0)
            {
                ModelState.AddModelError("renewalAmount", "Renewal amount must be greater than zero.");
            }

            // Check if the deal exists and has an active license
            var deal = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.DealId == dealId &&
                    (d.LicenseDeliveryStatus == "Delivered" || d.LicenseDeliveryStatus == "Activated"));

            if (deal == null)
            {
                ModelState.AddModelError("dealId", "Selected deal does not exist or does not have an active license.");
            }

            // Check if a renewal already exists for this deal
            var existingRenewal = await _context.Renewals.FirstOrDefaultAsync(r => r.DealId == dealId);
            if (existingRenewal != null)
            {
                ModelState.AddModelError("dealId", "A renewal reminder already exists for this deal.");
            }

            if (!ModelState.IsValid)
            {
                var eligibleDeals = await _context.Deals
                    .Include(d => d.Product)
                    .Include(d => d.Company)
                    .Where(d => d.LicenseDeliveryStatus == "Delivered" || d.LicenseDeliveryStatus == "Activated")
                    .ToListAsync();

                ViewBag.Deals = eligibleDeals.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = d.DealId.ToString(),
                    Text = $"{d.Product?.ProductName ?? "Unknown"} - {d.Company?.CompanyName ?? "Unknown"} " +
                           $"(Deal #{d.DealId}) - License Expires: {d.LicenseEndDate?.ToString("yyyy-MM-dd") ?? "N/A"}"
                }).ToList();

                ViewBag.TotalActiveDeals = eligibleDeals.Count;
                ViewBag.ConfiguredRenewals = await _context.Renewals.CountAsync();
                return View();
            }

            try
            {
                var renewal = new Renewal
                {
                    DealId = dealId,
                    RenewalDate = renewalDate,
                    RenewalAmount = renewalAmount,
                    Remarks = remarks ?? string.Empty
                };

                _context.Renewals.Add(renewal);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Renewal reminder configured successfully for {deal?.Product?.ProductName ?? "the selected deal"}!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                var eligibleDeals = await _context.Deals
                    .Include(d => d.Product)
                    .Include(d => d.Company)
                    .Where(d => d.LicenseDeliveryStatus == "Delivered" || d.LicenseDeliveryStatus == "Activated")
                    .ToListAsync();

                ViewBag.Deals = eligibleDeals.Select(d => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = d.DealId.ToString(),
                    Text = $"{d.Product?.ProductName ?? "Unknown"} - {d.Company?.CompanyName ?? "Unknown"} " +
                           $"(Deal #{d.DealId}) - License Expires: {d.LicenseEndDate?.ToString("yyyy-MM-dd") ?? "N/A"}"
                }).ToList();

                ViewBag.TotalActiveDeals = eligibleDeals.Count;
                ViewBag.ConfiguredRenewals = await _context.Renewals.CountAsync();
                ModelState.AddModelError(string.Empty, $"Error saving renewal: {ex.Message}");
                return View();
            }
        }
    }
}
