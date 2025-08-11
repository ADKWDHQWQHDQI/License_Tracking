
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OfficeOpenXml;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace License_Tracking.Services
{
    public class ReportService
    {
        private readonly AppDbContext _context;

        public ReportService(AppDbContext context)
        {
            _context = context;
        }

        private static ExcelPackage CreateExcelPackage()
        {
            try
            {
                // Set license for EPPlus 8+
                var package = new ExcelPackage();
                return package;
            }
            catch (Exception)
            {
                // If license exception occurs, this will handle it in the calling code
                throw;
            }
        }

        public MemoryStream ExportToExcel(Report report)
        {
            using var package = CreateExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Report");
            var data = JsonSerializer.Deserialize<dynamic[]>(report.Data);
            if (data == null || data.Length == 0)
                return new MemoryStream(package.GetAsByteArray());
            // Write headers
            var first = data[0];
            int col = 1;
            foreach (var prop in ((JsonElement)first).EnumerateObject())
            {
                ws.Cells[1, col].Value = prop.Name;
                col++;
            }
            // Write data
            for (int i = 0; i < data.Length; i++)
            {
                int c = 1;
                foreach (var prop in ((JsonElement)data[i]).EnumerateObject())
                {
                    ws.Cells[i + 2, c].Value = prop.Value.ToString();
                    c++;
                }
            }
            var ms = new MemoryStream();
            ms.Write(package.GetAsByteArray());
            ms.Position = 0;
            return ms;
        }

        public MemoryStream ExportToPdf(Report report)
        {
            var ms = new MemoryStream();
            var doc = new Document();
            var writer = PdfWriter.GetInstance(doc, ms);
            doc.Open();
            var data = JsonSerializer.Deserialize<dynamic[]>(report.Data);
            if (data != null && data.Length > 0)
            {
                var first = data[0];
                var table = new PdfPTable(((JsonElement)first).EnumerateObject().Count());
                // Headers
                foreach (var prop in ((JsonElement)first).EnumerateObject())
                {
                    table.AddCell(new Phrase(prop.Name));
                }
                // Data
                foreach (var row in data)
                {
                    foreach (var prop in ((JsonElement)row).EnumerateObject())
                    {
                        table.AddCell(new Phrase(prop.Value.ToString()));
                    }
                }
                doc.Add(table);
            }
            doc.Close();
            ms.Position = 0;
            return ms;
        }

        public async Task<Report> GenerateCustomerSummaryReport(DateTime? startDate, DateTime? endDate, string? client = null)
        {
            var query = _context.Deals.AsQueryable();
            if (startDate.HasValue) query = query.Where(d => d.CreatedDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(d => d.CreatedDate <= endDate.Value);
            if (!string.IsNullOrEmpty(client)) query = query.Where(d => d.Company.CompanyName.Contains(client));

            var deals = await query
                .Include(d => d.Company)
                .GroupBy(d => d.Company.CompanyName)
                .Select(g => new
                {
                    ClientName = g.Key,
                    LicenseCount = g.Count(),
                    TotalAmount = g.Sum(d => d.EstimatedMargin ?? 0)
                })
                .ToListAsync();

            var data = JsonSerializer.Serialize(deals);
            var report = new Report
            {
                ReportType = "CustomerSummary",
                GeneratedDate = DateTime.Now,
                Data = data,
                GeneratedByRole = "Admin" // Adjust based on current user role
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateProductBusinessReport(DateTime? startDate, DateTime? endDate, string? product = null)
        {
            var query = _context.Deals.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(d => d.CreatedDate >= startDate.Value);
            if (endDate.HasValue)
                query = query.Where(d => d.CreatedDate <= endDate.Value);
            if (!string.IsNullOrEmpty(product))
                query = query.Where(d => d.Product.ProductName == product);

            // Fetch data and compute Margin in memory
            var deals = await query
                .Include(d => d.Product)
                .Select(d => new
                {
                    d.Product.ProductName,
                    d.CustomerInvoiceAmount,
                    d.OemQuoteAmount
                })
                .ToListAsync();

            var groupedDeals = deals
                .GroupBy(d => d.ProductName)
                .Select(g => new
                {
                    ProductName = g.Key,
                    DealCount = g.Count(),
                    TotalMargin = g.Sum(d => (d.CustomerInvoiceAmount ?? 0) - (d.OemQuoteAmount ?? 0))
                })
                .ToList();

            var data = System.Text.Json.JsonSerializer.Serialize(groupedDeals);

            var report = new Report
            {
                ReportType = "ProductBusiness",
                GeneratedDate = DateTime.Now,
                Data = data,
                GeneratedByRole = "Admin" // Or set based on current user role
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateOemProcurementCostReport(DateTime? startDate, DateTime? endDate, string? oem = null)
        {
            var query = _context.PurchaseOrders.AsQueryable();
            if (startDate.HasValue) query = query.Where(p => p.CreatedDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(p => p.CreatedDate <= endDate.Value);
            if (!string.IsNullOrEmpty(oem)) query = query.Where(p => p.Deal.Oem.OemName.Contains(oem));

            var procurement = await query
                .Include(p => p.Deal)
                .ThenInclude(d => d.Oem)
                .GroupBy(p => p.Deal.Oem.OemName)
                .Select(g => new
                {
                    OemName = g.Key,
                    TotalCost = g.Sum(p => p.OemPoAmount),
                    PurchaseCount = g.Count()
                })
                .ToListAsync();

            var data = JsonSerializer.Serialize(procurement);
            var report = new Report
            {
                ReportType = "OemProcurementCost",
                GeneratedDate = DateTime.Now,
                Data = data,
                GeneratedByRole = "Operations"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateFinancialSummaryReport(DateTime? startDate, DateTime? endDate, string period = "Monthly")
        {
            var query = _context.Invoices.AsQueryable();
            if (startDate.HasValue) query = query.Where(i => i.DueDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(i => i.DueDate <= endDate.Value);

            var summary = await query
                .GroupBy(i => new { Year = i.DueDate.Year, Period = period == "Monthly" ? i.DueDate.Month : (period == "Quarterly" ? (i.DueDate.Month - 1) / 3 + 1 : 1) })
                .Select(g => new
                {
                    Period = $"{g.Key.Year}-{(period == "Monthly" ? g.Key.Period.ToString("00") : period == "Quarterly" ? $"Q{g.Key.Period}" : "Year")}",
                    TotalRevenue = g.Sum(i => i.Amount),
                    TotalPaid = g.Sum(i => i.PaymentStatus == "Completed" ? i.Amount : 0)
                })
                .ToListAsync();

            var data = JsonSerializer.Serialize(summary);
            var report = new Report
            {
                ReportType = $"FinancialSummary_{period}",
                GeneratedDate = DateTime.Now,
                Data = data,
                GeneratedByRole = "Finance"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateExpiryRenewalReport(DateTime? startDate, DateTime? endDate, string? status = null)
        {
            var query = _context.Renewals.AsQueryable();
            if (startDate.HasValue) query = query.Where(r => r.RenewalDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(r => r.RenewalDate <= endDate.Value);
            if (!string.IsNullOrEmpty(status)) query = query.Where(r => r.Deal.DealStage.Contains(status));

            var renewals = await query
                .Include(r => r.Deal)
                .GroupBy(r => r.Deal.DealStage)
                .Select(g => new
                {
                    DealStage = g.Key,
                    RenewalCount = g.Count(),
                    UpcomingRenewals = g.Count(r => r.RenewalDate <= DateTime.Now.AddDays(90))
                })
                .ToListAsync();

            var data = JsonSerializer.Serialize(renewals);
            var report = new Report
            {
                ReportType = "ExpiryRenewal",
                GeneratedDate = DateTime.Now,
                Data = data,
                GeneratedByRole = "Admin"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<Report> GenerateMarginAnalysisReport(DateTime? startDate, DateTime? endDate, string? client = null)
        {
            var query = _context.Deals.AsQueryable();
            if (startDate.HasValue) query = query.Where(d => d.CreatedDate >= startDate.Value);
            if (endDate.HasValue) query = query.Where(d => d.CreatedDate <= endDate.Value);
            if (!string.IsNullOrEmpty(client)) query = query.Where(d => d.Company.CompanyName.Contains(client));

            var margins = await query
                .Include(d => d.Company)
                .GroupBy(d => d.Company.CompanyName)
                .Select(g => new
                {
                    ClientName = g.Key,
                    TotalMargin = g.Sum(d => d.EstimatedMargin ?? 0),
                    AverageMargin = g.Average(d => d.EstimatedMargin ?? 0)
                })
                .ToListAsync();

            var data = JsonSerializer.Serialize(margins);
            var report = new Report
            {
                ReportType = "MarginAnalysis",
                GeneratedDate = DateTime.Now,
                Data = data,
                GeneratedByRole = "Management"
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }
    }
}
