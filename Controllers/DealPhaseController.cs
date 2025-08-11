using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class DealPhaseController : Controller
    {
        private readonly AppDbContext _context;

        public DealPhaseController(AppDbContext context)
        {
            _context = context;
        }

        // Phase 3: License Delivery Management
        [HttpGet]
        [Authorize(Roles = "Admin,Operations")]
        public async Task<IActionResult> Phase3LicenseDelivery(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .FirstOrDefaultAsync(m => m.DealId == id);

            if (deal == null)
            {
                return NotFound();
            }

            return View(deal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Operations")]
        public async Task<IActionResult> Phase3LicenseDelivery(int id, Phase3LicenseViewModel model)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                deal.LicenseStartDate = model.LicenseStartDate;
                deal.LicenseEndDate = model.LicenseEndDate;
                deal.LicenseDeliveryStatus = model.LicenseDeliveryStatus;
                deal.Notes = model.Notes;

                // Update deal stage to reflect Phase 3 completion
                if (model.LicenseDeliveryStatus == "Delivered")
                {
                    deal.DealStage = "License Delivered";
                }

                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;

                _context.Update(deal);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Phase 3 - License delivery updated successfully!";
                return RedirectToAction("Details", "Deals", new { id = deal.DealId });
            }

            return View(model);
        }

        // Phase 4: OEM Settlement Management
        [HttpGet]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> Phase4OemSettlement(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .FirstOrDefaultAsync(m => m.DealId == id);

            if (deal == null)
            {
                return NotFound();
            }

            return View(deal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> Phase4OemSettlement(int id, Phase4SettlementViewModel model)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                deal.OemInvoiceNumber = model.OemInvoiceNumber;
                deal.OemInvoiceAmount = model.OemInvoiceAmount;
                deal.OemPaymentStatus = model.OemPaymentStatus;
                deal.OemPaymentDate = model.OemPaymentDate;
                deal.Notes = model.Notes;

                // Update deal stage to reflect Phase 4 completion
                if (model.OemPaymentStatus == "Paid")
                {
                    deal.DealStage = "Completed";
                    deal.ActualCloseDate = DateTime.Now;
                }

                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;

                _context.Update(deal);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Phase 4 - OEM settlement updated successfully!";
                return RedirectToAction("Details", "Deals", new { id = deal.DealId });
            }

            return View(model);
        }

        // Deal Collaboration & Assignment
        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> AssignDeal(int id)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .FirstOrDefaultAsync(m => m.DealId == id);

            if (deal == null)
            {
                return NotFound();
            }

            // Load available users for assignment
            ViewBag.AvailableUsers = new List<string>
            {
                "john.doe@canarys.com",
                "jane.smith@canarys.com",
                "mike.johnson@canarys.com",
                "sarah.wilson@canarys.com"
            };

            return View(deal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> AssignDeal(int id, string assignedTo, string assignmentReason)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            var oldAssignee = deal.AssignedTo;
            deal.AssignedTo = assignedTo;
            deal.LastModifiedDate = DateTime.Now;
            deal.LastModifiedBy = User.Identity?.Name;

            // Add assignment note
            if (!string.IsNullOrEmpty(assignmentReason))
            {
                deal.Notes += $"\n[{DateTime.Now:yyyy-MM-dd HH:mm}] Reassigned from {oldAssignee} to {assignedTo}. Reason: {assignmentReason}";
            }

            _context.Update(deal);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Deal successfully assigned to {assignedTo}!";
            return RedirectToAction("Details", "Deals", new { id = deal.DealId });
        }

        // Quick Phase Status Update API
        [HttpPost]
        [Authorize(Roles = "Admin,Sales,Operations,Finance")]
        public async Task<JsonResult> UpdatePhaseStatus(int dealId, string phase, string status)
        {
            try
            {
                var deal = await _context.Deals.FindAsync(dealId);
                if (deal == null)
                {
                    return Json(new { success = false, message = "Deal not found" });
                }

                switch (phase.ToLower())
                {
                    case "phase1":
                        deal.CustomerPaymentStatus = status;
                        break;
                    case "phase2":
                        // Update OEM procurement status
                        if (status == "Completed")
                        {
                            deal.DealStage = "OEM Procurement Complete";
                        }
                        break;
                    case "phase3":
                        deal.LicenseDeliveryStatus = status;
                        break;
                    case "phase4":
                        deal.OemPaymentStatus = status;
                        if (status == "Paid")
                        {
                            deal.DealStage = "Completed";
                            deal.ActualCloseDate = DateTime.Now;
                        }
                        break;
                }

                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;

                _context.Update(deal);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = $"{phase} status updated to {status}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // ViewModels for Phase Management
    public class Phase3LicenseViewModel
    {
        [Required]
        [Display(Name = "License Start Date")]
        public DateTime LicenseStartDate { get; set; }

        [Required]
        [Display(Name = "License End Date")]
        public DateTime LicenseEndDate { get; set; }

        [Required]
        [Display(Name = "License Delivery Status")]
        public string LicenseDeliveryStatus { get; set; } = "Pending";

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public class Phase4SettlementViewModel
    {
        [Required]
        [Display(Name = "OEM Invoice Number")]
        public string OemInvoiceNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "OEM Invoice Amount")]
        public decimal OemInvoiceAmount { get; set; }

        [Required]
        [Display(Name = "Payment Status")]
        public string OemPaymentStatus { get; set; } = "Pending";

        [Display(Name = "Payment Date")]
        public DateTime? OemPaymentDate { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
