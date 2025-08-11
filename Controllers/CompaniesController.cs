using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.AspNetCore.Authorization;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly AppDbContext _context;

        public CompaniesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Companies
        public async Task<IActionResult> Index(string searchString, string companyType, string industry)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["CompanyTypeFilter"] = companyType;
            ViewData["IndustryFilter"] = industry;

            var companies = from c in _context.Companies
                            select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                companies = companies.Where(c => c.CompanyName.Contains(searchString) ||
                                               c.Industry!.Contains(searchString) ||
                                               c.Email!.Contains(searchString));
            }

            if (!string.IsNullOrEmpty(companyType))
            {
                companies = companies.Where(c => c.CompanyType == companyType);
            }

            if (!string.IsNullOrEmpty(industry))
            {
                companies = companies.Where(c => c.Industry == industry);
            }

            companies = companies.Where(c => c.IsActive)
                                .OrderBy(c => c.CompanyName);

            return View(await companies.ToListAsync());
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.ContactPersons)
                .Include(c => c.Deals)
                    .ThenInclude(d => d.Oem)
                .Include(c => c.Deals)
                    .ThenInclude(d => d.Product)
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Oem)
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(m => m.CompanyId == id && m.IsActive);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CompanyName,Industry,Address,Email,ContactNumber,CompanyType")] Company company)
        {
            if (ModelState.IsValid)
            {
                company.CreatedDate = DateTime.Now;
                company.IsActive = true;
                _context.Add(company);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Company created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null || !company.IsActive)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CompanyId,CompanyName,Industry,Address,Email,ContactNumber,CompanyType,CreatedDate,IsActive")] Company company)
        {
            if (id != company.CompanyId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Company updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.CompanyId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.CompanyId == id && m.IsActive);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                // Soft delete
                company.IsActive = false;
                _context.Update(company);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Company archived successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.CompanyId == id && e.IsActive);
        }

        // AJAX method for company search in dropdowns
        [HttpGet]
        public async Task<JsonResult> GetCompanies(string term)
        {
            var companies = await _context.Companies
                .Where(c => c.IsActive && c.CompanyName.Contains(term))
                .Select(c => new { id = c.CompanyId, text = c.CompanyName })
                .Take(10)
                .ToListAsync();

            return Json(companies);
        }

        // ============================================================================
        // PHASE 1 WEEK 3: CUSTOMER MANAGEMENT FEATURES
        // ============================================================================

        // GET: Companies/CustomerProfile/5 - Enhanced profile with tabbed interface
        public async Task<IActionResult> CustomerProfile(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.ContactPersons.Where(cp => cp.IsActive))
                .Include(c => c.Deals)
                    .ThenInclude(d => d.Oem)
                .Include(c => c.Deals)
                    .ThenInclude(d => d.Product)
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Oem)
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(m => m.CompanyId == id && m.IsActive);

            if (company == null)
            {
                return NotFound();
            }

            // For now, pass the company directly until ViewModels issue is resolved
            return View(company);
        }

        // GET: Companies/ContactManagement/5 - Contact management within customer profiles
        public async Task<IActionResult> ContactManagement(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.ContactPersons.Where(cp => cp.IsActive))
                .FirstOrDefaultAsync(c => c.CompanyId == id && c.IsActive);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/AddContact - Add new contact to company
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddContact(ContactPerson contact)
        {
            if (ModelState.IsValid)
            {
                contact.CreatedDate = DateTime.Now;
                contact.CreatedBy = User.Identity?.Name ?? "System";
                contact.IsActive = true;

                _context.ContactPersons.Add(contact);
                await _context.SaveChangesAsync();

                // Log activity
                await LogCompanyActivity(contact.CompanyId, "Contact Added",
                    $"New contact {contact.Name} ({contact.Designation}) was added to the company.");

                TempData["SuccessMessage"] = "Contact added successfully!";
                return RedirectToAction(nameof(ContactManagement), new { id = contact.CompanyId });
            }

            // Reload contacts if validation fails
            var company = await _context.Companies
                .Include(c => c.ContactPersons.Where(cp => cp.IsActive))
                .FirstOrDefaultAsync(c => c.CompanyId == contact.CompanyId);

            return View("ContactManagement", company);
        }

        // GET: Companies/OemRelationships/5 - Customer-OEM relationship tracking
        public async Task<IActionResult> OemRelationships(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Oem)
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(c => c.CompanyId == id && c.IsActive);

            if (company == null)
            {
                return NotFound();
            }

            ViewBag.AvailableOems = await _context.Oems.Where(o => o.IsActive).ToListAsync();
            ViewBag.AvailableProducts = await _context.Products.Where(p => p.IsActive).Include(p => p.Oem).ToListAsync();

            return View(company);
        }

        // GET: Companies/ActivityTracking/5 - Basic customer activity tracking
        public async Task<IActionResult> ActivityTracking(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.CompanyId == id && c.IsActive);

            if (company == null)
            {
                return NotFound();
            }

            ViewBag.Activities = await GetCompanyActivities(company.CompanyId);
            ViewBag.NewActivity = new Activity
            {
                RelatedEntityType = "Company",
                RelatedEntityId = company.CompanyId,
                ActivityDate = DateTime.Now,
                Status = "Pending",
                Priority = "Medium"
            };

            return View(company);
        }

        // POST: Companies/LogActivity - Log new activity for company
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LogActivity(Activity activity)
        {
            if (ModelState.IsValid)
            {
                activity.RelatedEntityType = "Company";
                activity.Status = "Completed";
                activity.CreatedBy = User.Identity?.Name ?? "System";
                activity.CompletedBy = User.Identity?.Name ?? "System";
                activity.CompletedDate = DateTime.Now;

                _context.Activities.Add(activity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Activity logged successfully!";
                return RedirectToAction(nameof(ActivityTracking), new { id = activity.RelatedEntityId });
            }

            // Reload activities if validation fails
            var company = await _context.Companies.FindAsync(activity.RelatedEntityId);
            ViewBag.Activities = await GetCompanyActivities(activity.RelatedEntityId);
            return View("ActivityTracking", company);
        }

        // Helper method to get company activities
        private async Task<List<Activity>> GetCompanyActivities(int companyId)
        {
            return await _context.Activities
                .Where(a => a.RelatedEntityType == "Company" && a.RelatedEntityId == companyId)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();
        }

        // Helper method to log company activities
        private async Task LogCompanyActivity(int companyId, string activityType, string description, int? contactPersonId = null)
        {
            var activity = new Activity
            {
                RelatedEntityType = "Company",
                RelatedEntityId = companyId,
                ActivityType = activityType,
                Subject = $"{activityType} - {description.Substring(0, Math.Min(description.Length, 50))}",
                Description = description,
                ActivityDate = DateTime.Now,
                Status = "Completed",
                CreatedBy = User.Identity?.Name ?? "System",
                CompletedBy = User.Identity?.Name ?? "System",
                CompletedDate = DateTime.Now
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();
        }

        // API endpoint for getting OEM products (for dropdown filtering)
        [HttpGet]
        public async Task<JsonResult> GetOemProducts(int oemId)
        {
            var products = await _context.Products
                .Where(p => p.OemId == oemId && p.IsActive)
                .Select(p => new { id = p.ProductId, text = p.ProductName, price = p.UnitPrice, licenseType = p.LicenseType })
                .ToListAsync();

            return Json(products);
        }

        // Quick action endpoints for enhanced customer management
        [HttpPost]
        public async Task<JsonResult> TogglePrimaryContact(int contactId)
        {
            var contact = await _context.ContactPersons.FindAsync(contactId);
            if (contact != null)
            {
                // Remove primary status from other contacts in the same company
                var otherContacts = await _context.ContactPersons
                    .Where(c => c.CompanyId == contact.CompanyId && c.ContactId != contactId)
                    .ToListAsync();

                foreach (var otherContact in otherContacts)
                {
                    otherContact.IsPrimaryContact = false;
                }

                // Toggle primary status for the selected contact
                contact.IsPrimaryContact = !contact.IsPrimaryContact;

                await _context.SaveChangesAsync();

                await LogCompanyActivity(contact.CompanyId, "Contact Updated",
                    $"Primary contact status updated for {contact.Name}.");

                return Json(new { success = true, isPrimary = contact.IsPrimaryContact });
            }

            return Json(new { success = false });
        }

        // GET: Companies/ExportOemRelationships - Reference Data Export
        public async Task<IActionResult> ExportOemRelationships(int companyId, string format = "csv")
        {
            var company = await _context.Companies
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Oem)
                .Include(c => c.CustomerOemProducts)
                    .ThenInclude(cop => cop.Product)
                .FirstOrDefaultAsync(c => c.CompanyId == companyId);

            if (company == null)
            {
                return NotFound();
            }

            var relationships = company.CustomerOemProducts ?? new List<CustomerOemProduct>();

            if (format.ToLower() == "csv")
            {
                return ExportToCsv(company, relationships);
            }
            else if (format.ToLower() == "json")
            {
                return ExportToJson(company, relationships);
            }

            return BadRequest("Unsupported export format. Use 'csv' or 'json'.");
        }

        private IActionResult ExportToCsv(Company company, ICollection<CustomerOemProduct> relationships)
        {
            var csv = new System.Text.StringBuilder();

            // CSV Header
            csv.AppendLine("Customer,OEM Partner,Product Name,License Type,Quantity,Unit Price,Total Value,Relationship Type,Start Date,Service Level,Payment Terms");

            // CSV Data
            foreach (var rel in relationships)
            {
                var unitPrice = rel.Product?.UnitPrice?.ToString("F2") ?? "0.00";
                var totalValue = rel.Product?.UnitPrice.HasValue == true ? (rel.Product.UnitPrice.Value * rel.Quantity).ToString("F2") : "0.00";
                var startDate = rel.RelationshipStartDate?.ToString("yyyy-MM-dd") ?? "";

                csv.AppendLine($"\"{company.CompanyName}\"," +
                              $"\"{rel.Oem?.OemName ?? "Unknown"}\"," +
                              $"\"{rel.Product?.ProductName ?? "Unknown"}\"," +
                              $"\"{rel.Product?.LicenseType ?? "Unknown"}\"," +
                              $"{rel.Quantity}," +
                              $"{unitPrice}," +
                              $"{totalValue}," +
                              $"\"{rel.RelationshipType ?? "Active"}\"," +
                              $"{startDate}," +
                              $"\"{rel.Oem?.ServiceLevel ?? ""}\"," +
                              $"\"{rel.Oem?.PaymentTerms ?? ""}\"");
            }

            var fileName = $"OEM_Relationships_{company.CompanyName}_{DateTime.Now:yyyyMMdd}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", fileName);
        }

        private IActionResult ExportToJson(Company company, ICollection<CustomerOemProduct> relationships)
        {
            var exportData = new
            {
                ExportDate = DateTime.Now,
                Customer = new
                {
                    company.CompanyId,
                    company.CompanyName,
                    company.CompanyType,
                    company.Industry
                },
                OemRelationships = relationships.Select(rel => new
                {
                    RelationshipId = rel.Id,
                    OEM = new
                    {
                        rel.Oem?.OemId,
                        rel.Oem?.OemName,
                        rel.Oem?.ServiceLevel,
                        rel.Oem?.PaymentTerms,
                        rel.Oem?.PerformanceRating,
                        rel.Oem?.ContactEmail,
                        rel.Oem?.ContactNumber
                    },
                    Product = new
                    {
                        rel.Product?.ProductId,
                        rel.Product?.ProductName,
                        rel.Product?.LicenseType,
                        rel.Product?.ProductCategory,
                        UnitPrice = rel.Product?.UnitPrice,
                        TotalValue = rel.Product?.UnitPrice.HasValue == true ? rel.Product.UnitPrice.Value * rel.Quantity : 0
                    },
                    Assignment = new
                    {
                        rel.Quantity,
                        rel.RelationshipType,
                        rel.RelationshipStartDate,
                        rel.AssignedDate,
                        rel.Notes
                    }
                }).ToList(),
                Summary = new
                {
                    TotalOemPartners = relationships.Select(r => r.OemId).Distinct().Count(),
                    TotalProducts = relationships.Count,
                    SubscriptionProducts = relationships.Count(r => r.Product?.LicenseType == "Subscription"),
                    PerpetualProducts = relationships.Count(r => r.Product?.LicenseType == "Perpetual"),
                    TotalValue = relationships.Where(r => r.Product?.UnitPrice.HasValue == true)
                                             .Sum(r => r.Product!.UnitPrice!.Value * r.Quantity)
                }
            };

            var fileName = $"OEM_Relationships_{company.CompanyName}_{DateTime.Now:yyyyMMdd}.json";
            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", fileName);
        }
    }
}
