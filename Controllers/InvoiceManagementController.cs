using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using License_Tracking.ViewModels;
using System.ComponentModel.DataAnnotations;
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

        // Multi-Invoice Dashboard with Phase Mapping
        public async Task<IActionResult> Index(string phase = "all", string status = "all")
        {
            var invoicesQuery = _context.Invoices
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Company)
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Oem)
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Product)
                .AsQueryable();

            // Filter by phase
            switch (phase.ToLower())
            {
                case "phase1":
                    invoicesQuery = invoicesQuery.Where(i => i.InvoiceType == InvoiceType.Customer);
                    break;
                case "phase4":
                    invoicesQuery = invoicesQuery.Where(i => i.InvoiceType == InvoiceType.OEM);
                    break;
            }

            // Filter by status
            if (status != "all")
            {
                invoicesQuery = invoicesQuery.Where(i => i.PaymentStatus == status);
            }

            var invoices = await invoicesQuery.OrderByDescending(i => i.InvoiceDate).ToListAsync();

            // Calculate summary statistics
            ViewBag.TotalInvoices = invoices.Count;
            ViewBag.TotalAmount = invoices.Sum(i => i.Amount);
            ViewBag.PendingAmount = invoices.Where(i => i.PaymentStatus == "Pending").Sum(i => i.Amount);
            ViewBag.PaidAmount = invoices.Where(i => i.PaymentStatus == "Completed").Sum(i => i.AmountReceived);

            ViewBag.CurrentPhase = phase;
            ViewBag.CurrentStatus = status;

            return View(invoices);
        }

        // Generate Invoice for Deal (Phase 1 - Customer Invoice)
        [HttpGet]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> GenerateCustomerInvoice(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            var viewModel = new InvoiceGenerationViewModel
            {
                DealId = dealId,
                InvoiceType = InvoiceType.Customer,
                Amount = deal.CustomerInvoiceAmount ?? 0,
                DueDate = DateTime.Now.AddDays(30),
                InvoiceDate = DateTime.Now,
                Deal = deal
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> GenerateCustomerInvoice(InvoiceGenerationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Generate invoice number
                var invoiceCount = await _context.Invoices.CountAsync() + 1;
                var invoiceNumber = $"CBMS-CUST-{DateTime.Now:yyyyMM}-{invoiceCount:D4}";

                var invoice = new Invoice
                {
                    DealId = viewModel.DealId,
                    InvoiceNumber = invoiceNumber,
                    InvoiceType = InvoiceType.Customer,
                    Amount = viewModel.Amount,
                    DueDate = viewModel.DueDate,
                    InvoiceDate = viewModel.InvoiceDate ?? DateTime.Now,
                    PaymentDate = viewModel.PaymentDate,
                    PaymentStatus = viewModel.PaymentStatus,
                    Notes = viewModel.Notes
                };

                _context.Add(invoice);

                // PHASE MAPPING FIX: Generate corresponding CbmsInvoice with Phase 1 mapping
                var cbmsInvoice = new CbmsInvoice
                {
                    DealId = invoice.DealId,
                    InvoiceType = "Customer_To_Canarys", // Phase 1 invoice type
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate ?? DateTime.Now,
                    DueDate = invoice.DueDate,
                    Amount = invoice.Amount,
                    PaymentStatus = invoice.PaymentStatus,
                    BusinessPhase = 1, // Phase 1 - Customer Engagement
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity?.Name
                };
                _context.CbmsInvoices.Add(cbmsInvoice);

                // Update deal status
                var relatedDeal = await _context.Deals.FindAsync(invoice.DealId);
                if (relatedDeal != null)
                {
                    relatedDeal.CustomerPaymentStatus = "Invoice Sent";
                    relatedDeal.LastModifiedDate = DateTime.Now;
                    relatedDeal.LastModifiedBy = User.Identity?.Name;
                    _context.Update(relatedDeal);
                }

                // Update deal with invoice information
                var deal = await _context.Deals.FindAsync(invoice.DealId);
                if (deal != null)
                {
                    deal.CustomerInvoiceNumber = invoice.InvoiceNumber;
                    deal.CustomerInvoiceAmount = invoice.Amount;
                    deal.CustomerPaymentStatus = "Invoice Sent";
                    deal.LastModifiedDate = DateTime.Now;
                    deal.LastModifiedBy = User.Identity?.Name;
                    _context.Update(deal);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Customer invoice generated successfully with Phase 1 mapping!";
                return RedirectToAction(nameof(Index));
            }

            // If model is not valid, reload the deal data for the view
            var dealData = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .FirstOrDefaultAsync(d => d.DealId == viewModel.DealId);

            viewModel.Deal = dealData;
            return View(viewModel);
        }

        // Generate OEM Invoice (Phase 4 - OEM Settlement)
        [HttpGet]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> GenerateOemInvoice(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            var viewModel = new InvoiceGenerationViewModel
            {
                DealId = dealId,
                InvoiceType = InvoiceType.OEM,
                Amount = deal.OemInvoiceAmount ?? 0,
                DueDate = DateTime.Now.AddDays(30),
                InvoiceDate = DateTime.Now,
                Deal = deal
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> GenerateOemInvoice(InvoiceGenerationViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                // Generate invoice number
                var invoiceCount = await _context.Invoices.CountAsync() + 1;
                var invoiceNumber = $"CBMS-OEM-{DateTime.Now:yyyyMM}-{invoiceCount:D4}";

                var invoice = new Invoice
                {
                    DealId = viewModel.DealId,
                    InvoiceNumber = invoiceNumber,
                    InvoiceType = InvoiceType.OEM,
                    Amount = viewModel.Amount,
                    DueDate = viewModel.DueDate,
                    InvoiceDate = viewModel.InvoiceDate ?? DateTime.Now,
                    PaymentDate = viewModel.PaymentDate,
                    PaymentStatus = viewModel.PaymentStatus,
                    Notes = viewModel.Notes
                };

                _context.Add(invoice);

                // PHASE MAPPING FIX: Generate corresponding CbmsInvoice with Phase 4 mapping
                var cbmsInvoice = new CbmsInvoice
                {
                    DealId = invoice.DealId,
                    InvoiceType = "OEM_To_Canarys", // Phase 4 invoice type
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate ?? DateTime.Now,
                    DueDate = invoice.DueDate,
                    Amount = invoice.Amount,
                    PaymentStatus = invoice.PaymentStatus,
                    BusinessPhase = 4, // Phase 4 - OEM Settlement
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity?.Name
                };
                _context.CbmsInvoices.Add(cbmsInvoice);

                // Update deal with OEM invoice information
                var deal = await _context.Deals.FindAsync(invoice.DealId);
                if (deal != null)
                {
                    deal.OemInvoiceNumber = invoice.InvoiceNumber;
                    deal.OemInvoiceAmount = invoice.Amount;
                    deal.OemPaymentStatus = "Invoice Received";
                    deal.LastModifiedDate = DateTime.Now;
                    deal.LastModifiedBy = User.Identity?.Name;
                    _context.Update(deal);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "OEM invoice generated successfully with Phase 4 mapping!";
                return RedirectToAction(nameof(Index));
            }

            // If model is not valid, reload the deal data for the view
            var dealData = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .FirstOrDefaultAsync(d => d.DealId == viewModel.DealId);

            viewModel.Deal = dealData;
            return View(viewModel);
        }

        // Generate Canarys to OEM Purchase Order Invoice (Phase 2 - OEM Procurement)
        [HttpGet]
        [Authorize(Roles = "Admin,Finance,Operations")]
        public async Task<IActionResult> GenerateCararysToPOInvoice(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            var invoice = new Invoice
            {
                DealId = dealId,
                InvoiceNumber = "", // Will be set when form is submitted
                InvoiceType = InvoiceType.OEM, // Using existing enum value
                Amount = deal.OemQuoteAmount ?? 0,
                DueDate = DateTime.Now.AddDays(30),
                InvoiceDate = DateTime.Now,
                Deal = deal
            };

            return View(invoice);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance,Operations")]
        public async Task<IActionResult> GenerateCararysToPOInvoice(Invoice invoice)
        {
            if (ModelState.IsValid)
            {
                // Generate PO invoice number
                var invoiceCount = await _context.Invoices.CountAsync() + 1;
                invoice.InvoiceNumber = $"CBMS-PO-{DateTime.Now:yyyyMM}-{invoiceCount:D4}";
                invoice.InvoiceType = InvoiceType.OEM;
                invoice.PaymentStatus = "Pending";

                _context.Add(invoice);

                // PHASE MAPPING FIX: Generate corresponding CbmsInvoice with Phase 2 mapping
                var cbmsInvoice = new CbmsInvoice
                {
                    DealId = invoice.DealId,
                    InvoiceType = "Canarys_To_OEM", // Phase 2 invoice type
                    InvoiceNumber = invoice.InvoiceNumber,
                    InvoiceDate = invoice.InvoiceDate ?? DateTime.Now,
                    DueDate = invoice.DueDate,
                    Amount = invoice.Amount,
                    PaymentStatus = invoice.PaymentStatus,
                    BusinessPhase = 2, // Phase 2 - OEM Procurement
                    CreatedDate = DateTime.Now,
                    CreatedBy = User.Identity?.Name
                };
                _context.CbmsInvoices.Add(cbmsInvoice);

                // Update deal with PO information
                var deal = await _context.Deals.FindAsync(invoice.DealId);
                if (deal != null)
                {
                    deal.CanarysPoNumber = invoice.InvoiceNumber;
                    deal.CanarysPoDate = DateTime.Now;
                    deal.OemQuoteAmount = invoice.Amount;
                    deal.LastModifiedDate = DateTime.Now;
                    deal.LastModifiedBy = User.Identity?.Name;
                    _context.Update(deal);
                }

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Canarys PO invoice generated successfully with Phase 2 mapping!";
                return RedirectToAction(nameof(Index));
            }

            return View(invoice);
        }

        // Record Payment for Invoice
        [HttpGet]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> RecordPayment(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Company)
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Oem)
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Product)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var paymentModel = new PaymentRecordViewModel
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                TotalAmount = invoice.Amount,
                PreviouslyPaid = invoice.AmountReceived,
                RemainingAmount = invoice.Amount - invoice.AmountReceived,
                PaymentDate = DateTime.Now
            };

            return View(paymentModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> RecordPayment(PaymentRecordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Deal)
                    .FirstOrDefaultAsync(i => i.InvoiceId == model.InvoiceId);

                if (invoice == null)
                {
                    return NotFound();
                }

                // Update invoice payment information
                invoice.AmountReceived += model.PaymentAmount;
                invoice.PaymentDate = model.PaymentDate;

                // Update payment status
                if (invoice.AmountReceived >= invoice.Amount)
                {
                    invoice.PaymentStatus = "Completed";
                }
                else if (invoice.AmountReceived > 0)
                {
                    invoice.PaymentStatus = "Partial";
                }

                // Update related deal payment status
                if (invoice.InvoiceType == InvoiceType.Customer)
                {
                    invoice.Deal.CustomerPaymentStatus = invoice.PaymentStatus;
                    invoice.Deal.CustomerPaymentDate = model.PaymentDate;
                }
                else
                {
                    invoice.Deal.OemPaymentStatus = invoice.PaymentStatus;
                    invoice.Deal.OemPaymentDate = model.PaymentDate;
                }

                invoice.Deal.LastModifiedDate = DateTime.Now;
                invoice.Deal.LastModifiedBy = User.Identity?.Name;

                _context.Update(invoice);
                _context.Update(invoice.Deal);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Payment recorded successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // Invoice Details View
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Deal)
                .ThenInclude(d => d.Company)
                .Include(i => i.Deal)
                .ThenInclude(d => d.Oem)
                .Include(i => i.Deal)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(m => m.InvoiceId == id);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // API endpoint for invoice status updates
        [HttpPost]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<JsonResult> UpdateInvoiceStatus(int invoiceId, string status)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.Deal)
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                if (invoice == null)
                {
                    return Json(new { success = false, message = "Invoice not found" });
                }

                invoice.PaymentStatus = status;

                // Update corresponding deal status
                if (invoice.InvoiceType == InvoiceType.Customer)
                {
                    invoice.Deal.CustomerPaymentStatus = status;
                }
                else
                {
                    invoice.Deal.OemPaymentStatus = status;
                }

                invoice.Deal.LastModifiedDate = DateTime.Now;
                invoice.Deal.LastModifiedBy = User.Identity?.Name;

                _context.Update(invoice);
                _context.Update(invoice.Deal);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Invoice status updated successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Invoice Aging Summary - Migrated from BillingController
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> AgingSummary(string invoiceType = "All")
        {
            var today = DateTime.Today;
            var query = _context.Invoices
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Company)
                .Include(i => i.Deal)
                .ThenInclude(d => d!.Product)
                .AsQueryable();

            if (invoiceType != "All")
            {
                if (Enum.TryParse<InvoiceType>(invoiceType, out var parsedType))
                {
                    query = query.Where(i => i.InvoiceType == parsedType);
                }
            }

            var agingSummary = await query
                .Where(i => i.PaymentStatus != "Completed")
                .Select(i => new
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceType = i.InvoiceType.ToString(),
                    ClientName = i.Deal != null && i.Deal.Company != null ? i.Deal.Company.CompanyName : "Unknown",
                    ProductName = i.Deal != null && i.Deal.Product != null ? i.Deal.Product.ProductName : "Pipeline Project",
                    Amount = i.Amount,
                    AmountReceived = i.AmountReceived,
                    OutstandingAmount = i.Amount - i.AmountReceived,
                    DueDate = i.DueDate,
                    DaysOverdue = (int)(today - i.DueDate).TotalDays,
                    PaymentStatus = i.PaymentStatus,
                    AgingCategory = (int)(today - i.DueDate).TotalDays <= 0 ? "Current" :
                                   (int)(today - i.DueDate).TotalDays <= 30 ? "1-30 Days" :
                                   (int)(today - i.DueDate).TotalDays <= 60 ? "31-60 Days" :
                                   (int)(today - i.DueDate).TotalDays <= 90 ? "61-90 Days" : "90+ Days"
                })
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            ViewBag.InvoiceTypeFilter = invoiceType;
            ViewBag.InvoiceTypes = new SelectList(new[] { "All", "Customer", "OEM" }, invoiceType);

            return View(agingSummary);
        }

        // Enhanced Payment Status Update - Migrated from BillingController
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> UpdatePaymentStatus(int invoiceId, string paymentStatus, decimal? amountReceived)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Deal)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            invoice.PaymentStatus = paymentStatus;
            if (amountReceived.HasValue)
            {
                invoice.AmountReceived = amountReceived.Value;

                // If fully paid, mark as completed and set payment date
                if (invoice.AmountReceived >= invoice.Amount)
                {
                    invoice.PaymentStatus = "Completed";
                    invoice.PaymentDate = DateTime.Now;
                }
            }

            // Update corresponding deal status
            if (invoice.Deal != null)
            {
                if (invoice.InvoiceType == InvoiceType.Customer)
                {
                    invoice.Deal.CustomerPaymentStatus = invoice.PaymentStatus;
                    // Note: AmountReceived is calculated, not directly assignable
                }
                else
                {
                    invoice.Deal.OemPaymentStatus = invoice.PaymentStatus;
                }

                invoice.Deal.LastModifiedDate = DateTime.Now;
                invoice.Deal.LastModifiedBy = User.Identity?.Name;
                _context.Update(invoice.Deal);
            }

            _context.Update(invoice);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment status updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Auto-generate Customer Invoice from Deal - Enhanced from BillingController
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> AutoGenerateCustomerInvoice(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                TempData["ErrorMessage"] = "Deal not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if Customer invoice already exists
            var existingCustomerInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.DealId == dealId && i.InvoiceType == InvoiceType.Customer);

            if (existingCustomerInvoice != null)
            {
                TempData["ErrorMessage"] = "Customer invoice already exists for this deal.";
                return RedirectToAction(nameof(Index));
            }

            // Generate invoice number
            var invoiceCount = await _context.Invoices.CountAsync() + 1;
            var invoiceNumber = $"CBMS-CUST-{DateTime.Now:yyyyMM}-{invoiceCount:D4}";

            var customerInvoice = new Invoice
            {
                DealId = dealId,
                InvoiceNumber = invoiceNumber,
                InvoiceType = InvoiceType.Customer,
                Amount = deal.CustomerInvoiceAmount ?? 0,
                PaymentStatus = (deal.AmountReceived ?? 0) >= (deal.CustomerInvoiceAmount ?? 0) ? "Completed" : "Pending",
                DueDate = DateTime.Today.AddDays(30), // Default 30 days
                InvoiceDate = DateTime.Now,
                AmountReceived = deal.AmountReceived ?? 0
            };

            _context.Invoices.Add(customerInvoice);

            // Generate corresponding CbmsInvoice with Phase 1 mapping
            var cbmsInvoice = new CbmsInvoice
            {
                DealId = customerInvoice.DealId,
                InvoiceType = "Customer_To_Canarys", // Phase 1 invoice type
                InvoiceNumber = customerInvoice.InvoiceNumber,
                InvoiceDate = customerInvoice.InvoiceDate ?? DateTime.Now,
                DueDate = customerInvoice.DueDate,
                Amount = customerInvoice.Amount,
                PaymentStatus = customerInvoice.PaymentStatus,
                BusinessPhase = 1, // Phase 1 - Customer Engagement
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name
            };
            _context.CbmsInvoices.Add(cbmsInvoice);

            // Update deal with invoice information
            deal.CustomerInvoiceNumber = customerInvoice.InvoiceNumber;
            deal.CustomerInvoiceAmount = customerInvoice.Amount;
            deal.CustomerPaymentStatus = "Invoice Sent";
            deal.LastModifiedDate = DateTime.Now;
            deal.LastModifiedBy = User.Identity?.Name;
            _context.Update(deal);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer invoice auto-generated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Auto-generate OEM Invoice from Deal - Enhanced from BillingController
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> AutoGenerateOemInvoice(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                TempData["ErrorMessage"] = "Deal not found.";
                return RedirectToAction(nameof(Index));
            }

            // Check if OEM invoice already exists
            var existingOemInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.DealId == dealId && i.InvoiceType == InvoiceType.OEM);

            if (existingOemInvoice != null)
            {
                TempData["ErrorMessage"] = "OEM invoice already exists for this deal.";
                return RedirectToAction(nameof(Index));
            }

            // Generate invoice number
            var invoiceCount = await _context.Invoices.CountAsync() + 1;
            var invoiceNumber = $"CBMS-OEM-{DateTime.Now:yyyyMM}-{invoiceCount:D4}";

            var oemInvoice = new Invoice
            {
                DealId = dealId,
                InvoiceNumber = invoiceNumber,
                InvoiceType = InvoiceType.OEM,
                Amount = deal.OemQuoteAmount ?? 0,
                PaymentStatus = deal.OemPaymentStatus == "Paid" ? "Completed" : "Pending",
                DueDate = DateTime.Today.AddDays(30), // Default 30 days
                InvoiceDate = DateTime.Now,
                AmountReceived = deal.OemPaymentStatus == "Paid" ? deal.OemQuoteAmount ?? 0 : 0
            };

            _context.Invoices.Add(oemInvoice);

            // Generate corresponding CbmsInvoice with Phase 4 mapping
            var cbmsInvoice = new CbmsInvoice
            {
                DealId = oemInvoice.DealId,
                InvoiceType = "OEM_To_Canarys", // Phase 4 invoice type
                InvoiceNumber = oemInvoice.InvoiceNumber,
                InvoiceDate = oemInvoice.InvoiceDate ?? DateTime.Now,
                DueDate = oemInvoice.DueDate,
                Amount = oemInvoice.Amount,
                PaymentStatus = oemInvoice.PaymentStatus,
                BusinessPhase = 4, // Phase 4 - OEM Settlement
                CreatedDate = DateTime.Now,
                CreatedBy = User.Identity?.Name
            };
            _context.CbmsInvoices.Add(cbmsInvoice);

            // Update deal with OEM invoice information
            deal.OemInvoiceNumber = oemInvoice.InvoiceNumber;
            deal.OemInvoiceAmount = oemInvoice.Amount;
            deal.OemPaymentStatus = "Invoice Received";
            deal.LastModifiedDate = DateTime.Now;
            deal.LastModifiedBy = User.Identity?.Name;
            _context.Update(deal);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "OEM invoice auto-generated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Billing Dashboard Summary - Migrated from BillingController
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> BillingDashboard()
        {
            var summary = new
            {
                TotalCustomerInvoices = await _context.Invoices.Where(i => i.InvoiceType == InvoiceType.Customer).CountAsync(),
                TotalOemInvoices = await _context.Invoices.Where(i => i.InvoiceType == InvoiceType.OEM).CountAsync(),
                PendingCustomerAmount = await _context.Invoices
                    .Where(i => i.InvoiceType == InvoiceType.Customer && i.PaymentStatus != "Completed")
                    .SumAsync(i => i.Amount - i.AmountReceived),
                PendingOemAmount = await _context.Invoices
                    .Where(i => i.InvoiceType == InvoiceType.OEM && i.PaymentStatus != "Completed")
                    .SumAsync(i => i.Amount - i.AmountReceived),
                OverdueCustomerInvoices = await _context.Invoices
                    .Where(i => i.InvoiceType == InvoiceType.Customer && i.DueDate < DateTime.Today && i.PaymentStatus != "Completed")
                    .CountAsync(),
                OverdueOemInvoices = await _context.Invoices
                    .Where(i => i.InvoiceType == InvoiceType.OEM && i.DueDate < DateTime.Today && i.PaymentStatus != "Completed")
                    .CountAsync()
            };

            return View(summary);
        }
    }

    // ViewModel for Payment Recording
    public class PaymentRecordViewModel
    {
        public int InvoiceId { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Display(Name = "Total Invoice Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Previously Paid")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal PreviouslyPaid { get; set; }

        [Display(Name = "Remaining Amount")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal RemainingAmount { get; set; }

        [Required]
        [Display(Name = "Payment Amount")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Payment amount must be greater than 0")]
        public decimal PaymentAmount { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; }

        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; } = "Bank Transfer";

        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
