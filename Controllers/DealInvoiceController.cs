using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class DealInvoiceController : Controller
    {
        private readonly AppDbContext _context;

        public DealInvoiceController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Deal Invoice Management Dashboard
        public async Task<IActionResult> Index(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .Include(d => d.Invoices)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            ViewBag.Deal = deal;
            return View();
        }

        // POST: Generate Phase-Specific Invoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> GeneratePhaseInvoice(int dealId, string invoiceType, decimal amount, DateTime dueDate, string notes)
        {
            var deal = await _context.Deals.FindAsync(dealId);
            if (deal == null)
            {
                return NotFound();
            }

            // Generate invoice number based on type and phase
            var invoiceNumber = await GenerateInvoiceNumber(invoiceType, dealId);

            var invoice = new CbmsInvoice
            {
                DealId = dealId,
                InvoiceType = invoiceType,
                InvoiceNumber = invoiceNumber,
                InvoiceDate = DateTime.Now,
                DueDate = dueDate,
                Amount = amount,
                TotalAmount = amount, // Add tax calculation if needed
                PaymentStatus = "Unpaid",
                Reference = notes,
                CreatedBy = User.Identity?.Name
            };

            _context.CbmsInvoices.Add(invoice);

            // Update deal fields based on invoice type
            switch (invoiceType)
            {
                case "Customer_To_Canarys":
                    deal.CustomerInvoiceNumber = invoiceNumber;
                    deal.CustomerInvoiceAmount = amount;
                    deal.CustomerPaymentStatus = "Pending";
                    break;
                case "OEM_To_Canarys":
                    deal.OemInvoiceNumber = invoiceNumber;
                    deal.OemInvoiceAmount = amount;
                    deal.OemPaymentStatus = "Pending";
                    break;
            }

            deal.LastModifiedDate = DateTime.Now;
            deal.LastModifiedBy = User.Identity?.Name;

            // Create activity for invoice generation
            var activity = new DealCollaborationActivity
            {
                DealId = dealId,
                ActivityType = "Invoice_Generated",
                ActivityTitle = $"{invoiceType.Replace("_", " ")} Invoice Generated",
                ActivityDescription = $"Invoice {invoiceNumber} generated for {amount:C}. Due date: {dueDate:yyyy-MM-dd}",
                PerformedBy = User.Identity?.Name,
                CreatedBy = User.Identity?.Name,
                Status = "Completed"
            };

            _context.DealActivities.Add(activity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Invoice {invoiceNumber} generated successfully!";
            return RedirectToAction(nameof(Index), new { dealId });
        }

        // POST: Record Payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Finance")]
        public async Task<IActionResult> RecordPayment(int invoiceId, decimal paymentAmount, DateTime paymentDate, string paymentMethod, string reference)
        {
            var invoice = await _context.CbmsInvoices
                .Include(i => i.Deal)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            // Update invoice payment status
            invoice.PaymentDate = paymentDate;
            invoice.PaymentReceived = paymentAmount;

            if (paymentAmount >= invoice.TotalAmount)
            {
                invoice.PaymentStatus = "Paid";
            }
            else if (paymentAmount > 0)
            {
                invoice.PaymentStatus = "Partial";
            }

            // Update deal payment status
            if (invoice.InvoiceType == "Customer_To_Canarys")
            {
                invoice.Deal!.CustomerPaymentStatus = invoice.PaymentStatus;
                invoice.Deal.CustomerPaymentDate = paymentDate;
            }
            else if (invoice.InvoiceType == "OEM_To_Canarys")
            {
                invoice.Deal!.OemPaymentStatus = invoice.PaymentStatus;
                invoice.Deal.OemPaymentDate = paymentDate;
            }

            invoice.Deal!.LastModifiedDate = DateTime.Now;
            invoice.Deal.LastModifiedBy = User.Identity?.Name;

            // Create activity for payment
            var activity = new DealCollaborationActivity
            {
                DealId = invoice.DealId,
                ActivityType = "Payment_Received",
                ActivityTitle = $"Payment Recorded - {invoice.InvoiceNumber}",
                ActivityDescription = $"Payment of {paymentAmount:C} recorded for invoice {invoice.InvoiceNumber}. Method: {paymentMethod}. Reference: {reference}",
                PerformedBy = User.Identity?.Name,
                CreatedBy = User.Identity?.Name,
                Status = "Completed"
            };

            _context.DealActivities.Add(activity);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Payment of {paymentAmount:C} recorded successfully!";
            return RedirectToAction(nameof(Index), new { dealId = invoice.DealId });
        }

        // GET: Invoice Details
        public async Task<IActionResult> InvoiceDetails(int invoiceId)
        {
            var invoice = await _context.CbmsInvoices
                .Include(i => i.Deal)
                .ThenInclude(d => d.Company)
                .Include(i => i.Deal)
                .ThenInclude(d => d.Oem)
                .Include(i => i.Deal)
                .ThenInclude(d => d.Product)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // GET: Multi-Invoice Dashboard for All Deals
        public async Task<IActionResult> MultiInvoiceDashboard(string phase = "all", string status = "all")
        {
            var invoicesQuery = _context.CbmsInvoices
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
                    invoicesQuery = invoicesQuery.Where(i => i.InvoiceType == "Customer_To_Canarys");
                    break;
                case "phase2":
                    invoicesQuery = invoicesQuery.Where(i => i.InvoiceType == "Canarys_To_OEM");
                    break;
                case "phase4":
                    invoicesQuery = invoicesQuery.Where(i => i.InvoiceType == "OEM_To_Canarys");
                    break;
            }

            // Filter by status
            if (status != "all")
            {
                invoicesQuery = invoicesQuery.Where(i => i.PaymentStatus == status);
            }

            var invoices = await invoicesQuery.OrderByDescending(i => i.InvoiceDate).ToListAsync();

            // Calculate dashboard metrics
            var metrics = new InvoiceDashboardMetrics
            {
                TotalInvoices = invoices.Count,
                TotalAmount = invoices.Sum(i => i.TotalAmount),
                PaidAmount = invoices.Where(i => i.PaymentStatus == "Paid").Sum(i => i.PaymentReceived ?? 0),
                PendingAmount = invoices.Where(i => i.PaymentStatus == "Unpaid" || i.PaymentStatus == "Partial").Sum(i => i.TotalAmount - (i.PaymentReceived ?? 0)),
                OverdueCount = invoices.Count(i => i.DueDate < DateTime.Now && i.PaymentStatus != "Paid"),
                Phase1Count = invoices.Count(i => i.InvoiceType == "Customer_To_Canarys"),
                Phase2Count = invoices.Count(i => i.InvoiceType == "Canarys_To_OEM"),
                Phase4Count = invoices.Count(i => i.InvoiceType == "OEM_To_Canarys")
            };

            ViewBag.Metrics = metrics;
            ViewBag.CurrentPhase = phase;
            ViewBag.CurrentStatus = status;

            return View(invoices);
        }

        // GET: Phase Summary Report
        public async Task<IActionResult> PhaseSummaryReport()
        {
            var phaseSummary = await _context.Deals
                .Include(d => d.Invoices)
                .Where(d => d.IsActive)
                .GroupBy(d => d.CurrentPhase)
                .Select(g => new PhaseSummaryViewModel
                {
                    Phase = g.Key ?? "Unknown",
                    DealCount = g.Count(),
                    TotalValue = g.Sum(d => d.CustomerInvoiceAmount ?? 0),
                    InvoicesGenerated = g.SelectMany(d => d.Invoices).Count(),
                    PaidInvoices = g.SelectMany(d => d.Invoices).Count(i => i.PaymentStatus == "Paid"),
                    PendingPayments = g.SelectMany(d => d.Invoices).Where(i => i.PaymentStatus != "Paid").Sum(i => i.TotalAmount - (i.PaymentReceived ?? 0))
                })
                .ToListAsync();

            return View(phaseSummary);
        }

        // Helper method to generate invoice numbers
        private async Task<string> GenerateInvoiceNumber(string invoiceType, int dealId)
        {
            var prefix = invoiceType switch
            {
                "Customer_To_Canarys" => "CNR-C",
                "Canarys_To_OEM" => "CNR-O",
                "OEM_To_Canarys" => "OEM-C",
                _ => "INV"
            };

            var count = await _context.CbmsInvoices.CountAsync(i => i.InvoiceType == invoiceType);
            return $"{prefix}-{DateTime.Now:yyyyMM}-{(count + 1):D4}-D{dealId}";
        }

        // GET: Export Invoice Report
        public async Task<IActionResult> ExportInvoiceReport(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.Invoices)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Invoice Report - Deal: {deal.DealName}");
            csv.AppendLine($"Company: {deal.Company?.CompanyName}");
            csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            csv.AppendLine("");
            csv.AppendLine("Invoice Number,Type,Date,Due Date,Amount,Payment Status,Payment Date,Payment Amount");

            foreach (var invoice in deal.Invoices)
            {
                csv.AppendLine($"\"{invoice.InvoiceNumber}\",\"{invoice.InvoiceType}\",\"{invoice.InvoiceDate:yyyy-MM-dd}\",\"{invoice.DueDate?.ToString("yyyy-MM-dd")}\",{invoice.TotalAmount},\"{invoice.PaymentStatus}\",\"{invoice.PaymentDate?.ToString("yyyy-MM-dd")}\",{invoice.PaymentReceived ?? 0}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Deal_{dealId}_Invoice_Report_{DateTime.Now:yyyyMMdd}.csv");
        }
    }

    // Invoice Dashboard Metrics ViewModel
    public class InvoiceDashboardMetrics
    {
        public int TotalInvoices { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal PendingAmount { get; set; }
        public int OverdueCount { get; set; }
        public int Phase1Count { get; set; }
        public int Phase2Count { get; set; }
        public int Phase4Count { get; set; }

        public decimal CollectionRate => TotalAmount > 0 ? (PaidAmount / TotalAmount) * 100 : 0;
        public decimal OutstandingPercentage => TotalAmount > 0 ? (PendingAmount / TotalAmount) * 100 : 0;
    }

    // Phase Summary ViewModel
    public class PhaseSummaryViewModel
    {
        public string Phase { get; set; } = string.Empty;
        public int DealCount { get; set; }
        public decimal TotalValue { get; set; }
        public int InvoicesGenerated { get; set; }
        public int PaidInvoices { get; set; }
        public decimal PendingPayments { get; set; }

        public decimal PaymentRate => InvoicesGenerated > 0 ? ((decimal)PaidInvoices / InvoicesGenerated) * 100 : 0;
        public string PhaseIcon => Phase switch
        {
            "Phase 1" => "fas fa-handshake",
            "Phase 2" => "fas fa-shopping-cart",
            "Phase 3" => "fas fa-key",
            "Phase 4" => "fas fa-credit-card",
            _ => "fas fa-info-circle"
        };
    }
}
