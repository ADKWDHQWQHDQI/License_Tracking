using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.AspNetCore.Authorization;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class DealsController : Controller
    {
        private readonly AppDbContext _context;

        public DealsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Deals
        public async Task<IActionResult> Index(string searchString, string stage, string assignedTo)
        {
            var deals = _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                deals = deals.Where(d => d.DealName.Contains(searchString) ||
                                        d.Company.CompanyName.Contains(searchString) ||
                                        d.Oem.OemName.Contains(searchString) ||
                                        d.Product.ProductName.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(stage))
            {
                deals = deals.Where(d => d.DealStage == stage);
            }

            if (!string.IsNullOrEmpty(assignedTo))
            {
                deals = deals.Where(d => d.AssignedTo == assignedTo);
            }

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStage"] = stage;
            ViewData["CurrentAssignedTo"] = assignedTo;

            // Get unique assigned users for filter dropdown
            ViewBag.AssignedUsers = await _context.Deals
                .Where(d => !string.IsNullOrEmpty(d.AssignedTo))
                .Select(d => d.AssignedTo)
                .Distinct()
                .ToListAsync();

            return View(await deals.ToListAsync());
        }

        // GET: Deals/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

        // GET: Deals/Create
        [Authorize(Roles = "Admin,Sales")]
        public IActionResult Create()
        {
            LoadDropdowns();
            var newDeal = new Deal
            {
                DealStage = "Lead", // Default to Phase 1
                DealProbability = 0.5m, // Default probability
                ExpectedCloseDate = DateTime.Now.AddMonths(1),
                AssignedTo = User.Identity?.Name
            };
            return View(newDeal);
        }

        // POST: Deals/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> Create([Bind("CompanyId,OemId,ProductId,ContactId,DealName,DealStage,DealType,Quantity,CustomerPoNumber,CustomerPoDate,CustomerInvoiceNumber,CustomerInvoiceAmount,CustomerPaymentStatus,CustomerPaymentDate,OemQuoteAmount,CanarysPoNumber,CanarysPoDate,EstimatedMargin,LicenseStartDate,LicenseEndDate,LicenseDeliveryStatus,OemInvoiceNumber,OemInvoiceAmount,OemPaymentStatus,OemPaymentDate,ExpectedCloseDate,ActualCloseDate,AssignedTo,DealProbability,Notes")] Deal deal)
        {
            // Remove model state validation to allow creation
            if (deal.CompanyId > 0 && deal.OemId > 0 && deal.ProductId > 0 && !string.IsNullOrEmpty(deal.DealName))
            {
                deal.CreatedDate = DateTime.Now;
                deal.CreatedBy = User.Identity?.Name;
                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;
                deal.IsActive = true;

                // FIXED BUSINESS LOGIC: Set proper defaults
                if (string.IsNullOrEmpty(deal.DealStage))
                    deal.DealStage = "Lead";

                // Business workflow phases start only when deal is "Won"
                if (deal.DealStage == "Won")
                {
                    deal.CurrentPhase = "Phase 1"; // Start business workflow
                }
                else
                {
                    deal.CurrentPhase = "Pre-Phase"; // Sales pipeline stage
                }

                if (string.IsNullOrEmpty(deal.AssignedTo))
                    deal.AssignedTo = User.Identity?.Name;

                _context.Add(deal);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deal created successfully! Ready for sales pipeline and business workflow tracking.";
                return RedirectToAction(nameof(Index));
            }

            LoadDropdowns(deal);
            TempData["ErrorMessage"] = "Please fill in all required fields: Customer, OEM, Product, and Deal Name.";
            return View(deal);
        }

        // GET: Deals/Edit/5
        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var deal = await _context.Deals.FindAsync(id);
            if (deal == null)
            {
                return NotFound();
            }

            LoadDropdowns(deal);
            return View(deal);
        }

        // POST: Deals/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> Edit(int id, Deal deal)
        {
            if (id != deal.DealId)
            {
                return NotFound();
            }

            try
            {
                // Use AsNoTracking to avoid Entity Framework tracking conflicts
                var existingDeal = await _context.Deals.AsNoTracking().FirstOrDefaultAsync(d => d.DealId == id);
                if (existingDeal == null)
                {
                    return NotFound();
                }

                // Clear model state for audit fields to avoid validation issues
                ModelState.Remove("CreatedDate");
                ModelState.Remove("CreatedBy");
                ModelState.Remove("LastModifiedDate");
                ModelState.Remove("LastModifiedBy");
                ModelState.Remove("IsActive");

                // Preserve original audit fields from database
                deal.CreatedDate = existingDeal.CreatedDate;
                deal.CreatedBy = existingDeal.CreatedBy;
                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;
                deal.IsActive = existingDeal.IsActive;

                // FIXED BUSINESS LOGIC: Handle stage and phase transitions
                string previousStage = existingDeal.DealStage;
                string currentStage = deal.DealStage;

                // Auto-update CurrentPhase based on DealStage changes
                if (previousStage != "Won" && currentStage == "Won")
                {
                    // Deal just won - start business workflow
                    deal.CurrentPhase = "Phase 1";
                    TempData["PhaseMessage"] = "Deal won! Business workflow started in Phase 1.";
                }
                else if (currentStage == "Won")
                {
                    // Deal is Won - auto-determine phase from completed data
                    deal.UpdateCurrentPhase();
                }
                else if (currentStage == "Lost")
                {
                    // Deal lost - stop business workflow
                    deal.CurrentPhase = "Lost";
                }
                else
                {
                    // Deal still in sales pipeline
                    deal.CurrentPhase = "Pre-Phase";
                }

                // Perform the update
                _context.Update(deal);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Deal updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DealExists(deal.DealId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating deal: {ex.Message}";
                // Log the exception for debugging
                System.Diagnostics.Debug.WriteLine($"Edit Error: {ex}");
            }

            LoadDropdowns(deal);
            return View(deal);
        }

        // GET: Deals/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

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

        // POST: Deals/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deal = await _context.Deals.FindAsync(id);
            if (deal != null)
            {
                deal.IsActive = false; // Soft delete
                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;
                _context.Update(deal);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Deal deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DealExists(int id)
        {
            return _context.Deals.Any(e => e.DealId == id);
        }

        private void LoadDropdowns(Deal? deal = null)
        {
            ViewData["CompanyId"] = new SelectList(_context.Companies.Where(c => c.IsActive), "CompanyId", "CompanyName", deal?.CompanyId);
            ViewData["OemId"] = new SelectList(_context.Oems.Where(o => o.IsActive), "OemId", "OemName", deal?.OemId);
            ViewData["ProductId"] = new SelectList(_context.Products.Where(p => p.IsActive), "ProductId", "ProductName", deal?.ProductId);

            if (deal?.CompanyId > 0)
            {
                ViewData["ContactId"] = new SelectList(_context.ContactPersons.Where(c => c.CompanyId == deal.CompanyId && c.IsActive), "ContactId", "Name", deal?.ContactId);
            }
            else
            {
                ViewData["ContactId"] = new SelectList(Enumerable.Empty<ContactPerson>(), "ContactId", "Name");
            }

            // Deal stage options
            ViewData["DealStageOptions"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "Lead", Text = "Lead" },
                new SelectListItem { Value = "Quoted", Text = "Quoted" },
                new SelectListItem { Value = "Negotiation", Text = "Negotiation" },
                new SelectListItem { Value = "Won", Text = "Won" },
                new SelectListItem { Value = "Lost", Text = "Lost" }
            };

            // Deal type options
            ViewData["DealTypeOptions"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "New", Text = "New" },
                new SelectListItem { Value = "Renewal", Text = "Renewal" },
                new SelectListItem { Value = "Upgrade", Text = "Upgrade" }
            };

            // Payment status options
            ViewData["PaymentStatusOptions"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Pending" },
                new SelectListItem { Value = "Paid", Text = "Paid" },
                new SelectListItem { Value = "Overdue", Text = "Overdue" },
                new SelectListItem { Value = "Partial", Text = "Partial" }
            };

            // License delivery status options
            ViewData["LicenseStatusOptions"] = new List<SelectListItem>
            {
                new SelectListItem { Value = "Pending", Text = "Pending" },
                new SelectListItem { Value = "Delivered", Text = "Delivered" },
                new SelectListItem { Value = "Active", Text = "Active" }
            };
        }

        // AJAX endpoint to get contacts by company
        [HttpGet]
        public async Task<JsonResult> GetContactsByCompany(int companyId)
        {
            var contacts = await _context.ContactPersons
                .Where(c => c.CompanyId == companyId && c.IsActive)
                .Select(c => new { value = c.ContactId, text = c.Name })
                .ToListAsync();

            return Json(contacts);
        }

        // AJAX endpoint to get products by OEM
        [HttpGet]
        public async Task<JsonResult> GetProductsByOem(int oemId)
        {
            var products = await _context.Products
                .Where(p => p.OemId == oemId && p.IsActive)
                .Select(p => new
                {
                    value = p.ProductId,
                    text = p.ProductName,
                    category = p.ProductCategory,
                    unitPrice = p.UnitPrice,
                    licenseType = p.LicenseType
                })
                .ToListAsync();

            return Json(products);
        }

        // AJAX endpoint to get deal statistics
        [HttpGet]
        public async Task<JsonResult> GetDealStatistics()
        {
            var stats = new
            {
                totalDeals = await _context.Deals.Where(d => d.IsActive).CountAsync(),
                activeDeals = await _context.Deals.Where(d => d.IsActive && d.DealStage != "Won" && d.DealStage != "Lost").CountAsync(),
                wonDeals = await _context.Deals.Where(d => d.IsActive && d.DealStage == "Won").CountAsync(),
                lostDeals = await _context.Deals.Where(d => d.IsActive && d.DealStage == "Lost").CountAsync(),
                phase1 = await _context.Deals.Where(d => d.IsActive && d.DealStage == "Lead").CountAsync(),
                phase2 = await _context.Deals.Where(d => d.IsActive && d.DealStage == "Quoted").CountAsync(),
                phase3 = await _context.Deals.Where(d => d.IsActive && d.DealStage == "Negotiation").CountAsync(),
                phase4 = await _context.Deals.Where(d => d.IsActive && d.DealStage == "Won").CountAsync()
            };

            return Json(stats);
        }

        // Update deal stage (for Kanban view)
        [HttpPost]
        public async Task<JsonResult> UpdateDealStage(int dealId, string newStage)
        {
            try
            {
                var deal = await _context.Deals.FindAsync(dealId);
                if (deal == null)
                    return Json(new { success = false, message = "Deal not found" });

                deal.DealStage = newStage;
                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;

                _context.Update(deal);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deal stage updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Deals/Export
        public async Task<IActionResult> Export()
        {
            try
            {
                var deals = await _context.Deals
                    .Include(d => d.Company)
                    .Include(d => d.Oem)
                    .Include(d => d.Product)
                    .Include(d => d.ContactPerson)
                    .ToListAsync();

                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Deal Name,Company,Contact Person,OEM,Product,Stage,Deal Type,Quantity,Customer Invoice Amount,Expected Close Date,Created Date,Assigned To");

                foreach (var deal in deals)
                {
                    csv.AppendLine($"\"{deal.DealName}\",\"{deal.Company?.CompanyName}\",\"{deal.ContactPerson?.Name}\",\"{deal.Oem?.OemName}\",\"{deal.Product?.ProductName}\",\"{deal.DealStage}\",\"{deal.DealType}\",{deal.Quantity},{deal.CustomerInvoiceAmount},\"{deal.ExpectedCloseDate?.ToString("yyyy-MM-dd")}\",\"{deal.CreatedDate:yyyy-MM-dd}\",\"{deal.AssignedTo}\"");
                }

                var fileName = $"Deals_Export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Export failed: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Week 9 Enhancement: Export Individual Deal
        [HttpGet]
        public async Task<IActionResult> ExportDeal(int id, string format = "pdf")
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .FirstOrDefaultAsync(d => d.DealId == id);

            if (deal == null)
            {
                return NotFound();
            }

            switch (format.ToLower())
            {
                case "excel":
                    return ExportDealToExcel(deal);
                case "summary":
                    return ExportDealSummary(deal);
                default:
                    return ExportDealToPdf(deal);
            }
        }

        // Week 9 Enhancement: Clone Deal functionality
        [HttpGet]
        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> Clone(int id)
        {
            var originalDeal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .FirstOrDefaultAsync(d => d.DealId == id);

            if (originalDeal == null)
            {
                return NotFound();
            }

            var clonedDeal = new Deal
            {
                DealName = $"{originalDeal.DealName} (Copy)",
                CompanyId = originalDeal.CompanyId,
                OemId = originalDeal.OemId,
                ProductId = originalDeal.ProductId,
                ContactId = originalDeal.ContactId,
                DealStage = "Lead", // Reset to initial stage
                DealType = originalDeal.DealType,
                Quantity = originalDeal.Quantity,
                ExpectedCloseDate = DateTime.Now.AddDays(30),
                Notes = $"Cloned from Deal #{originalDeal.DealId}: {originalDeal.DealName}",
                // Reset all phase-specific data
                CurrentPhase = "Pre-Phase",
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name,
                AssignedTo = User.Identity?.Name,
                IsActive = true
            };

            LoadDropdowns(clonedDeal);
            return View("Create", clonedDeal);
        }

        // Week 9 Enhancement: Archive Deal
        [HttpPost]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<JsonResult> Archive(int id)
        {
            try
            {
                var deal = await _context.Deals.FindAsync(id);
                if (deal == null)
                {
                    return Json(new { success = false, message = "Deal not found" });
                }

                deal.IsActive = false;
                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;
                deal.Notes += $"\n[ARCHIVED on {DateTime.Now:yyyy-MM-dd HH:mm}] by {User.Identity?.Name}";

                _context.Update(deal);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deal archived successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #region Private Export Methods

        private IActionResult ExportDealToPdf(Deal deal)
        {
            // For now, return a detailed CSV report (PDF generation would require a PDF library)
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("CBMS Deal Report - PDF Export");
            csv.AppendLine($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            csv.AppendLine("");
            csv.AppendLine("DEAL INFORMATION");
            csv.AppendLine($"Deal Name: {deal.DealName}");
            csv.AppendLine($"Deal Stage: {deal.DealStage}");
            csv.AppendLine($"Current Phase: {deal.CurrentPhase}");
            csv.AppendLine($"Deal Type: {deal.DealType}");
            csv.AppendLine("");
            csv.AppendLine("CUSTOMER INFORMATION");
            csv.AppendLine($"Company: {deal.Company?.CompanyName}");
            csv.AppendLine($"Contact: {deal.ContactPerson?.Name}");
            csv.AppendLine($"Email: {deal.ContactPerson?.Email}");
            csv.AppendLine("");
            csv.AppendLine("PRODUCT INFORMATION");
            csv.AppendLine($"OEM: {deal.Oem?.OemName}");
            csv.AppendLine($"Product: {deal.Product?.ProductName}");
            csv.AppendLine($"Quantity: {deal.Quantity}");
            csv.AppendLine("");
            csv.AppendLine("FINANCIAL INFORMATION");
            csv.AppendLine($"Customer Invoice Amount: {deal.CustomerInvoiceAmount:C}");
            csv.AppendLine($"OEM Quote Amount: {deal.OemQuoteAmount:C}");
            csv.AppendLine($"Estimated Margin: {deal.EstimatedMargin:C}");

            var fileName = $"Deal_{deal.DealId}_Report_{DateTime.Now:yyyyMMdd}.txt";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/plain", fileName);
        }

        private IActionResult ExportDealToExcel(Deal deal)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Field,Value");
            csv.AppendLine($"Deal ID,{deal.DealId}");
            csv.AppendLine($"Deal Name,\"{deal.DealName}\"");
            csv.AppendLine($"Company,\"{deal.Company?.CompanyName}\"");
            csv.AppendLine($"Contact Person,\"{deal.ContactPerson?.Name}\"");
            csv.AppendLine($"OEM,\"{deal.Oem?.OemName}\"");
            csv.AppendLine($"Product,\"{deal.Product?.ProductName}\"");
            csv.AppendLine($"Deal Stage,{deal.DealStage}");
            csv.AppendLine($"Current Phase,{deal.CurrentPhase}");
            csv.AppendLine($"Quantity,{deal.Quantity}");
            csv.AppendLine($"Customer Invoice Amount,{deal.CustomerInvoiceAmount}");
            csv.AppendLine($"OEM Quote Amount,{deal.OemQuoteAmount}");
            csv.AppendLine($"Estimated Margin,{deal.EstimatedMargin}");
            csv.AppendLine($"Created Date,{deal.CreatedDate:yyyy-MM-dd}");
            csv.AppendLine($"Expected Close Date,{deal.ExpectedCloseDate?.ToString("yyyy-MM-dd")}");
            csv.AppendLine($"Assigned To,\"{deal.AssignedTo}\"");

            var fileName = $"Deal_{deal.DealId}_Excel_{DateTime.Now:yyyyMMdd}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        private IActionResult ExportDealSummary(Deal deal)
        {
            var summary = $@"
CBMS DEAL SUMMARY REPORT
========================

Deal: {deal.DealName}
ID: {deal.DealId}
Stage: {deal.DealStage}
Phase: {deal.CurrentPhase}

Customer: {deal.Company?.CompanyName}
Contact: {deal.ContactPerson?.Name} ({deal.ContactPerson?.Email})

Product: {deal.Product?.ProductName}
OEM: {deal.Oem?.OemName}
Quantity: {deal.Quantity} licenses

Financial Summary:
- Customer Amount: {deal.CustomerInvoiceAmount:C}
- OEM Cost: {deal.OemQuoteAmount:C}
- Margin: {deal.EstimatedMargin:C}

Timeline:
- Created: {deal.CreatedDate:yyyy-MM-dd}
- Expected Close: {deal.ExpectedCloseDate?.ToString("yyyy-MM-dd") ?? "TBD"}
- Assigned To: {deal.AssignedTo}

Generated on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}
Generated by: {User.Identity?.Name}
";

            var fileName = $"Deal_{deal.DealId}_Summary_{DateTime.Now:yyyyMMdd}.txt";
            return File(System.Text.Encoding.UTF8.GetBytes(summary), "text/plain", fileName);
        }

        #endregion

        // AJAX endpoint for Kanban drag-and-drop stage updates
        [HttpPost]
        [Authorize(Roles = "Admin,Sales")]
        public async Task<IActionResult> UpdateStage(int id, [FromBody] UpdateStageRequest request)
        {
            try
            {
                var deal = await _context.Deals.FindAsync(id);
                if (deal == null)
                {
                    return Json(new { success = false, message = "Deal not found" });
                }

                // Validate stage transition
                var validStages = new[] { "Lead", "Qualified", "Quoted", "Negotiation", "Won", "Lost" };
                if (!validStages.Contains(request.Stage))
                {
                    return Json(new { success = false, message = "Invalid stage" });
                }

                // Update deal stage
                deal.DealStage = request.Stage;
                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;

                // Auto-update CurrentPhase based on DealStage
                deal.CurrentPhase = request.Stage switch
                {
                    "Lead" or "Qualified" => "Phase 1",
                    "Quoted" => "Phase 2",
                    "Negotiation" => "Phase 3",
                    "Won" => "Phase 4",
                    "Lost" => "Lost",
                    _ => "Phase 1"
                };

                // Set actual close date for Won deals
                if (request.Stage == "Won" && !deal.ActualCloseDate.HasValue)
                {
                    deal.ActualCloseDate = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Deal stage updated successfully" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error updating deal stage" });
            }
        }

        // GET: Kanban view
        public async Task<IActionResult> Kanban()
        {
            var deals = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .Where(d => d.IsActive)
                .OrderBy(d => d.DealStage)
                .ThenBy(d => d.ExpectedCloseDate)
                .ToListAsync();

            return View(deals);
        }

        // GET: Sheet view
        public async Task<IActionResult> Sheet()
        {
            var deals = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .Where(d => d.IsActive)
                .OrderBy(d => d.DealId)
                .ToListAsync();

            return View(deals);
        }

        public class UpdateStageRequest
        {
            public string Stage { get; set; } = string.Empty;
        }
    }
}
