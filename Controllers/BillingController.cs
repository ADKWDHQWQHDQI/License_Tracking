using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace License_Tracking.Controllers
{
    [Authorize(Roles = "Admin,Finance")]
    public class BillingController : Controller
    {
        private readonly AppDbContext _context;

        public BillingController(AppDbContext context)
        {
            _context = context;
        }

        // Dashboard for billing overview
        public async Task<IActionResult> Index()
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

        [HttpGet]
        public IActionResult GenerateInvoice()
        {
            ViewBag.Licenses = _context.Deals
                .Include(l => l.Product)
                .Include(l => l.Company)
                .Select(l => new SelectListItem
                {
                    Value = l.DealId.ToString(),
                    Text = $"{(l.Product != null ? l.Product.ProductName : "Unknown")} - {(l.Company != null ? l.Company.CompanyName : "Unknown")}"
                }).ToList();

            ViewBag.ProjectPipelines = _context.ProjectPipelines
                .Select(p => new SelectListItem
                {
                    Value = p.ProjectPipelineId.ToString(),
                    Text = $"{p.ProductName} - {p.ClientName} (Pipeline)"
                }).ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateInvoice(int? dealId, int? projectPipelineId, string invoiceNumber,
            InvoiceType invoiceType, decimal amount, DateTime dueDate, string vendorName, string description)
        {
            if ((dealId == null && projectPipelineId == null) || string.IsNullOrWhiteSpace(invoiceNumber) ||
                amount <= 0 || dueDate == default)
            {
                ViewBag.Licenses = _context.Deals
                    .Include(l => l.Product)
                    .Include(l => l.Company)
                    .Select(l => new SelectListItem
                    {
                        Value = l.DealId.ToString(),
                        Text = $"{(l.Product != null ? l.Product.ProductName : "Unknown")} - {(l.Company != null ? l.Company.CompanyName : "Unknown")}"
                    }).ToList();

                ViewBag.ProjectPipelines = _context.ProjectPipelines
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProjectPipelineId.ToString(),
                        Text = $"{p.ProductName} - {p.ClientName} (Pipeline)"
                    }).ToList();

                ModelState.AddModelError("", "Please fill all required fields correctly.");
                return View();
            }

            try
            {
                var invoice = new Invoice
                {
                    DealId = dealId ?? 0,
                    InvoiceNumber = invoiceNumber,
                    InvoiceType = invoiceType,
                    Amount = amount,
                    PaymentStatus = "Pending",
                    DueDate = dueDate,
                    InvoiceDate = DateTime.Now,
                    VendorName = vendorName ?? "Unknown",
                    Description = description ?? "No description"
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"{invoiceType} invoice generated successfully!";
                return RedirectToAction(nameof(PaymentStatus));
            }
            catch (Exception ex)
            {
                ViewBag.Licenses = _context.Deals
                    .Include(l => l.Product)
                    .Include(l => l.Company)
                    .Select(l => new SelectListItem
                    {
                        Value = l.DealId.ToString(),
                        Text = $"{(l.Product != null ? l.Product.ProductName : "Unknown")} - {(l.Company != null ? l.Company.CompanyName : "Unknown")}"
                    }).ToList();

                ViewBag.ProjectPipelines = _context.ProjectPipelines
                    .Select(p => new SelectListItem
                    {
                        Value = p.ProjectPipelineId.ToString(),
                        Text = $"{p.ProductName} - {p.ClientName} (Pipeline)"
                    }).ToList();

                ModelState.AddModelError("", "Error generating invoice: " + ex.Message);
                return View();
            }
        }

        public async Task<IActionResult> PaymentStatus(string invoiceType = "All")
        {
            var query = _context.Invoices.Include(i => i.Deal).AsQueryable();

            if (invoiceType != "All")
            {
                if (Enum.TryParse<InvoiceType>(invoiceType, out var parsedType))
                {
                    query = query.Where(i => i.InvoiceType == parsedType);
                }
            }

            var invoices = await query
                .Select(i => new
                {
                    InvoiceId = i.InvoiceId,
                    InvoiceNumber = i.InvoiceNumber,
                    InvoiceType = i.InvoiceType.ToString(),
                    ProductName = i.Deal != null && i.Deal.Product != null ? i.Deal.Product.ProductName : "Pipeline Project",
                    ClientName = i.Deal != null && i.Deal.Company != null ? i.Deal.Company.CompanyName : (i.VendorName ?? "Unknown"),
                    VendorName = i.VendorName,
                    Amount = i.Amount,
                    AmountReceived = i.AmountReceived,
                    OutstandingAmount = i.Amount - i.AmountReceived,
                    PaymentStatus = i.PaymentStatus,
                    DueDate = i.DueDate,
                    InvoiceDate = i.InvoiceDate,
                    Description = i.Description,
                    IsOverdue = i.DueDate < DateTime.Today && i.PaymentStatus != "Completed"
                })
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            ViewBag.InvoiceTypeFilter = invoiceType;
            ViewBag.InvoiceTypes = new SelectList(new[] { "All", "Customer", "OEM" }, invoiceType);

            return View(invoices);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int invoiceId, string paymentStatus, decimal? amountReceived)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
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

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Payment status updated successfully!";

            return RedirectToAction(nameof(PaymentStatus));
        }

        public async Task<IActionResult> AgingSummary(string invoiceType = "All")
        {
            var today = DateTime.Today;
            var query = _context.Invoices.Include(i => i.Deal).AsQueryable();

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
                    ClientName = i.Deal != null && i.Deal.Company != null ? i.Deal.Company.CompanyName : (i.VendorName ?? "Unknown"),
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

        // Generate separate OEM invoice for cost tracking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateOemInvoice(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                TempData["ErrorMessage"] = "Deal not found.";
                return RedirectToAction(nameof(PaymentStatus));
            }

            // Check if OEM invoice already exists
            var existingOemInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.DealId == dealId && i.InvoiceType == InvoiceType.OEM);

            if (existingOemInvoice != null)
            {
                TempData["ErrorMessage"] = "OEM invoice already exists for this deal.";
                return RedirectToAction(nameof(PaymentStatus));
            }

            var oemInvoice = new Invoice
            {
                DealId = dealId,
                InvoiceNumber = $"OEM-{deal.DealId}-{DateTime.Now:yyyyMMdd}",
                InvoiceType = InvoiceType.OEM,
                Amount = deal.OemQuoteAmount ?? 0,
                PaymentStatus = deal.OemPaymentStatus == "Paid" ? "Completed" : "Pending",
                DueDate = DateTime.Today.AddDays(30), // Default 30 days
                InvoiceDate = DateTime.Now,
                VendorName = deal.Oem?.OemName ?? "Unknown OEM",
                Description = $"OEM cost for {deal.Product?.ProductName ?? "Unknown Product"}",
                AmountReceived = deal.OemPaymentStatus == "Paid" ? deal.OemQuoteAmount ?? 0 : 0
            };

            _context.Invoices.Add(oemInvoice);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "OEM invoice generated successfully!";
            return RedirectToAction(nameof(PaymentStatus));
        }

        // Generate separate Customer invoice for revenue tracking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateCustomerInvoice(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Product)
                .Include(d => d.Company)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                TempData["ErrorMessage"] = "Deal not found.";
                return RedirectToAction(nameof(PaymentStatus));
            }

            // Check if Customer invoice already exists
            var existingCustomerInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.DealId == dealId && i.InvoiceType == InvoiceType.Customer);

            if (existingCustomerInvoice != null)
            {
                TempData["ErrorMessage"] = "Customer invoice already exists for this deal.";
                return RedirectToAction(nameof(PaymentStatus));
            }

            var customerInvoice = new Invoice
            {
                DealId = dealId,
                InvoiceNumber = $"CUST-{deal.DealId}-{DateTime.Now:yyyyMMdd}",
                InvoiceType = InvoiceType.Customer,
                Amount = deal.CustomerInvoiceAmount ?? 0,
                PaymentStatus = (deal.AmountReceived ?? 0) >= (deal.CustomerInvoiceAmount ?? 0) ? "Completed" : "Pending",
                DueDate = DateTime.Today.AddDays(30), // Default 30 days
                InvoiceDate = DateTime.Now,
                VendorName = deal.Company?.CompanyName ?? "Unknown Company",
                Description = $"License for {deal.Product?.ProductName ?? "Unknown Product"}",
                AmountReceived = deal.AmountReceived ?? 0
            };

            _context.Invoices.Add(customerInvoice);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Customer invoice generated successfully!";
            return RedirectToAction(nameof(PaymentStatus));
        }
    }
}
