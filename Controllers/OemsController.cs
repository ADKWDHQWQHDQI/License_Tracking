using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class OemsController : Controller
    {
        private readonly AppDbContext _context;

        public OemsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Oems
        public async Task<IActionResult> Index(string searchString, string serviceLevel, string performanceFilter)
        {
            ViewData["CurrentFilter"] = searchString;
            ViewData["ServiceLevelFilter"] = serviceLevel;
            ViewData["PerformanceFilter"] = performanceFilter;

            var oems = from o in _context.Oems
                       .Include(o => o.Products.Where(p => p.IsActive))
                       .Include(o => o.Deals)
                       .Include(o => o.CustomerOemProducts)
                       select o;

            // Search filter
            if (!string.IsNullOrEmpty(searchString))
            {
                oems = oems.Where(o => o.OemName.Contains(searchString) ||
                                     o.ContactEmail!.Contains(searchString) ||
                                     o.ContactNumber!.Contains(searchString));
            }

            // Service level filter
            if (!string.IsNullOrEmpty(serviceLevel))
            {
                oems = oems.Where(o => o.ServiceLevel == serviceLevel);
            }

            // Performance rating filter
            if (!string.IsNullOrEmpty(performanceFilter))
            {
                switch (performanceFilter.ToLower())
                {
                    case "high":
                        oems = oems.Where(o => o.PerformanceRating >= 4.0m);
                        break;
                    case "medium":
                        oems = oems.Where(o => o.PerformanceRating >= 3.0m && o.PerformanceRating < 4.0m);
                        break;
                    case "low":
                        oems = oems.Where(o => o.PerformanceRating < 3.0m && o.PerformanceRating.HasValue);
                        break;
                }
            }

            oems = oems.Where(o => o.IsActive)
                      .OrderByDescending(o => o.ServiceLevel == "Gold")
                      .ThenByDescending(o => o.ServiceLevel == "Silver")
                      .ThenByDescending(o => o.PerformanceRating)
                      .ThenBy(o => o.OemName);

            return View(await oems.ToListAsync());
        }

        // GET: Oems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oem = await _context.Oems
                .Include(o => o.Products)
                .Include(o => o.Deals)
                    .ThenInclude(d => d.Company)
                .FirstOrDefaultAsync(m => m.OemId == id && m.IsActive);

            if (oem == null)
            {
                return NotFound();
            }

            return View(oem);
        }

        // GET: Oems/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Oems/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("OemName,ContactEmail,ContactNumber,PaymentTerms,ServiceLevel,PerformanceRating,Address,Website")] Oem oem)
        {
            if (ModelState.IsValid)
            {
                oem.CreatedDate = DateTime.Now;
                oem.CreatedBy = User.Identity?.Name ?? "System";
                oem.IsActive = true;
                _context.Add(oem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"OEM partner '{oem.OemName}' created successfully!";
                return RedirectToAction(nameof(Index));
            }
            return View(oem);
        }

        // GET: Oems/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oem = await _context.Oems.FindAsync(id);
            if (oem == null || !oem.IsActive)
            {
                return NotFound();
            }
            return View(oem);
        }

        // POST: Oems/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("OemId,OemName,ContactEmail,ContactNumber,PaymentTerms,ServiceLevel,PerformanceRating,Address,Website,CreatedDate,CreatedBy,IsActive")] Oem oem)
        {
            if (id != oem.OemId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(oem);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"OEM partner '{oem.OemName}' updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OemExists(oem.OemId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Details), new { id = oem.OemId });
            }
            return View(oem);
        }

        // GET: Oems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var oem = await _context.Oems
                .FirstOrDefaultAsync(m => m.OemId == id && m.IsActive);
            if (oem == null)
            {
                return NotFound();
            }

            return View(oem);
        }

        // POST: Oems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var oem = await _context.Oems.FindAsync(id);
            if (oem != null)
            {
                // Soft delete
                oem.IsActive = false;
                _context.Update(oem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "OEM archived successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool OemExists(int id)
        {
            return _context.Oems.Any(e => e.OemId == id && e.IsActive);
        }

        // AJAX method for OEM search in dropdowns
        [HttpGet]
        public async Task<JsonResult> GetOems(string term)
        {
            var oems = await _context.Oems
                .Where(o => o.IsActive && o.OemName.Contains(term))
                .Select(o => new { id = o.OemId, text = o.OemName })
                .Take(10)
                .ToListAsync();

            return Json(oems);
        }

        // Get products for a specific OEM
        [HttpGet]
        public async Task<JsonResult> GetProductsByOem(int oemId)
        {
            var products = await _context.Products
                .Where(p => p.OemId == oemId && p.IsActive)
                .Select(p => new
                {
                    id = p.ProductId,
                    text = p.ProductName,
                    price = p.UnitPrice,
                    licenseType = p.LicenseType,
                    category = p.ProductCategory
                })
                .ToListAsync();

            return Json(products);
        }

        // API endpoint for OEM dropdown functionality
        [HttpGet]
        public async Task<JsonResult> GetOemsForDropdown(string term = "")
        {
            var oems = await _context.Oems
                .Where(o => o.IsActive &&
                       (string.IsNullOrEmpty(term) || o.OemName.Contains(term)))
                .Select(o => new
                {
                    id = o.OemId,
                    text = o.OemName,
                    serviceLevel = o.ServiceLevel,
                    paymentTerms = o.PaymentTerms,
                    performanceRating = o.PerformanceRating
                })
                .OrderByDescending(o => o.serviceLevel == "Gold")
                .ThenByDescending(o => o.serviceLevel == "Silver")
                .ThenBy(o => o.text)
                .Take(20)
                .ToListAsync();

            return Json(oems);
        }

        // API endpoint for performance rating update
        [HttpPost]
        public async Task<JsonResult> UpdatePerformanceRating(int oemId, decimal rating)
        {
            try
            {
                var oem = await _context.Oems.FindAsync(oemId);
                if (oem == null || !oem.IsActive)
                {
                    return Json(new { success = false, message = "OEM not found" });
                }

                oem.PerformanceRating = Math.Max(1.0m, Math.Min(5.0m, rating)); // Ensure rating is between 1.0 and 5.0
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Performance rating updated successfully",
                    newRating = oem.PerformanceRating
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // API endpoint for service level update
        [HttpPost]
        public async Task<JsonResult> UpdateServiceLevel(int oemId, string serviceLevel)
        {
            try
            {
                var oem = await _context.Oems.FindAsync(oemId);
                if (oem == null || !oem.IsActive)
                {
                    return Json(new { success = false, message = "OEM not found" });
                }

                var validLevels = new[] { "Gold", "Silver", "Bronze" };
                if (!validLevels.Contains(serviceLevel))
                {
                    return Json(new { success = false, message = "Invalid service level" });
                }

                oem.ServiceLevel = serviceLevel;
                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Service level updated successfully",
                    newServiceLevel = oem.ServiceLevel
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Oems/Export - Reference Data Export
        public async Task<IActionResult> Export(string format = "csv")
        {
            var oems = await _context.Oems
                .Include(o => o.Products.Where(p => p.IsActive))
                .Include(o => o.CustomerOemProducts)
                    .ThenInclude(cop => cop.Company)
                .Where(o => o.IsActive)
                .OrderBy(o => o.OemName)
                .ToListAsync();

            if (format.ToLower() == "csv")
            {
                return ExportToCsv(oems);
            }
            else if (format.ToLower() == "json")
            {
                return ExportToJson(oems);
            }
            else if (format.ToLower() == "excel")
            {
                return ExportToExcel(oems);
            }

            return BadRequest("Unsupported export format. Use 'csv', 'json', or 'excel'.");
        }

        // GET: Oems/Import - Show import form
        public IActionResult Import()
        {
            return View();
        }

        // POST: Oems/Import - Process import file
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to import.";
                return View();
            }

            var fileExtension = Path.GetExtension(file.FileName).ToLower();

            try
            {
                List<Oem> importedOems;

                if (fileExtension == ".csv")
                {
                    importedOems = await ImportFromCsv(file);
                }
                else if (fileExtension == ".json")
                {
                    importedOems = await ImportFromJson(file);
                }
                else
                {
                    TempData["ErrorMessage"] = "Unsupported file format. Please use CSV or JSON files.";
                    return View();
                }

                // Validate and save imported data
                var validationResults = ValidateImportedOems(importedOems);

                if (validationResults.HasErrors)
                {
                    TempData["ErrorMessage"] = $"Import validation failed: {validationResults.ErrorMessage}";
                    return View();
                }

                // Save valid OEMs
                var savedCount = await SaveImportedOems(importedOems);

                TempData["SuccessMessage"] = $"Successfully imported {savedCount} OEM partners. {validationResults.SkippedCount} duplicates were skipped.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Import failed: {ex.Message}";
                return View();
            }
        }

        // GET: Oems/Template - Download import template
        public IActionResult Template(string format = "csv")
        {
            if (format.ToLower() == "csv")
            {
                return DownloadCsvTemplate();
            }
            else if (format.ToLower() == "json")
            {
                return DownloadJsonTemplate();
            }

            return BadRequest("Unsupported template format. Use 'csv' or 'json'.");
        }

        // Private Export Methods
        private IActionResult ExportToCsv(List<Oem> oems)
        {
            var csv = new System.Text.StringBuilder();

            // CSV Header
            csv.AppendLine("OEM Name,Contact Email,Contact Number,Payment Terms,Service Level,Performance Rating,Address,Website,Total Products,Active Customers,Created Date");

            // CSV Data
            foreach (var oem in oems)
            {
                var totalProducts = oem.Products?.Count ?? 0;
                var activeCustomers = oem.CustomerOemProducts?.Select(cop => cop.CompanyId).Distinct().Count() ?? 0;
                var createdDate = oem.CreatedDate.ToString("yyyy-MM-dd");

                csv.AppendLine($"\"{oem.OemName}\"," +
                              $"\"{oem.ContactEmail ?? ""}\"," +
                              $"\"{oem.ContactNumber ?? ""}\"," +
                              $"\"{oem.PaymentTerms ?? ""}\"," +
                              $"\"{oem.ServiceLevel ?? ""}\"," +
                              $"{oem.PerformanceRating?.ToString("F2") ?? "0.00"}," +
                              $"\"{oem.Address ?? ""}\"," +
                              $"\"{oem.Website ?? ""}\"," +
                              $"{totalProducts}," +
                              $"{activeCustomers}," +
                              $"{createdDate}");
            }

            var fileName = $"OEM_Partners_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", fileName);
        }

        private IActionResult ExportToJson(List<Oem> oems)
        {
            var exportData = new
            {
                ExportDate = DateTime.Now,
                TotalRecords = oems.Count,
                OemPartners = oems.Select(oem => new
                {
                    oem.OemId,
                    oem.OemName,
                    oem.ContactEmail,
                    oem.ContactNumber,
                    oem.PaymentTerms,
                    oem.ServiceLevel,
                    oem.PerformanceRating,
                    oem.Address,
                    oem.Website,
                    oem.CreatedDate,
                    oem.CreatedBy,
                    Statistics = new
                    {
                        TotalProducts = oem.Products?.Count ?? 0,
                        ActiveCustomers = oem.CustomerOemProducts?.Select(cop => cop.CompanyId).Distinct().Count() ?? 0,
                        ProductCategories = oem.Products?.Select(p => p.ProductCategory).Where(pc => !string.IsNullOrEmpty(pc)).Distinct().Cast<string>().ToList() ?? new List<string>()
                    }
                }).ToList()
            };

            var fileName = $"OEM_Partners_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            var json = System.Text.Json.JsonSerializer.Serialize(exportData, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", fileName);
        }

        private IActionResult ExportToExcel(List<Oem> oems)
        {
            // For Excel export, we'll use a simple CSV format with Excel-compatible headers
            var csv = new System.Text.StringBuilder();

            // Excel-compatible CSV with UTF-8 BOM
            csv.AppendLine("OEM Name,Contact Email,Contact Number,Payment Terms,Service Level,Performance Rating,Address,Website,Total Products,Active Customers,Created Date,Created By");

            foreach (var oem in oems)
            {
                var totalProducts = oem.Products?.Count ?? 0;
                var activeCustomers = oem.CustomerOemProducts?.Select(cop => cop.CompanyId).Distinct().Count() ?? 0;

                csv.AppendLine($"\"{oem.OemName}\"," +
                              $"\"{oem.ContactEmail ?? ""}\"," +
                              $"\"{oem.ContactNumber ?? ""}\"," +
                              $"\"{oem.PaymentTerms ?? ""}\"," +
                              $"\"{oem.ServiceLevel ?? ""}\"," +
                              $"{oem.PerformanceRating?.ToString("F2") ?? ""}," +
                              $"\"{oem.Address?.Replace("\"", "\"\"") ?? ""}\"," +
                              $"\"{oem.Website ?? ""}\"," +
                              $"{totalProducts}," +
                              $"{activeCustomers}," +
                              $"{oem.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")}," +
                              $"\"{oem.CreatedBy ?? ""}\"");
            }

            var fileName = $"OEM_Partners_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var bytes = System.Text.Encoding.UTF8.GetPreamble().Concat(System.Text.Encoding.UTF8.GetBytes(csv.ToString())).ToArray();

            return File(bytes, "application/vnd.ms-excel", fileName);
        }

        // Private Import Methods
        private async Task<List<Oem>> ImportFromCsv(IFormFile file)
        {
            var oems = new List<Oem>();

            using var reader = new StreamReader(file.OpenReadStream());
            var headerLine = await reader.ReadLineAsync();

            if (string.IsNullOrEmpty(headerLine))
                throw new InvalidOperationException("CSV file is empty or invalid.");

            var headers = headerLine.Split(',').Select(h => h.Trim('"').Trim()).ToArray();
            var expectedHeaders = new[] { "OEM Name", "Contact Email", "Contact Number", "Payment Terms", "Service Level", "Performance Rating", "Address", "Website" };

            // Validate headers
            if (!expectedHeaders.All(eh => headers.Any(h => h.Equals(eh, StringComparison.OrdinalIgnoreCase))))
            {
                throw new InvalidOperationException($"CSV headers don't match expected format. Expected: {string.Join(", ", expectedHeaders)}");
            }

            string? line;
            var lineNumber = 1;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                lineNumber++;
                if (string.IsNullOrWhiteSpace(line)) continue;

                try
                {
                    var values = ParseCsvLine(line);
                    var oem = new Oem
                    {
                        OemName = GetValueByHeader(values, headers, "OEM Name"),
                        ContactEmail = GetValueByHeader(values, headers, "Contact Email"),
                        ContactNumber = GetValueByHeader(values, headers, "Contact Number"),
                        PaymentTerms = GetValueByHeader(values, headers, "Payment Terms"),
                        ServiceLevel = GetValueByHeader(values, headers, "Service Level"),
                        Address = GetValueByHeader(values, headers, "Address"),
                        Website = GetValueByHeader(values, headers, "Website"),
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "Import System",
                        IsActive = true
                    };

                    // Parse performance rating
                    var performanceStr = GetValueByHeader(values, headers, "Performance Rating");
                    if (!string.IsNullOrEmpty(performanceStr) && decimal.TryParse(performanceStr, out var performance))
                    {
                        oem.PerformanceRating = Math.Max(0, Math.Min(5, performance)); // Clamp to 0-5 range
                    }

                    oems.Add(oem);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Error parsing line {lineNumber}: {ex.Message}");
                }
            }

            return oems;
        }

        private async Task<List<Oem>> ImportFromJson(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var jsonContent = await reader.ReadToEndAsync();

            var importData = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(jsonContent);
            var oems = new List<Oem>();

            if (importData.TryGetProperty("OemPartners", out var oemsArray) && oemsArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var oemElement in oemsArray.EnumerateArray())
                {
                    var oem = new Oem
                    {
                        OemName = GetJsonString(oemElement, "OemName") ?? "",
                        ContactEmail = GetJsonString(oemElement, "ContactEmail"),
                        ContactNumber = GetJsonString(oemElement, "ContactNumber"),
                        PaymentTerms = GetJsonString(oemElement, "PaymentTerms"),
                        ServiceLevel = GetJsonString(oemElement, "ServiceLevel"),
                        Address = GetJsonString(oemElement, "Address"),
                        Website = GetJsonString(oemElement, "Website"),
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "Import System",
                        IsActive = true
                    };

                    // Parse performance rating
                    if (oemElement.TryGetProperty("PerformanceRating", out var perfElement) && perfElement.ValueKind == JsonValueKind.Number)
                    {
                        oem.PerformanceRating = Math.Max(0, Math.Min(5, perfElement.GetDecimal()));
                    }

                    oems.Add(oem);
                }
            }
            else if (importData.ValueKind == JsonValueKind.Array)
            {
                // Direct array format
                foreach (var oemElement in importData.EnumerateArray())
                {
                    var oem = new Oem
                    {
                        OemName = GetJsonString(oemElement, "OemName") ?? GetJsonString(oemElement, "OEM Name") ?? "",
                        ContactEmail = GetJsonString(oemElement, "ContactEmail") ?? GetJsonString(oemElement, "Contact Email"),
                        ContactNumber = GetJsonString(oemElement, "ContactNumber") ?? GetJsonString(oemElement, "Contact Number"),
                        PaymentTerms = GetJsonString(oemElement, "PaymentTerms") ?? GetJsonString(oemElement, "Payment Terms"),
                        ServiceLevel = GetJsonString(oemElement, "ServiceLevel") ?? GetJsonString(oemElement, "Service Level"),
                        Address = GetJsonString(oemElement, "Address"),
                        Website = GetJsonString(oemElement, "Website"),
                        CreatedDate = DateTime.Now,
                        CreatedBy = User.Identity?.Name ?? "Import System",
                        IsActive = true
                    };

                    // Parse performance rating
                    var perfProperty = oemElement.TryGetProperty("PerformanceRating", out var perfElement) ? perfElement :
                                      oemElement.TryGetProperty("Performance Rating", out var perfElement2) ? perfElement2 : default;

                    if (perfProperty.ValueKind == JsonValueKind.Number)
                    {
                        oem.PerformanceRating = Math.Max(0, Math.Min(5, perfProperty.GetDecimal()));
                    }

                    oems.Add(oem);
                }
            }

            return oems;
        }

        // Import Validation and Helper Methods
        private (bool HasErrors, string ErrorMessage, int SkippedCount) ValidateImportedOems(List<Oem> oems)
        {
            var errors = new List<string>();
            var skippedCount = 0;

            // Check for required fields
            for (int i = 0; i < oems.Count; i++)
            {
                var oem = oems[i];
                var rowErrors = new List<string>();

                if (string.IsNullOrWhiteSpace(oem.OemName))
                    rowErrors.Add("OEM Name is required");

                if (!string.IsNullOrEmpty(oem.ContactEmail) && !IsValidEmail(oem.ContactEmail))
                    rowErrors.Add("Invalid email format");

                if (!string.IsNullOrEmpty(oem.ServiceLevel) && !new[] { "Gold", "Silver", "Bronze" }.Contains(oem.ServiceLevel))
                    rowErrors.Add("Service Level must be Gold, Silver, or Bronze");

                if (oem.PerformanceRating.HasValue && (oem.PerformanceRating < 0 || oem.PerformanceRating > 5))
                    rowErrors.Add("Performance Rating must be between 0 and 5");

                if (rowErrors.Any())
                {
                    errors.Add($"Row {i + 1}: {string.Join(", ", rowErrors)}");
                }
            }

            // Check for duplicates within import data
            var duplicateNames = oems.GroupBy(o => o.OemName.ToLower())
                                    .Where(g => g.Count() > 1)
                                    .Select(g => g.Key);

            if (duplicateNames.Any())
            {
                errors.Add($"Duplicate OEM names in import data: {string.Join(", ", duplicateNames)}");
            }

            // Check for existing OEMs in database
            var existingOemNames = _context.Oems
                .Where(o => o.IsActive && oems.Select(io => io.OemName.ToLower()).Contains(o.OemName.ToLower()))
                .Select(o => o.OemName.ToLower())
                .ToList();

            skippedCount = existingOemNames.Count;

            return (errors.Any(), string.Join("; ", errors), skippedCount);
        }

        private async Task<int> SaveImportedOems(List<Oem> oems)
        {
            // Filter out existing OEMs
            var existingOemNames = await _context.Oems
                .Where(o => o.IsActive)
                .Select(o => o.OemName.ToLower())
                .ToListAsync();

            var newOems = oems.Where(o => !existingOemNames.Contains(o.OemName.ToLower())).ToList();

            if (newOems.Any())
            {
                _context.Oems.AddRange(newOems);
                await _context.SaveChangesAsync();
            }

            return newOems.Count;
        }

        // Template Download Methods
        private IActionResult DownloadCsvTemplate()
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("OEM Name,Contact Email,Contact Number,Payment Terms,Service Level,Performance Rating,Address,Website");
            csv.AppendLine("\"Sample OEM Corp\",\"contact@sampleoem.com\",\"+1-555-0123\",\"Net 30\",\"Gold\",\"4.5\",\"123 Business St, City, State 12345\",\"https://www.sampleoem.com\"");
            csv.AppendLine("\"Tech Solutions Inc\",\"sales@techsolutions.com\",\"+1-555-0456\",\"Net 45\",\"Silver\",\"4.0\",\"456 Technology Ave, Tech City, TC 67890\",\"https://www.techsolutions.com\"");

            var fileName = "OEM_Import_Template.csv";
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());

            return File(bytes, "text/csv", fileName);
        }

        private IActionResult DownloadJsonTemplate()
        {
            var template = new
            {
                Instructions = "This is a template for importing OEM partners. Replace the sample data with your actual OEM information.",
                RequiredFields = new[] { "OemName" },
                OptionalFields = new[] { "ContactEmail", "ContactNumber", "PaymentTerms", "ServiceLevel", "PerformanceRating", "Address", "Website" },
                ServiceLevelOptions = new[] { "Gold", "Silver", "Bronze" },
                PerformanceRatingRange = "0.0 to 5.0",
                OemPartners = new[]
                {
                    new
                    {
                        OemName = "Sample OEM Corp",
                        ContactEmail = "contact@sampleoem.com",
                        ContactNumber = "+1-555-0123",
                        PaymentTerms = "Net 30",
                        ServiceLevel = "Gold",
                        PerformanceRating = 4.5,
                        Address = "123 Business St, City, State 12345",
                        Website = "https://www.sampleoem.com"
                    },
                    new
                    {
                        OemName = "Tech Solutions Inc",
                        ContactEmail = "sales@techsolutions.com",
                        ContactNumber = "+1-555-0456",
                        PaymentTerms = "Net 45",
                        ServiceLevel = "Silver",
                        PerformanceRating = 4.0,
                        Address = "456 Technology Ave, Tech City, TC 67890",
                        Website = "https://www.techsolutions.com"
                    }
                }
            };

            var fileName = "OEM_Import_Template.json";
            var json = System.Text.Json.JsonSerializer.Serialize(template, new System.Text.Json.JsonSerializerOptions
            {
                WriteIndented = true
            });
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            return File(bytes, "application/json", fileName);
        }

        // Helper Methods
        private string[] ParseCsvLine(string line)
        {
            var values = new List<string>();
            var inQuotes = false;
            var currentValue = new System.Text.StringBuilder();

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString());
            return values.ToArray();
        }

        private string GetValueByHeader(string[] values, string[] headers, string headerName)
        {
            var index = Array.FindIndex(headers, h => h.Equals(headerName, StringComparison.OrdinalIgnoreCase));
            return index >= 0 && index < values.Length ? values[index].Trim() : string.Empty;
        }

        private string? GetJsonString(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString()
                : null;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
