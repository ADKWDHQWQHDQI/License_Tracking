using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using License_Tracking.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class InvoiceManagementController : Controller
    {
        private readonly AppDbContext _context;

        public InvoiceManagementController(AppDbContext context)
        {
            _context = context;
        }

        // Simple Bigin.com-style Invoice Dashboard with Enhanced Filtering
        public async Task<IActionResult> Index(string phase = "all", string status = "all", string search = "",
            DateTime? startDate = null, DateTime? endDate = null, string sortBy = "date", bool sortDesc = true,
            decimal? minAmount = null, decimal? maxAmount = null, string customer = "", string oem = "")
        {
            var model = new InvoiceManagementViewModel();

            // Build query with proper includes
            var query = _context.CbmsInvoices
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Company)
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Oem)
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Product)
                .AsQueryable();

            // Apply enhanced filters
            if (phase == "phase1")
                query = query.Where(i => i.InvoiceType == "Customer_To_Canarys");
            else if (phase == "phase4")
                query = query.Where(i => i.InvoiceType == "OEM_To_Canarys");

            if (status != "all")
                query = query.Where(i => i.PaymentStatus == status);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    i.InvoiceNumber.Contains(search) ||
                    i.Deal!.Company!.CompanyName.Contains(search) ||
                    i.Deal!.DealName.Contains(search) ||
                    i.Deal!.Oem!.OemName.Contains(search));
            }

            if (!string.IsNullOrEmpty(customer))
                query = query.Where(i => i.Deal!.Company!.CompanyName.Contains(customer));

            if (!string.IsNullOrEmpty(oem))
                query = query.Where(i => i.Deal!.Oem!.OemName.Contains(oem));

            if (startDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(i => i.InvoiceDate <= endDate.Value);

            if (minAmount.HasValue)
                query = query.Where(i => i.TotalAmount >= minAmount.Value);

            if (maxAmount.HasValue)
                query = query.Where(i => i.TotalAmount <= maxAmount.Value);

            var invoices = await query.ToListAsync();

            // Set invoices and filter values
            model.Invoices = invoices;
            model.CurrentPhase = phase;
            model.CurrentStatus = status;
            model.SearchTerm = search;

            // Calculate statistics
            model.TotalInvoices = invoices.Count;
            model.TotalAmount = invoices.Sum(i => i.TotalAmount);
            model.PaidAmount = invoices.Where(i => i.PaymentStatus == "Paid").Sum(i => i.PaymentReceived ?? 0);
            model.PendingAmount = invoices.Where(i => i.PaymentStatus != "Paid").Sum(i => i.TotalAmount - (i.PaymentReceived ?? 0));
            model.OverdueCount = invoices.Count(i => i.DueDate < DateTime.Today && i.PaymentStatus != "Paid");
            model.Phase1Count = invoices.Count(i => i.InvoiceType == "Customer_To_Canarys");
            model.Phase4Count = invoices.Count(i => i.InvoiceType == "OEM_To_Canarys");

            // Set filter options for dropdowns
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.MinAmount = minAmount;
            ViewBag.MaxAmount = maxAmount;
            ViewBag.Customer = customer;
            ViewBag.Oem = oem;
            ViewBag.SortBy = sortBy;
            ViewBag.SortDesc = sortDesc;

            // Add invoice type and status dropdowns for the view
            ViewBag.PhaseOptions = new SelectList(new[]
            {
                new { Value = "all", Text = "All Invoice Types" },
                new { Value = "phase1", Text = "Customer Invoices (Phase 1)" },
                new { Value = "phase4", Text = "OEM Invoices (Phase 4)" }
            }, "Value", "Text", phase);

            ViewBag.StatusOptions = new SelectList(new[]
            {
                new { Value = "all", Text = "All Status" },
                new { Value = "Unpaid", Text = "Unpaid" },
                new { Value = "Partial", Text = "Partial" },
                new { Value = "Paid", Text = "Paid" },
                new { Value = "Overdue", Text = "Overdue" }
            }, "Value", "Text", status);

            return View(model);
        }

        // Unified Create Invoice Method (Customer or OEM Invoice)
        [Authorize(Roles = "Admin,Finance,Operations")]
        public async Task<IActionResult> Create(int dealId, string invoiceType)
        {
            if (string.IsNullOrEmpty(invoiceType) ||
                (invoiceType != "Customer_To_Canarys" && invoiceType != "OEM_To_Canarys"))
            {
                TempData["ErrorMessage"] = "Invalid invoice type. Must be Customer_To_Canarys or OEM_To_Canarys.";
                return RedirectToAction("Index");
            }

            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                TempData["ErrorMessage"] = "Deal not found.";
                return RedirectToAction("Index", "Deals");
            }

            // Determine business phase based on invoice type
            int businessPhase = invoiceType == "Customer_To_Canarys" ? 1 : 4;

            // Check if invoice already exists for this deal and type
            var existing = await _context.CbmsInvoices
                .FirstOrDefaultAsync(i => i.DealId == dealId && i.InvoiceType == invoiceType);

            if (existing != null)
            {
                TempData["ErrorMessage"] = $"{invoiceType.Replace("_", " ")} invoice already exists for this deal.";
                return RedirectToAction("Details", "Deals", new { id = dealId });
            }

            // Calculate amount based on invoice type
            decimal amount = invoiceType == "Customer_To_Canarys"
                ? deal.CustomerInvoiceAmount ?? 0
                : deal.OemInvoiceAmount ?? 0;

            var model = new InvoiceGenerationViewModel
            {
                DealId = dealId,
                Deal = deal,
                BusinessPhase = businessPhase,
                InvoiceType = invoiceType,
                InvoiceNumber = GenerateInvoiceNumber(invoiceType),
                TotalAmount = amount,
                Description = $"{invoiceType.Replace("_", " ")} Invoice - {deal.DealName}",
                InvoiceDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30) // Default 30 days payment terms
            };

            return View(model);
        }

        // POST: Create Invoice (Customer or OEM)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance,Operations")]
        public async Task<IActionResult> Create(InvoiceGenerationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload deal information if validation fails
                model.Deal = await _context.Deals
                    .Include(d => d.Company)
                    .Include(d => d.Product)
                    .Include(d => d.Oem)
                    .FirstOrDefaultAsync(d => d.DealId == model.DealId);
                return View(model);
            }

            try
            {
                var deal = await _context.Deals.FindAsync(model.DealId);
                if (deal == null)
                {
                    TempData["ErrorMessage"] = "Deal not found.";
                    return RedirectToAction("Index");
                }

                // Check if invoice already exists
                var existing = await _context.CbmsInvoices
                    .FirstOrDefaultAsync(i => i.DealId == model.DealId && i.InvoiceType == model.InvoiceType);

                if (existing != null)
                {
                    TempData["ErrorMessage"] = $"{model.InvoiceType.Replace("_", " ")} invoice already exists for this deal.";
                    return RedirectToAction("Details", "Deals", new { id = model.DealId });
                }

                var invoice = new CbmsInvoice
                {
                    DealId = model.DealId,
                    InvoiceType = model.InvoiceType,
                    BusinessPhase = model.BusinessPhase,
                    InvoiceNumber = model.InvoiceNumber,
                    InvoiceDate = model.InvoiceDate,
                    DueDate = model.DueDate,
                    Amount = model.TotalAmount,
                    TaxAmount = 0, // Can be calculated based on business rules
                    TotalAmount = model.TotalAmount,
                    PaymentStatus = "Unpaid",
                    Notes = model.Description,
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity?.Name
                };

                // Handle file attachment
                if (model.AttachmentFile != null)
                {
                    var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "invoices");
                    Directory.CreateDirectory(uploadsPath);

                    var fileName = $"{invoice.InvoiceNumber}_{Path.GetFileName(model.AttachmentFile.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.AttachmentFile.CopyToAsync(stream);
                    }

                    invoice.AttachmentPath = $"/uploads/invoices/{fileName}";
                    invoice.AttachmentFileName = model.AttachmentFile.FileName;
                }

                _context.CbmsInvoices.Add(invoice);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{model.InvoiceType.Replace("_", " ")} invoice created successfully.";
                return RedirectToAction("Details", new { id = invoice.InvoiceId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error creating invoice: {ex.Message}";
                // Reload deal information if error occurs
                model.Deal = await _context.Deals
                    .Include(d => d.Company)
                    .Include(d => d.Product)
                    .Include(d => d.Oem)
                    .FirstOrDefaultAsync(d => d.DealId == model.DealId);
                return View(model);
            }
        }

        // Edit Invoice
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> Edit(int id)
        {
            var invoice = await _context.CbmsInvoices
                .Include(i => i.Deal)
                    .ThenInclude(d => d.Company)
                .Include(i => i.Deal)
                    .ThenInclude(d => d.Product)
                .Include(i => i.Deal)
                    .ThenInclude(d => d.Oem)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            // Don't allow editing if payments have been made
            if ((invoice.PaymentReceived ?? 0) > 0)
            {
                TempData["ErrorMessage"] = "Cannot edit invoice with recorded payments.";
                return RedirectToAction("Details", new { id });
            }

            var model = new InvoiceGenerationViewModel
            {
                DealId = invoice.DealId,
                Deal = invoice.Deal,
                InvoiceType = invoice.InvoiceType,
                BusinessPhase = invoice.BusinessPhase,
                InvoiceNumber = invoice.InvoiceNumber,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate ?? DateTime.Now.AddDays(30),
                TotalAmount = invoice.TotalAmount,
                Description = invoice.Notes
            };

            ViewBag.InvoiceId = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> Edit(int id, InvoiceGenerationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.InvoiceId = id;
                return View(model);
            }

            var invoice = await _context.CbmsInvoices.FindAsync(id);
            if (invoice == null)
                return NotFound();

            // Don't allow editing if payments have been made
            if ((invoice.PaymentReceived ?? 0) > 0)
            {
                TempData["ErrorMessage"] = "Cannot edit invoice with recorded payments.";
                return RedirectToAction("Details", new { id });
            }

            // Update invoice
            invoice.InvoiceDate = model.InvoiceDate;
            invoice.DueDate = model.DueDate;
            invoice.Amount = model.TotalAmount;
            invoice.TotalAmount = model.TotalAmount;
            invoice.Notes = model.Description;
            invoice.ModifiedBy = User.Identity?.Name;
            invoice.ModifiedDate = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Invoice {invoice.InvoiceNumber} updated successfully!";
            return RedirectToAction("Details", new { id });
        }

        // Bulk Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> BulkDelete(List<int> selectedInvoices)
        {
            if (selectedInvoices == null || !selectedInvoices.Any())
            {
                TempData["ErrorMessage"] = "No invoices selected.";
                return RedirectToAction("Index");
            }

            var invoices = await _context.CbmsInvoices
                .Where(i => selectedInvoices.Contains(i.InvoiceId))
                .ToListAsync();

            // Check if any invoice has payments
            var invoicesWithPayments = invoices.Where(i => (i.PaymentReceived ?? 0) > 0).ToList();
            if (invoicesWithPayments.Any())
            {
                TempData["ErrorMessage"] = $"Cannot delete {invoicesWithPayments.Count} invoice(s) with recorded payments.";
                return RedirectToAction("Index");
            }

            _context.CbmsInvoices.RemoveRange(invoices);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{invoices.Count} invoice(s) deleted successfully.";
            return RedirectToAction("Index");
        }

        // Export to CSV
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> ExportCsv(string phase = "all", string status = "all", string search = "")
        {
            var query = _context.CbmsInvoices
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Company)
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Oem)
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Product)
                .AsQueryable();

            // Apply same filters as Index
            if (phase == "phase1") query = query.Where(i => i.BusinessPhase == 1);
            else if (phase == "phase2") query = query.Where(i => i.BusinessPhase == 2);
            else if (phase == "phase3") query = query.Where(i => i.BusinessPhase == 3);
            else if (phase == "phase4") query = query.Where(i => i.BusinessPhase == 4);

            if (status != "all") query = query.Where(i => i.PaymentStatus == status);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    i.InvoiceNumber.Contains(search) ||
                    i.Deal!.Company!.CompanyName.Contains(search) ||
                    i.Deal!.DealName.Contains(search));
            }

            var invoices = await query.OrderByDescending(i => i.InvoiceDate).ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Invoice Number,Phase,Deal Name,Customer,OEM,Product,Amount,Payment Status,Invoice Date,Due Date,Payment Received");

            foreach (var invoice in invoices)
            {
                csv.AppendLine($"{invoice.InvoiceNumber},Phase {invoice.BusinessPhase},{invoice.Deal?.DealName},{invoice.Deal?.Company?.CompanyName},{invoice.Deal?.Oem?.OemName},{invoice.Deal?.Product?.ProductName},{invoice.TotalAmount},{invoice.PaymentStatus},{invoice.InvoiceDate:yyyy-MM-dd},{invoice.DueDate:yyyy-MM-dd},{invoice.PaymentReceived ?? 0}");
            }

            var fileName = $"invoices_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            return File(System.Text.Encoding.UTF8.GetBytes(csv.ToString()), "text/csv", fileName);
        }

        // Get Overdue Invoices for Notifications
        [HttpGet]
        public async Task<IActionResult> GetOverdueInvoices()
        {
            var overdueInvoices = await _context.CbmsInvoices
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Company)
                .Where(i => i.DueDate < DateTime.Today && i.PaymentStatus != "Paid")
                .Select(i => new
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    Customer = i.Deal!.Company!.CompanyName,
                    Amount = i.TotalAmount,
                    DueDate = i.DueDate,
                    DaysOverdue = (DateTime.Today - i.DueDate!.Value).Days
                })
                .ToListAsync();

            return Json(overdueInvoices);
        }

        // Legacy Customer Invoice Creation (redirects to unified method)
        [Authorize(Roles = "Admin,Finance,Sales")]
        public async Task<IActionResult> CreateCustomerInvoice(int dealId)
        {
            return RedirectToAction("Create", new { dealId = dealId, invoiceType = "Customer_To_Canarys" });
        }

        // Legacy OEM Invoice Creation (redirects to unified method)
        [Authorize(Roles = "Admin,Finance,Operations")]
        public async Task<IActionResult> CreateOemInvoice(int dealId)
        {
            return RedirectToAction("Create", new { dealId = dealId, invoiceType = "OEM_To_Canarys" });
        }

        // Details View for Invoice
        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.CbmsInvoices
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Company)
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Product)
                .Include(i => i.Deal)
                    .ThenInclude(d => d!.Oem)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        // Record Payment for Invoice
        public async Task<IActionResult> RecordPayment(int id)
        {
            var invoice = await _context.CbmsInvoices
                .Include(i => i.Deal)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);

            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            var model = new PaymentRecordViewModel
            {
                InvoiceId = invoice.InvoiceId,
                Invoice = invoice,
                OutstandingAmount = invoice.TotalAmount - (invoice.PaymentReceived ?? 0)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> RecordPayment(PaymentRecordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var invoice = await _context.CbmsInvoices
                .Include(i => i.Deal)
                .FirstOrDefaultAsync(i => i.InvoiceId == model.InvoiceId);

            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            // Update payment information
            invoice.PaymentReceived = (invoice.PaymentReceived ?? 0) + model.PaymentAmount;
            invoice.PaymentMethod = model.PaymentMethod;
            invoice.PaymentReference = model.ReferenceNumber;
            invoice.PaymentDate = model.PaymentDate;

            // Update status based on payment
            if (invoice.PaymentReceived >= invoice.TotalAmount)
            {
                invoice.PaymentStatus = "Paid";
            }
            else if (invoice.PaymentReceived > 0)
            {
                invoice.PaymentStatus = "Partial";
            }

            // Update deal payment status
            if (invoice.Deal != null)
            {
                if (invoice.BusinessPhase == 1) // Customer payment
                {
                    invoice.Deal.CustomerPaymentStatus = invoice.PaymentStatus;
                    invoice.Deal.CustomerPaymentDate = model.PaymentDate;
                }
                else if (invoice.BusinessPhase == 4) // OEM payment
                {
                    invoice.Deal.OemPaymentStatus = invoice.PaymentStatus;
                    invoice.Deal.OemPaymentDate = model.PaymentDate;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment of {model.PaymentAmount:C} recorded successfully!";
            return RedirectToAction("Index");
        }

        // Simple Delete Invoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var invoice = await _context.CbmsInvoices.FindAsync(id);
            if (invoice == null)
            {
                TempData["ErrorMessage"] = "Invoice not found.";
                return RedirectToAction("Index");
            }

            // Check if invoice has payments
            if ((invoice.PaymentReceived ?? 0) > 0)
            {
                TempData["ErrorMessage"] = "Cannot delete invoice with recorded payments.";
                return RedirectToAction("Index");
            }

            _context.CbmsInvoices.Remove(invoice);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Invoice {invoice.InvoiceNumber} deleted successfully.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableDeals(string invoiceType)
        {
            // Determine which deals are available based on invoice type
            var deals = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .Where(d => !_context.CbmsInvoices.Any(i => i.DealId == d.DealId && i.InvoiceType == invoiceType))
                .Select(d => new
                {
                    Value = d.DealId,
                    Text = $"{d.DealName} - {d.Product.ProductName}",
                    Customer = d.Company.CompanyName,
                    Product = d.Product.ProductName,
                    Oem = d.Oem.OemName
                })
                .ToListAsync();

            return Json(deals);
        }

        private string GenerateInvoiceNumber(string invoiceType)
        {
            var prefix = invoiceType == "Customer_To_Canarys" ? "CUST" : "OEM";
            var date = DateTime.Now.ToString("yyyyMM");
            var count = _context.CbmsInvoices.Count(i => i.InvoiceType == invoiceType &&
                                                   i.InvoiceDate.Year == DateTime.Now.Year &&
                                                   i.InvoiceDate.Month == DateTime.Now.Month) + 1;
            return $"{prefix}-{date}-{count:D3}";
        }
    }
}
