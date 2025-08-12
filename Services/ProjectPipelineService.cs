using License_Tracking.Data;
using License_Tracking.Models;
using License_Tracking.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace License_Tracking.Services
{
    public interface IProjectPipelineService
    {
        Task<List<ProjectPipelineViewModel>> GetAllProjectsAsync();
        Task<ProjectPipelineViewModel?> GetProjectByIdAsync(int id);
        Task<ProjectPipeline?> GetByIdAsync(int id); // Week 10: Added for direct model access
        Task<ProjectPipelineViewModel> CreateProjectAsync(ProjectPipelineViewModel model, string userId);
        Task<ProjectPipelineViewModel> UpdateProjectAsync(ProjectPipelineViewModel model, string userId);
        Task<bool> DeleteProjectAsync(int id);
        Task<Deal> ConvertToDealAsync(int projectId, string userId);
        Task<ProjectPipelineListViewModel> GetFilteredProjectsAsync(string? statusFilter = null, string? oemFilter = null, string? customerFilter = null);

        // Week 10 Enhanced: Advanced Filtering with Desktop-First Experience
        Task<ProjectPipelineListViewModel> GetAdvancedFilteredProjectsAsync(
            string? statusFilter = null,
            string? oemFilter = null,
            string? customerFilter = null,
            string? stageFilter = null,
            string? revenueFilter = null,
            string? closeDateFilter = null,
            string? confidenceFilter = null,
            string? activityFilter = null,
            string? successFilter = null);

        Task<decimal> GetTotalProjectedRevenueAsync();
        Task<decimal> GetTotalProjectedMarginAsync();

        // Week 10 Enhanced Analytics Methods
        Task<decimal> GetTotalEstimatedRevenueAsync();
        Task<decimal> GetTotalEstimatedMarginAsync();
        Task<decimal> GetTotalWeightedRevenueAsync();
        Task<object> GetPipelineAnalyticsSummaryAsync();

        // Week 9 Enhancements
        Task BulkUpdateStatusAsync(int[] projectIds, string newStatus);
        Task<byte[]> ExportToExcelAsync(string? statusFilter = null, string? oemFilter = null, string? customerFilter = null);
        Task<object> GetPipelineAnalyticsAsync();

        // Week 10 Enhanced: Desktop Reporting Tools
        Task<object> GetDesktopReportingDataAsync();
        Task<List<string>> GetUniqueOemNamesAsync();
        Task<List<string>> GetUniqueCustomerNamesAsync();
    }

    public class ProjectPipelineService : IProjectPipelineService
    {
        private readonly AppDbContext _context;

        public ProjectPipelineService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ProjectPipelineViewModel>> GetAllProjectsAsync()
        {
            var projects = await _context.ProjectPipelines
                .OrderByDescending(pp => pp.CreatedDate)
                .ToListAsync();

            return projects.Select(MapToViewModel).ToList();
        }

        public async Task<ProjectPipelineViewModel?> GetProjectByIdAsync(int id)
        {
            var project = await _context.ProjectPipelines.FindAsync(id);
            return project != null ? MapToViewModel(project) : null;
        }

        // Week 10: Direct model access for Delete view
        public async Task<ProjectPipeline?> GetByIdAsync(int id)
        {
            return await _context.ProjectPipelines.FindAsync(id);
        }

        public async Task<ProjectPipelineViewModel> CreateProjectAsync(ProjectPipelineViewModel model, string userId)
        {
            var project = new ProjectPipeline
            {
                ProductName = model.ProductName,
                OemName = model.OemName,
                ClientName = model.ClientName,
                ClientContactEmail = model.ClientContactEmail,
                ClientContactPhone = model.ClientContactPhone,
                ExpectedLicenseDate = model.ExpectedLicenseDate,
                ExpectedExpiryDate = model.ExpectedExpiryDate,
                CustomerPoNumber = model.CustomerPoNumber,
                CustomerPoItemDescription = model.CustomerPoItemDescription,
                ExpectedCustomerPoAmount = model.ExpectedCustomerPoAmount,
                OemPoNumber = model.OemPoNumber,
                OemPoItemDescription = model.OemPoItemDescription,
                ExpectedOemPoAmount = model.ExpectedOemPoAmount,
                ExpectedAmountToReceive = model.ExpectedAmountToReceive,
                ExpectedAmountToPay = model.ExpectedAmountToPay,
                ProjectedMarginInput = model.ProjectedMarginInput,
                MarginNotes = model.MarginNotes,
                MarginInputBy = userId,
                MarginLastUpdated = DateTime.Now,
                AlertDaysBefore = model.AlertDaysBefore,
                AlertsEnabled = model.AlertsEnabled,
                ProjectStatus = model.ProjectStatus,
                SuccessProbability = model.SuccessProbability,
                ShipToAddress = model.ShipToAddress,
                BillToAddress = model.BillToAddress,
                Remarks = model.Remarks,
                CreatedBy = userId,
                CreatedDate = DateTime.Now,

                // Week 9 Enhancement fields
                ExpectedInvoiceNumber = model.ExpectedInvoiceNumber,
                PaymentStatus = model.PaymentStatus,
                AmountReceived = model.AmountReceived,
                AmountPaid = model.AmountPaid,
                OemType = model.OemType,
                CustomerType = model.CustomerType,
                CustomerIndustry = model.CustomerIndustry,
                CustomerEmployeeCount = model.CustomerEmployeeCount,
                CustomerWebsite = model.CustomerWebsite,
                OemRelationshipType = model.OemRelationshipType,
                LastCustomerContact = model.LastCustomerContact,
                CustomerNotes = model.CustomerNotes,

                // Week 10 Enhancement: Advanced Revenue and Margin Calculations
                EstimatedRevenue = model.EstimatedRevenue,
                EstimatedCost = model.EstimatedRevenue - model.EstimatedMargin, // Calculate cost from revenue and margin
                PipelineStage = model.PipelineStage,
                StageConfidenceLevel = model.StageConfidenceLevel,
                ExpectedCloseDate = model.ExpectedCloseDate
            };

            _context.ProjectPipelines.Add(project);
            await _context.SaveChangesAsync();

            return MapToViewModel(project);
        }

        public async Task<ProjectPipelineViewModel> UpdateProjectAsync(ProjectPipelineViewModel model, string userId)
        {
            var project = await _context.ProjectPipelines.FindAsync(model.ProjectPipelineId);
            if (project == null)
                throw new ArgumentException("Project not found");

            // Update properties
            project.ProductName = model.ProductName;
            project.OemName = model.OemName;
            project.ClientName = model.ClientName;
            project.ClientContactEmail = model.ClientContactEmail;
            project.ClientContactPhone = model.ClientContactPhone;
            project.ExpectedLicenseDate = model.ExpectedLicenseDate;
            project.ExpectedExpiryDate = model.ExpectedExpiryDate;
            project.CustomerPoNumber = model.CustomerPoNumber;
            project.CustomerPoItemDescription = model.CustomerPoItemDescription;
            project.ExpectedCustomerPoAmount = model.ExpectedCustomerPoAmount;
            project.OemPoNumber = model.OemPoNumber;
            project.OemPoItemDescription = model.OemPoItemDescription;
            project.ExpectedOemPoAmount = model.ExpectedOemPoAmount;
            project.ExpectedAmountToReceive = model.ExpectedAmountToReceive;
            project.ExpectedAmountToPay = model.ExpectedAmountToPay;

            // Update margin if changed
            if (project.ProjectedMarginInput != model.ProjectedMarginInput)
            {
                project.ProjectedMarginInput = model.ProjectedMarginInput;
                project.MarginInputBy = userId;
                project.MarginLastUpdated = DateTime.Now;
            }

            project.MarginNotes = model.MarginNotes;
            project.AlertDaysBefore = model.AlertDaysBefore;
            project.AlertsEnabled = model.AlertsEnabled;
            project.ProjectStatus = model.ProjectStatus;
            project.SuccessProbability = model.SuccessProbability;
            project.ShipToAddress = model.ShipToAddress;
            project.BillToAddress = model.BillToAddress;
            project.Remarks = model.Remarks;

            // Phase 4.7 - Invoice & Payment Tracking
            project.ExpectedInvoiceNumber = model.ExpectedInvoiceNumber;
            project.PaymentStatus = model.PaymentStatus;
            project.AmountReceived = model.AmountReceived;
            project.AmountPaid = model.AmountPaid;

            // Phase 4.7 - OEM Type Classification
            project.OemType = model.OemType;

            // Phase 4.7 - Enhanced Customer Profile
            project.CustomerType = model.CustomerType;
            project.CustomerIndustry = model.CustomerIndustry;
            project.CustomerEmployeeCount = model.CustomerEmployeeCount;
            project.CustomerWebsite = model.CustomerWebsite;

            // Phase 4.7 - OEM Relationship
            project.OemRelationshipType = model.OemRelationshipType;
            project.LastCustomerContact = model.LastCustomerContact;
            project.CustomerNotes = model.CustomerNotes;

            // Week 10 Enhancement: Advanced Revenue and Margin Calculations
            project.EstimatedRevenue = model.EstimatedRevenue;
            project.EstimatedCost = model.EstimatedRevenue - model.EstimatedMargin; // Calculate cost from revenue and margin
            project.PipelineStage = model.PipelineStage;
            project.StageConfidenceLevel = model.StageConfidenceLevel;
            project.ExpectedCloseDate = model.ExpectedCloseDate;

            project.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();
            return MapToViewModel(project);
        }

        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.ProjectPipelines.FindAsync(id);
            if (project == null)
                return false;

            _context.ProjectPipelines.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Deal> ConvertToDealAsync(int projectId, string userId)
        {
            var project = await _context.ProjectPipelines.FindAsync(projectId);
            if (project == null)
                throw new ArgumentException("Project not found");

            if (project.ConvertedToLicenseId.HasValue)
                throw new InvalidOperationException("Project has already been converted to a deal");

            // Find or create Company
            var company = await _context.Companies.FirstOrDefaultAsync(c => c.CompanyName == project.ClientName)
                ?? new Company
                {
                    CompanyName = project.ClientName,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    IsActive = true
                };

            if (company.CompanyId == 0)
            {
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();
            }

            // Find or create OEM
            var oem = await _context.Oems.FirstOrDefaultAsync(o => o.OemName == project.OemName)
                ?? new Oem
                {
                    OemName = project.OemName,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    IsActive = true
                };

            if (oem.OemId == 0)
            {
                _context.Oems.Add(oem);
                await _context.SaveChangesAsync();
            }

            // Find or create Product
            var product = await _context.Products.FirstOrDefaultAsync(p => p.ProductName == project.ProductName && p.OemId == oem.OemId)
                ?? new Product
                {
                    ProductName = project.ProductName,
                    OemId = oem.OemId,
                    CreatedDate = DateTime.Now,
                    CreatedBy = userId,
                    IsActive = true
                };

            if (product.ProductId == 0)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }

            // Create new deal from project pipeline
            var deal = new Deal
            {
                CompanyId = company.CompanyId,
                OemId = oem.OemId,
                ProductId = product.ProductId,
                DealName = $"{project.ProductName} - {project.ClientName}",
                DealStage = "Quoted",
                DealType = "New License",
                Quantity = 1,
                CustomerPoNumber = project.CustomerPoNumber,
                CustomerInvoiceAmount = project.ExpectedCustomerPoAmount,
                OemQuoteAmount = project.ExpectedOemPoAmount,
                EstimatedMargin = project.ProjectedMarginInput,
                LicenseStartDate = project.ExpectedLicenseDate,
                LicenseEndDate = project.ExpectedExpiryDate,
                AssignedTo = userId,
                Notes = $"Converted from Pipeline Project. Original Remarks: {project.Remarks}",
                CreatedDate = DateTime.Now,
                CreatedBy = userId,
                LastModifiedDate = DateTime.Now,
                LastModifiedBy = userId,
                IsActive = true
            };

            _context.Deals.Add(deal);
            await _context.SaveChangesAsync();

            // Update project to mark as converted
            project.ConvertedToLicenseId = deal.DealId; // Reusing this field for deal ID
            project.ProjectStatus = "Converted";
            project.LastUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            return deal;
        }

        public async Task<ProjectPipelineListViewModel> GetFilteredProjectsAsync(string? statusFilter = null, string? oemFilter = null, string? customerFilter = null)
        {
            var query = _context.ProjectPipelines.AsQueryable();

            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(pp => pp.ProjectStatus == statusFilter);

            if (!string.IsNullOrEmpty(oemFilter))
                query = query.Where(pp => pp.OemName.Contains(oemFilter));

            if (!string.IsNullOrEmpty(customerFilter))
                query = query.Where(pp => pp.ClientName.Contains(customerFilter));

            var projects = await query
                .OrderByDescending(pp => pp.CreatedDate)
                .ToListAsync();

            var viewModels = projects.Select(MapToViewModel).ToList();

            return new ProjectPipelineListViewModel
            {
                Projects = viewModels,
                TotalCount = viewModels.Count,
                TotalProjectedRevenue = viewModels.Sum(p => p.ProjectedRevenue),
                TotalProjectedMargin = viewModels.Sum(p => p.ProjectedMargin),
                StatusFilter = statusFilter ?? string.Empty,
                OemFilter = oemFilter ?? string.Empty,
                CustomerFilter = customerFilter ?? string.Empty
            };
        }

        public async Task<decimal> GetTotalProjectedRevenueAsync()
        {
            var projects = await _context.ProjectPipelines
                .Where(pp => pp.ProjectStatus == "Pipeline" || pp.ProjectStatus == "In Progress")
                .ToListAsync();

            return projects.Sum(p => p.ProjectedRevenue);
        }

        public async Task<decimal> GetTotalProjectedMarginAsync()
        {
            var projects = await _context.ProjectPipelines
                .Where(pp => pp.ProjectStatus == "Pipeline" || pp.ProjectStatus == "In Progress")
                .ToListAsync();

            return projects.Sum(p => p.ProjectedMargin);
        }

        // Week 10 Enhanced Analytics Method Implementations
        public async Task<decimal> GetTotalEstimatedRevenueAsync()
        {
            var projects = await _context.ProjectPipelines
                .Where(pp => pp.ProjectStatus == "Pipeline" || pp.ProjectStatus == "In Progress")
                .ToListAsync();

            return projects.Sum(p => p.EstimatedRevenue);
        }

        public async Task<decimal> GetTotalEstimatedMarginAsync()
        {
            var projects = await _context.ProjectPipelines
                .Where(pp => pp.ProjectStatus == "Pipeline" || pp.ProjectStatus == "In Progress")
                .ToListAsync();

            return projects.Sum(p => p.EstimatedMargin);
        }

        public async Task<decimal> GetTotalWeightedRevenueAsync()
        {
            var projects = await _context.ProjectPipelines
                .Where(pp => pp.ProjectStatus == "Pipeline" || pp.ProjectStatus == "In Progress")
                .ToListAsync();

            return projects.Sum(p => p.WeightedRevenue);
        }

        public async Task<object> GetPipelineAnalyticsSummaryAsync()
        {
            var projects = await _context.ProjectPipelines
                .Where(pp => pp.ProjectStatus == "Pipeline" || pp.ProjectStatus == "In Progress")
                .ToListAsync();

            return new
            {
                TotalEstimatedRevenue = projects.Sum(p => p.EstimatedRevenue),
                TotalEstimatedMargin = projects.Sum(p => p.EstimatedMargin),
                TotalWeightedRevenue = projects.Sum(p => p.WeightedRevenue),
                AverageMarginPercentage = projects.Where(p => p.EstimatedRevenue > 0).Any() ?
                    projects.Where(p => p.EstimatedRevenue > 0).Average(p => (p.EstimatedMargin / p.EstimatedRevenue) * 100) : 0,
                AverageSuccessProbability = projects.Any() ? projects.Average(p => p.SuccessProbability) : 0,
                AverageConfidenceLevel = projects.Any() ? projects.Average(p => p.StageConfidenceLevel) : 0,
                ActiveProjectsCount = projects.Count,
                HighValueProjects = projects.Count(p => p.EstimatedRevenue >= 50000),
                HighProbabilityProjects = projects.Count(p => p.SuccessProbability >= 75)
            };
        }

        #region Private Helper Methods

        private static ProjectPipelineViewModel MapToViewModel(ProjectPipeline project)
        {
            return new ProjectPipelineViewModel
            {
                ProjectPipelineId = project.ProjectPipelineId,
                ProductName = project.ProductName,
                OemName = project.OemName,
                ClientName = project.ClientName,
                ClientContactEmail = project.ClientContactEmail,
                ClientContactPhone = project.ClientContactPhone,
                ExpectedLicenseDate = project.ExpectedLicenseDate,
                ExpectedExpiryDate = project.ExpectedExpiryDate,
                CustomerPoNumber = project.CustomerPoNumber,
                CustomerPoItemDescription = project.CustomerPoItemDescription,
                ExpectedCustomerPoAmount = project.ExpectedCustomerPoAmount,
                OemPoNumber = project.OemPoNumber,
                OemPoItemDescription = project.OemPoItemDescription,
                ExpectedOemPoAmount = project.ExpectedOemPoAmount,
                ExpectedAmountToReceive = project.ExpectedAmountToReceive,
                ExpectedAmountToPay = project.ExpectedAmountToPay,
                ProjectedMarginInput = project.ProjectedMarginInput,
                MarginNotes = project.MarginNotes,
                AlertDaysBefore = project.AlertDaysBefore,
                AlertsEnabled = project.AlertsEnabled,
                ProjectStatus = project.ProjectStatus,
                SuccessProbability = project.SuccessProbability,
                ShipToAddress = project.ShipToAddress,
                BillToAddress = project.BillToAddress,
                Remarks = project.Remarks,
                CreatedBy = project.CreatedBy ?? string.Empty,
                CreatedDate = project.CreatedDate,
                LastUpdated = project.LastUpdated,
                ConvertedToLicenseId = project.ConvertedToLicenseId,

                // Week 9 Enhancement fields
                ExpectedInvoiceNumber = project.ExpectedInvoiceNumber,
                PaymentStatus = project.PaymentStatus,
                AmountReceived = project.AmountReceived,
                AmountPaid = project.AmountPaid,
                OemType = project.OemType,
                CustomerType = project.CustomerType,
                CustomerIndustry = project.CustomerIndustry,
                CustomerEmployeeCount = project.CustomerEmployeeCount,
                CustomerWebsite = project.CustomerWebsite,
                OemRelationshipType = project.OemRelationshipType,
                LastCustomerContact = project.LastCustomerContact,
                CustomerNotes = project.CustomerNotes,

                // Week 10 Enhancement: Advanced Revenue and Margin Calculations
                EstimatedRevenue = project.EstimatedRevenue,
                EstimatedCost = project.EstimatedCost,
                PipelineStage = project.PipelineStage,
                StageConfidenceLevel = project.StageConfidenceLevel,
                ExpectedCloseDate = project.ExpectedCloseDate
            };
        }

        // Week 9 Enhancement: Bulk Status Update
        public async Task BulkUpdateStatusAsync(int[] projectIds, string newStatus)
        {
            var projects = await _context.ProjectPipelines
                .Where(p => projectIds.Contains(p.ProjectPipelineId))
                .ToListAsync();

            foreach (var project in projects)
            {
                project.ProjectStatus = newStatus;
                project.LastUpdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }

        // Week 9 Enhancement: Export to Excel
        public async Task<byte[]> ExportToExcelAsync(string? statusFilter = null, string? oemFilter = null, string? customerFilter = null)
        {
            var projects = await _context.ProjectPipelines.AsQueryable()
                .Where(p => string.IsNullOrEmpty(statusFilter) || p.ProjectStatus == statusFilter)
                .Where(p => string.IsNullOrEmpty(oemFilter) || p.OemName.Contains(oemFilter))
                .Where(p => string.IsNullOrEmpty(customerFilter) || p.ClientName.Contains(customerFilter))
                .OrderByDescending(p => p.CreatedDate)
                .ToListAsync();

            using var package = new OfficeOpenXml.ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Pipeline Export");

            // Headers
            var headers = new[]
            {
                "Project ID", "Product Name", "OEM Name", "Client Name", "Status",
                "Expected License Date", "Expected Expiry Date", "Expected Revenue",
                "Expected Cost", "Projected Margin", "Success Probability", "Created Date"
            };

            for (int i = 0; i < headers.Length; i++)
            {
                worksheet.Cells[1, i + 1].Value = headers[i];
                worksheet.Cells[1, i + 1].Style.Font.Bold = true;
            }

            // Data
            for (int i = 0; i < projects.Count; i++)
            {
                var project = projects[i];
                var row = i + 2;

                worksheet.Cells[row, 1].Value = project.ProjectPipelineId;
                worksheet.Cells[row, 2].Value = project.ProductName;
                worksheet.Cells[row, 3].Value = project.OemName;
                worksheet.Cells[row, 4].Value = project.ClientName;
                worksheet.Cells[row, 5].Value = project.ProjectStatus;
                worksheet.Cells[row, 6].Value = project.ExpectedLicenseDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 7].Value = project.ExpectedExpiryDate.ToString("yyyy-MM-dd");
                worksheet.Cells[row, 8].Value = project.ExpectedAmountToReceive;
                worksheet.Cells[row, 9].Value = project.ExpectedAmountToPay;
                worksheet.Cells[row, 10].Value = project.ProjectedMarginInput;
                worksheet.Cells[row, 11].Value = project.SuccessProbability;
                worksheet.Cells[row, 12].Value = project.CreatedDate.ToString("yyyy-MM-dd HH:mm");
            }

            worksheet.Cells.AutoFitColumns();
            return package.GetAsByteArray();
        }

        // Week 10 Enhancement: Enhanced Pipeline Analytics with Accurate Calculations
        public async Task<object> GetPipelineAnalyticsAsync()
        {
            var projects = await _context.ProjectPipelines.ToListAsync();

            var analytics = new
            {
                // Basic Metrics
                TotalProjects = projects.Count,
                ActiveProjects = projects.Count(p => p.ProjectStatus == "Pipeline" || p.ProjectStatus == "Negotiation"),

                // Week 10 Enhanced Revenue Calculations
                TotalEstimatedRevenue = projects.Sum(p => p.EstimatedRevenue),
                TotalEstimatedCost = projects.Sum(p => p.EstimatedCost),
                TotalEstimatedMargin = projects.Sum(p => p.EstimatedMargin),
                TotalWeightedRevenue = projects.Sum(p => p.WeightedRevenue),

                // Legacy Calculations (for backward compatibility)
                TotalProjectedRevenue = projects.Sum(p => p.ProjectedRevenue),
                TotalProjectedMargin = projects.Sum(p => p.ProjectedMargin),

                // Performance Metrics
                AverageSuccessProbability = projects.Any() ? projects.Average(p => p.SuccessProbability) : 0,
                AverageMarginPercentage = projects.Where(p => p.EstimatedRevenue > 0).Any() ?
                    projects.Where(p => p.EstimatedRevenue > 0).Average(p => (p.EstimatedMargin / p.EstimatedRevenue) * 100) : 0,
                AverageConfidenceLevel = projects.Any() ? projects.Average(p => p.StageConfidenceLevel) : 0,

                // Groupings and Analysis
                ProjectsByStatus = projects.GroupBy(p => p.ProjectStatus)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Count = g.Count(),
                        EstimatedRevenue = g.Sum(p => p.EstimatedRevenue),
                        WeightedRevenue = g.Sum(p => p.WeightedRevenue),
                        EstimatedMargin = g.Sum(p => p.EstimatedMargin)
                    }),

                ProjectsByStage = projects.GroupBy(p => p.PipelineStage)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Count = g.Count(),
                        EstimatedRevenue = g.Sum(p => p.EstimatedRevenue),
                        WeightedRevenue = g.Sum(p => p.WeightedRevenue),
                        AverageConfidence = g.Average(p => p.StageConfidenceLevel)
                    }),

                ProjectsByOem = projects.GroupBy(p => p.OemName)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Count = g.Count(),
                        EstimatedRevenue = g.Sum(p => p.EstimatedRevenue),
                        WeightedRevenue = g.Sum(p => p.WeightedRevenue),
                        EstimatedMargin = g.Sum(p => p.EstimatedMargin)
                    }),

                // Time-based Analysis
                ProjectsThisMonth = projects.Count(p => p.CreatedDate.Month == DateTime.Now.Month &&
                                                      p.CreatedDate.Year == DateTime.Now.Year),
                UpcomingClosures = projects.Count(p => p.ExpectedCloseDate.HasValue &&
                                                      p.ExpectedCloseDate <= DateTime.Now.AddDays(30) &&
                                                      p.ExpectedCloseDate >= DateTime.Now),
                UpcomingLicenses = projects.Count(p => p.ExpectedLicenseDate <= DateTime.Now.AddDays(30) &&
                                                      p.ExpectedLicenseDate >= DateTime.Now),

                // Monthly Trend with Enhanced Metrics
                MonthlyTrend = projects.GroupBy(p => new { p.CreatedDate.Year, p.CreatedDate.Month })
                    .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                    .Select(g => new
                    {
                        Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Count = g.Count(),
                        EstimatedRevenue = g.Sum(p => p.EstimatedRevenue),
                        WeightedRevenue = g.Sum(p => p.WeightedRevenue),
                        EstimatedMargin = g.Sum(p => p.EstimatedMargin),
                        AverageSuccessProbability = g.Average(p => p.SuccessProbability)
                    })
                    .TakeLast(12)
                    .ToList(),

                // High-Value Pipeline Analysis
                HighValueDeals = projects.Where(p => p.EstimatedRevenue >= 50000)
                    .Select(p => new
                    {
                        p.ProjectPipelineId,
                        p.ProductName,
                        p.ClientName,
                        p.EstimatedRevenue,
                        p.WeightedRevenue,
                        p.SuccessProbability,
                        p.PipelineStage
                    }).ToList(),

                // Pipeline Health Indicators
                PipelineHealth = new
                {
                    HighConfidenceDeals = projects.Count(p => p.StageConfidenceLevel >= 4),
                    LowConfidenceDeals = projects.Count(p => p.StageConfidenceLevel <= 2),
                    HighProbabilityDeals = projects.Count(p => p.SuccessProbability >= 75),
                    AtRiskDeals = projects.Count(p => p.SuccessProbability <= 25),
                    StaleDeals = projects.Count(p => p.LastActivityDate.HasValue &&
                                               p.LastActivityDate < DateTime.Now.AddDays(-30))
                }
            };

            return analytics;
        }

        // Week 10 Enhanced: Advanced Filtering with Desktop-First Experience
        public async Task<ProjectPipelineListViewModel> GetAdvancedFilteredProjectsAsync(
            string? statusFilter = null,
            string? oemFilter = null,
            string? customerFilter = null,
            string? stageFilter = null,
            string? revenueFilter = null,
            string? closeDateFilter = null,
            string? confidenceFilter = null,
            string? activityFilter = null,
            string? successFilter = null)
        {
            var query = _context.ProjectPipelines.AsQueryable();

            // Apply basic filters
            if (!string.IsNullOrEmpty(statusFilter))
                query = query.Where(p => p.ProjectStatus == statusFilter);

            if (!string.IsNullOrEmpty(oemFilter))
                query = query.Where(p => p.OemName.Contains(oemFilter));

            if (!string.IsNullOrEmpty(customerFilter))
                query = query.Where(p => p.ClientName.Contains(customerFilter));

            // Apply advanced filters
            if (!string.IsNullOrEmpty(stageFilter))
                query = query.Where(p => p.PipelineStage == stageFilter);

            if (!string.IsNullOrEmpty(revenueFilter))
            {
                query = revenueFilter switch
                {
                    "small" => query.Where(p => p.EstimatedRevenue < 50000),
                    "medium" => query.Where(p => p.EstimatedRevenue >= 50000 && p.EstimatedRevenue < 500000),
                    "large" => query.Where(p => p.EstimatedRevenue >= 500000 && p.EstimatedRevenue < 5000000),
                    "enterprise" => query.Where(p => p.EstimatedRevenue >= 5000000),
                    _ => query
                };
            }

            if (!string.IsNullOrEmpty(closeDateFilter))
            {
                var now = DateTime.Now;
                query = closeDateFilter switch
                {
                    "overdue" => query.Where(p => p.ExpectedCloseDate.HasValue && p.ExpectedCloseDate < now),
                    "thisweek" => query.Where(p => p.ExpectedCloseDate.HasValue && p.ExpectedCloseDate >= now && p.ExpectedCloseDate <= now.AddDays(7)),
                    "thismonth" => query.Where(p => p.ExpectedCloseDate.HasValue && p.ExpectedCloseDate >= now && p.ExpectedCloseDate <= now.AddMonths(1)),
                    "nextmonth" => query.Where(p => p.ExpectedCloseDate.HasValue && p.ExpectedCloseDate >= now.AddMonths(1) && p.ExpectedCloseDate <= now.AddMonths(2)),
                    "thisquarter" => query.Where(p => p.ExpectedCloseDate.HasValue && p.ExpectedCloseDate >= now && p.ExpectedCloseDate <= now.AddMonths(3)),
                    _ => query
                };
            }

            if (!string.IsNullOrEmpty(confidenceFilter) && int.TryParse(confidenceFilter, out int confidence))
                query = query.Where(p => p.StageConfidenceLevel == confidence);

            if (!string.IsNullOrEmpty(activityFilter))
            {
                var now = DateTime.Now;
                query = activityFilter switch
                {
                    "today" => query.Where(p => p.LastActivityDate.HasValue && p.LastActivityDate.Value.Date == now.Date),
                    "week" => query.Where(p => p.LastActivityDate.HasValue && p.LastActivityDate >= now.AddDays(-7)),
                    "month" => query.Where(p => p.LastActivityDate.HasValue && p.LastActivityDate >= now.AddMonths(-1)),
                    "stale" => query.Where(p => !p.LastActivityDate.HasValue || p.LastActivityDate < now.AddDays(-30)),
                    _ => query
                };
            }

            if (!string.IsNullOrEmpty(successFilter))
            {
                query = successFilter switch
                {
                    "high" => query.Where(p => p.SuccessProbability >= 75),
                    "medium" => query.Where(p => p.SuccessProbability >= 25 && p.SuccessProbability < 75),
                    "low" => query.Where(p => p.SuccessProbability < 25),
                    _ => query
                };
            }

            var projects = await query.OrderByDescending(p => p.CreatedDate).ToListAsync();
            var projectViewModels = projects.Select(MapToViewModel).ToList();

            return new ProjectPipelineListViewModel
            {
                Projects = projectViewModels,
                TotalCount = projectViewModels.Count,
                TotalProjectedRevenue = projectViewModels.Sum(p => p.ProjectedRevenue),
                TotalProjectedMargin = projectViewModels.Sum(p => p.ProjectedMargin),
                StatusFilter = statusFilter ?? "",
                OemFilter = oemFilter ?? "",
                CustomerFilter = customerFilter ?? ""
            };
        }

        // Week 10 Enhanced: Desktop Reporting Tools
        public async Task<object> GetDesktopReportingDataAsync()
        {
            var projects = await _context.ProjectPipelines.ToListAsync();

            return new
            {
                TotalProjects = projects.Count,
                ProjectsByMonth = projects.GroupBy(p => new { p.CreatedDate.Year, p.CreatedDate.Month })
                    .Select(g => new
                    {
                        Period = $"{g.Key.Year}-{g.Key.Month:D2}",
                        Count = g.Count(),
                        Revenue = g.Sum(p => p.EstimatedRevenue),
                        Margin = g.Sum(p => p.EstimatedMargin)
                    }).ToList(),
                ProjectsByOem = projects.GroupBy(p => p.OemName)
                    .Select(g => new
                    {
                        OemName = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(p => p.EstimatedRevenue),
                        AverageSuccessRate = g.Average(p => p.SuccessProbability)
                    }).ToList(),
                ProjectsByStage = projects.GroupBy(p => p.PipelineStage)
                    .Select(g => new
                    {
                        Stage = g.Key,
                        Count = g.Count(),
                        Revenue = g.Sum(p => p.EstimatedRevenue),
                        WeightedRevenue = g.Sum(p => p.WeightedRevenue)
                    }).ToList(),
                PerformanceMetrics = new
                {
                    AverageSuccessRate = projects.Any() ? projects.Average(p => p.SuccessProbability) : 0,
                    AverageMarginPercentage = projects.Where(p => p.EstimatedRevenue > 0).Any() ?
                        projects.Where(p => p.EstimatedRevenue > 0).Average(p => (p.EstimatedMargin / p.EstimatedRevenue) * 100) : 0,
                    ConversionRate = projects.Count(p => p.ProjectStatus == "Won") / (double)Math.Max(projects.Count, 1) * 100,
                    PipelineVelocity = projects.Where(p => p.LastActivityDate.HasValue)
                        .Select(p => (DateTime.Now - p.LastActivityDate!.Value).TotalDays)
                        .DefaultIfEmpty(0)
                        .Average()
                }
            };
        }

        // Week 10 Enhanced: Autocomplete Support
        public async Task<List<string>> GetUniqueOemNamesAsync()
        {
            return await _context.ProjectPipelines
                .Select(p => p.OemName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }

        public async Task<List<string>> GetUniqueCustomerNamesAsync()
        {
            return await _context.ProjectPipelines
                .Select(p => p.ClientName)
                .Distinct()
                .OrderBy(name => name)
                .ToListAsync();
        }

        #endregion
    }
}
