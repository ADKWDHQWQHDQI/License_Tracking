using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            // Get regular license renewals
            var renewals = await _context.Renewals
                .Include(r => r.Deal)
                .ThenInclude(d => d.Product)
                .Include(r => r.Deal)
                .ThenInclude(d => d.Company)
                .Select(r => new
                {
                    RenewalId = r.RenewalId,
                    LicenseId = (int?)r.DealId,
                    ProjectPipelineId = (int?)null,
                    ProductName = r.Deal.Product != null ? r.Deal.Product.ProductName : "Unknown Product",
                    ClientName = r.Deal.Company != null ? r.Deal.Company.CompanyName : "Unknown Client",
                    RenewalDate = r.RenewalDate,
                    RenewalAmount = r.RenewalAmount,
                    Remarks = r.Remarks ?? "No remarks",
                    Status = "Pending",
                    Type = "License"
                })
                .ToListAsync();

            // Get pipeline project renewals (projects approaching expected license dates)
            var pipelineRenewals = await _context.ProjectPipelines
                .Where(p => p.ExpectedLicenseDate.AddMonths(12) <= DateTime.Now.AddMonths(3) &&
                           p.ProjectStatus == "Confirmed" &&
                           !p.ConvertedToLicenseId.HasValue)
                .Select(p => new
                {
                    RenewalId = 0,
                    LicenseId = (int?)null,
                    ProjectPipelineId = (int?)p.ProjectPipelineId,
                    ProductName = p.ProductName ?? "Unknown Product",
                    ClientName = p.ClientName ?? "Unknown Client",
                    RenewalDate = p.ExpectedLicenseDate.AddMonths(12),
                    RenewalAmount = p.ExpectedAmountToReceive,
                    Remarks = $"Pipeline project renewal - Original project: {p.Remarks}",
                    Status = "Pipeline",
                    Type = "Pipeline Project"
                })
                .ToListAsync();

            var allRenewals = renewals.Concat(pipelineRenewals).OrderBy(r => r.RenewalDate);
            return View(allRenewals);
        }

        [HttpGet]
        public IActionResult ConfigureReminders()
        {
            ViewBag.Licenses = _context.Deals
                .Include(l => l.Product)
                .Select(l => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Value = l.DealId.ToString(),
                    Text = l.Product != null ? l.Product.ProductName : "Unknown Product"
                }).ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfigureReminders(int licenseId, DateTime renewalDate, decimal renewalAmount, string remarks)
        {
            // Basic validation
            if (licenseId <= 0)
            {
                ModelState.AddModelError("licenseId", "Please select a valid license.");
            }
            if (renewalDate <= DateTime.Now)
            {
                ModelState.AddModelError("renewalDate", "Renewal date must be in the future.");
            }
            if (renewalAmount <= 0)
            {
                ModelState.AddModelError("renewalAmount", "Renewal amount must be greater than zero.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Licenses = _context.Deals
                    .Include(l => l.Product)
                    .Select(l => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = l.DealId.ToString(),
                        Text = l.Product != null ? l.Product.ProductName : "Unknown Product"
                    }).ToList();
                return View();
            }

            try
            {
                var renewal = new Renewal
                {
                    DealId = licenseId,
                    RenewalDate = renewalDate,
                    RenewalAmount = renewalAmount,
                    Remarks = remarks ?? string.Empty
                };

                _context.Renewals.Add(renewal);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Renewal reminder configured successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ViewBag.Licenses = _context.Deals
                    .Include(l => l.Product)
                    .Select(l => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                    {
                        Value = l.DealId.ToString(),
                        Text = l.Product != null ? l.Product.ProductName : "Unknown Product"
                    }).ToList();
                ModelState.AddModelError(string.Empty, $"Error saving renewal: {ex.Message}");
                return View();
            }
        }
    }
}
