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
    /// <summary>
    /// ProcurementController handles Phase 2 of the CBMS 4-phase workflow (Canarys → OEM)
    /// 
    /// Business Process Mapping:
    /// - Phase 1: Customer → Canarys (handled by InvoiceManagement with BusinessPhase = 1)
    /// - Phase 2: Canarys → OEM (handled by this controller - OEM procurement & purchase orders)
    /// - Phase 3: License Delivery (handled by Deal management)
    /// - Phase 4: OEM → Canarys Settlement (handled by InvoiceManagement with BusinessPhase = 4)
    /// 
    /// Key Relationships:
    /// - PurchaseOrder entity is specific to Phase 2 OEM procurement
    /// - CbmsInvoice (Invoice Management) handles multi-phase invoicing (BusinessPhase 1-4)
    /// - TrackPayment here is for OEM purchase order payments (Phase 2)
    /// - RecordPayment in InvoiceManagement is for comprehensive invoice payments (all phases)
    /// </summary>
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
                    .ThenInclude(d => d.Product)
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Oem)
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
            var query = _context.PurchaseOrders
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Product)
                .Include(p => p.Deal)
                    .ThenInclude(d => d.Oem)
                .AsQueryable();

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.OemPoNumber.Contains(search) ||
                                   (p.Deal.Product != null && p.Deal.Product.ProductName.Contains(search)) ||
                                   (p.Deal.Oem != null && p.Deal.Oem.OemName.Contains(search)));
            }

            // Apply payment status filter
            if (!string.IsNullOrEmpty(paymentStatus))
            {
                query = query.Where(p => p.PaymentStatus == paymentStatus);
            }

            // Apply OEM filter
            if (!string.IsNullOrEmpty(oemFilter))
            {
                query = query.Where(p => p.Deal.Oem != null && p.Deal.Oem.OemName == oemFilter);
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
                "OemName" => sortOrder == "asc" ? query.OrderBy(p => p.Deal.Oem != null ? p.Deal.Oem.OemName : "") : query.OrderByDescending(p => p.Deal.Oem != null ? p.Deal.Oem.OemName : ""),
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
                .Where(l => l.Oem != null)
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
        public async Task<IActionResult> TrackPayment(int? purchaseOrderId, int dealId, string oemPoNumber,
            decimal oemPoAmount, string oemInvoiceNumber, decimal amountPaid, string paymentStatus,
            DateTime? paymentDate, string? paymentTerms, string? notes)
        {
            try
            {
                // Clear any automatic validation errors for optional fields
                if (ModelState.ContainsKey("notes"))
                {
                    ModelState.Remove("notes");
                }
                if (ModelState.ContainsKey("paymentTerms"))
                {
                    ModelState.Remove("paymentTerms");
                }
                if (ModelState.ContainsKey("paymentDate"))
                {
                    ModelState.Remove("paymentDate");
                }

                // Validate required fields manually
                if (dealId <= 0)
                {
                    ModelState.AddModelError("dealId", "Please select a valid deal/license");
                }

                if (string.IsNullOrWhiteSpace(oemPoNumber))
                {
                    ModelState.AddModelError("oemPoNumber", "OEM PO Number is required");
                }

                if (oemPoAmount <= 0)
                {
                    ModelState.AddModelError("oemPoAmount", "OEM PO Amount must be greater than zero");
                }

                if (string.IsNullOrWhiteSpace(oemInvoiceNumber))
                {
                    ModelState.AddModelError("oemInvoiceNumber", "OEM Invoice Number is required");
                }

                if (amountPaid < 0)
                {
                    ModelState.AddModelError("amountPaid", "Amount Paid cannot be negative");
                }

                if (amountPaid > oemPoAmount)
                {
                    ModelState.AddModelError("amountPaid", "Amount Paid cannot exceed PO Amount");
                }

                // Debug: Log ModelState errors
                if (!ModelState.IsValid)
                {
                    foreach (var error in ModelState.Where(x => x.Value?.Errors.Count > 0))
                    {
                        System.Diagnostics.Debug.WriteLine($"ModelState Error - Key: {error.Key}, Errors: {string.Join(", ", error.Value?.Errors.Select(e => e.ErrorMessage) ?? Array.Empty<string>())}");
                    }
                }

                // If validation fails, reload the form
                if (!ModelState.IsValid)
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

                    if (purchaseOrderId.HasValue)
                    {
                        var existingPO = await _context.PurchaseOrders.Include(p => p.Deal).FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId.Value);
                        ViewBag.IsEdit = true;
                        ViewBag.PurchaseOrder = existingPO;
                    }

                    return View();
                }

                PurchaseOrder? purchaseOrder;

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
                        DealId = dealId,
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
                purchaseOrder.UpdatedDate = DateTime.Now;

                // Automatically determine payment status based on amount paid
                if (amountPaid >= oemPoAmount)
                {
                    purchaseOrder.PaymentStatus = "Paid";
                    purchaseOrder.PaymentDate = paymentDate ?? DateTime.Now;
                }
                else if (amountPaid > 0)
                {
                    purchaseOrder.PaymentStatus = "Partial";
                }
                else
                {
                    purchaseOrder.PaymentStatus = "Pending";
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = purchaseOrderId.HasValue ?
                    $"Payment tracking updated successfully! Status: {purchaseOrder.PaymentStatus}" :
                    $"Payment tracking created successfully! Status: {purchaseOrder.PaymentStatus}";

                return RedirectToAction(nameof(PurchaseOrders));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error tracking payment: " + ex.Message;

                ViewBag.Licenses = await _context.Deals
                    .Include(l => l.Product)
                    .Include(l => l.Company)
                    .Include(l => l.Oem)
                    .Select(l => new SelectListItem
                    {
                        Value = l.DealId.ToString(),
                        Text = $"{(l.Product != null ? l.Product.ProductName : "Unknown")} - {(l.Company != null ? l.Company.CompanyName : "Unknown")} ({(l.Oem != null ? l.Oem.OemName : "Unknown")})"
                    }).ToListAsync();

                if (purchaseOrderId.HasValue)
                {
                    var existingPO = await _context.PurchaseOrders.Include(p => p.Deal).FirstOrDefaultAsync(p => p.PurchaseOrderId == purchaseOrderId.Value);
                    ViewBag.IsEdit = true;
                    ViewBag.PurchaseOrder = existingPO;
                }

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
            var uniqueOems = await _context.Deals
                .Include(l => l.Oem)
                .Where(l => l.Oem != null)
                .Select(l => l.Oem.OemName)
                .Distinct()
                .CountAsync();

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
