using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace License_Tracking.Controllers
{
    [Authorize(Roles = "Admin,Operations,Finance")]
    public class ProcurementController : Controller
    {
        private readonly AppDbContext _context;

        public ProcurementController(AppDbContext context)
        {
            _context = context;
        }

        // Dashboard view with OEM analytics
        public async Task<IActionResult> Index()
        {
            var oemSummary = await GetOemSummaryAsync();
            var recentPurchaseOrders = await _context.PurchaseOrders
                .Include(p => p.Deal)
                .OrderByDescending(p => p.PurchaseOrderId)
                .Take(10)
                .ToListAsync();

            ViewBag.OemSummary = oemSummary;
            ViewBag.RecentPurchaseOrders = recentPurchaseOrders;

            return View();
        }

        // Enhanced Purchase Order Management
        public async Task<IActionResult> PurchaseOrders(string search = "", string paymentStatus = "",
            string oemFilter = "", string dateRange = "", string sortBy = "CreatedDate", string sortOrder = "desc")
        {
            var query = _context.PurchaseOrders.Include(p => p.Deal).AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.OemPoNumber.Contains(search) ||
                                   p.Deal.Product.ProductName.Contains(search) ||
                                   p.Deal.Oem.OemName.Contains(search));
            }

            // Apply payment status filter
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(p => p.PaymentStatus == paymentStatus);
            }

            // Apply OEM filter
            if (!string.IsNullOrEmpty(oemFilter))
            {
                query = query.Where(p => p.Deal.Oem.OemName == oemFilter);
            }

            // Apply date range filter
            if (!string.IsNullOrEmpty(dateRange))
            {
                var cutoffDate = dateRange switch
                {
                    "30days" => DateTime.Now.AddDays(-30),
                    "90days" => DateTime.Now.AddDays(-90),
                    "12months" => DateTime.Now.AddMonths(-12),
                    _ => DateTime.MinValue
                };

                if (cutoffDate != DateTime.MinValue)
                {
                    query = query.Where(p => p.CreatedDate >= cutoffDate);
                }
            }

            // Apply sorting
            query = sortBy switch
            {
                "OemPoNumber" => sortOrder == "asc" ? query.OrderBy(p => p.OemPoNumber) : query.OrderByDescending(p => p.OemPoNumber),
                "OemName" => sortOrder == "asc" ? query.OrderBy(p => p.Deal.Oem.OemName) : query.OrderByDescending(p => p.Deal.Oem.OemName),
                "OemPoAmount" => sortOrder == "asc" ? query.OrderBy(p => p.OemPoAmount) : query.OrderByDescending(p => p.OemPoAmount),
                "CreatedDate" => sortOrder == "asc" ? query.OrderBy(p => p.CreatedDate) : query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            var purchaseOrders = await query.ToListAsync();

            // Create summary data
            var summary = new
            {
                TotalPOs = purchaseOrders?.Count ?? 0,
                TotalAmount = purchaseOrders?.Sum(p => p.OemPoAmount) ?? 0,
                PendingAmount = purchaseOrders?.Where(p => p.PaymentStatus != "Paid").Sum(p => p.OemPoAmount - p.AmountPaid) ?? 0,
                AverageAmount = purchaseOrders?.Any() == true ? purchaseOrders.Average(p => p.OemPoAmount) : 0
            };

            // Get available OEMs for filter
            var availableOems = await _context.Deals
                .Include(l => l.Oem)
                .Select(l => l.Oem.OemName)
                .Distinct()
                .ToListAsync();

            // Set ViewBag data
            ViewBag.PurchaseOrders = purchaseOrders ?? new List<PurchaseOrder>();
            ViewBag.Summary = summary;
            ViewBag.CurrentSearch = search ?? "";
            ViewBag.CurrentPaymentStatus = paymentStatus ?? "";
            ViewBag.CurrentOem = oemFilter ?? "";
            ViewBag.CurrentDateRange = dateRange ?? "";
            ViewBag.AvailableOems = availableOems ?? new List<string>();
            ViewBag.TotalCount = purchaseOrders?.Count ?? 0;
            ViewBag.SortBy = sortBy ?? "CreatedDate";
            ViewBag.SortOrder = sortOrder ?? "desc";

            // Legacy ViewBag for compatibility
            ViewBag.StatusFilter = paymentStatus ?? "";
            ViewBag.OemFilter = oemFilter ?? "";
            ViewBag.StatusOptions = new SelectList(new[] { "All", "Pending", "Paid", "Partial", "Overdue" }, paymentStatus);
            ViewBag.OemOptions = new SelectList(
                new[] { "All" }.Concat(availableOems ?? new List<string>()),
                oemFilter);

            return View(purchaseOrders);
        }

        [HttpGet]
        public async Task<IActionResult> TrackPayment(int? id)
        {
            if (id.HasValue)
            {
                var existingPO = await _context.PurchaseOrders.Include(p => p.Deal).FirstOrDefaultAsync(p => p.PurchaseOrderId == id.Value);
                if (existingPO != null)
                {
                    ViewBag.IsEdit = true;
                    ViewBag.PurchaseOrder = existingPO;
                }
            }

            ViewBag.Licenses = await _context.Deals
                .Include(l => l.Product)
                .Include(l => l.Company)
                .Include(l => l.Oem)
                .Select(l => new SelectListItem
                {
                    Value = l.DealId.ToString(),
                    Text = $"{(l.Product != null ? l.Product.ProductName : "Unknown")} - {(l.Company != null ? l.Company.CompanyName : "Unknown")} ({(l.Oem != null ? l.Oem.OemName : "Unknown")})"
                }).ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackPayment(int? purchaseOrderId, int licenseId, string oemPoNumber,
            decimal oemPoAmount, string oemInvoiceNumber, decimal amountPaid, string paymentStatus,
            DateTime? paymentDate, string paymentTerms, string notes)
        {
            try
            {
                PurchaseOrder purchaseOrder;

                if (purchaseOrderId.HasValue)
                {
                    // Update existing PO
                    purchaseOrder = await _context.PurchaseOrders.FindAsync(purchaseOrderId.Value);
                    if (purchaseOrder == null)
                    {
                        TempData["ErrorMessage"] = "Purchase Order not found.";
                        return RedirectToAction(nameof(PurchaseOrders));
                    }
                }
                else
                {
                    // Create new PO
                    purchaseOrder = new PurchaseOrder
                    {
                        DealId = licenseId,
                        CreatedDate = DateTime.Now
                    };
                    _context.PurchaseOrders.Add(purchaseOrder);
                }

                // Update fields
                purchaseOrder.OemPoNumber = oemPoNumber;
                purchaseOrder.OemPoAmount = oemPoAmount;
                purchaseOrder.OemInvoiceNumber = oemInvoiceNumber;
                purchaseOrder.AmountPaid = amountPaid;
                purchaseOrder.PaymentStatus = paymentStatus;
                purchaseOrder.PaymentDate = paymentDate;
                purchaseOrder.PaymentTerms = paymentTerms;
                purchaseOrder.Notes = notes;

                // Update payment status based on amount paid
                if (amountPaid >= oemPoAmount)
                {
                    purchaseOrder.PaymentStatus = "Paid";
                    purchaseOrder.PaymentDate = paymentDate ?? DateTime.Now;
                }
                else if (amountPaid > 0)
                {
                    purchaseOrder.PaymentStatus = "Partial";
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = purchaseOrderId.HasValue ?
                    "Payment tracking updated successfully!" :
                    "Payment tracking created successfully!";

                return RedirectToAction(nameof(PurchaseOrders));
            }
            catch (Exception ex)
            {
                ViewBag.Licenses = await _context.Deals
                    .Include(l => l.Product)
                    .Include(l => l.Company)
                    .Include(l => l.Oem)
                    .Select(l => new SelectListItem
                    {
                        Value = l.DealId.ToString(),
                        Text = $"{(l.Product != null ? l.Product.ProductName : "Unknown")} - {(l.Company != null ? l.Company.CompanyName : "Unknown")} ({(l.Oem != null ? l.Oem.OemName : "Unknown")})"
                    }).ToListAsync();

                ModelState.AddModelError("", "Error tracking payment: " + ex.Message);
                return View();
            }
        }

        private async Task<object> GetOemSummaryAsync()
        {
            var totalPurchaseOrders = await _context.PurchaseOrders.CountAsync();
            var totalCost = await _context.PurchaseOrders.SumAsync(p => p.OemPoAmount);
            var pendingPayments = await _context.PurchaseOrders
                .Where(p => p.PaymentStatus != "Paid")
                .SumAsync(p => p.OemPoAmount - p.AmountPaid);
            var uniqueOems = await _context.Deals.Select(l => l.OemName).Distinct().CountAsync();

            return new
            {
                TotalPurchaseOrders = totalPurchaseOrders,
                TotalCost = totalCost,
                PendingPayments = pendingPayments,
                UniqueOems = uniqueOems
            };
        }
    }
}
