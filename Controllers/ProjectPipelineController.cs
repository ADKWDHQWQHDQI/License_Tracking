using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using License_Tracking.Services;
using License_Tracking.ViewModels;
using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class ProjectPipelineController : Controller
    {
        private readonly IProjectPipelineService _projectPipelineService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppDbContext _context;

        public ProjectPipelineController(
            IProjectPipelineService projectPipelineService,
            UserManager<IdentityUser> userManager,
            AppDbContext context)
        {
            _projectPipelineService = projectPipelineService;
            _userManager = userManager;
            _context = context;
        }

        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> Index(
            string? statusFilter = null,
            string? oemFilter = null,
            string? customerFilter = null,
            string? stageFilter = null,
            string? revenueFilter = null,
            string? closeDateFilter = null,
            string? confidenceFilter = null,
            string? activityFilter = null,
            string? successFilter = null,
            string view = "list")
        {
            try
            {
                // Week 10 Enhancement: Use advanced filtering if any advanced filters are provided
                var hasAdvancedFilters = !string.IsNullOrEmpty(stageFilter) || !string.IsNullOrEmpty(revenueFilter) ||
                                         !string.IsNullOrEmpty(closeDateFilter) || !string.IsNullOrEmpty(confidenceFilter) ||
                                         !string.IsNullOrEmpty(activityFilter) || !string.IsNullOrEmpty(successFilter);

                ProjectPipelineListViewModel projects;

                if (hasAdvancedFilters)
                {
                    projects = await _projectPipelineService.GetAdvancedFilteredProjectsAsync(
                        statusFilter, oemFilter, customerFilter, stageFilter, revenueFilter,
                        closeDateFilter, confidenceFilter, activityFilter, successFilter);
                }
                else
                {
                    projects = await _projectPipelineService.GetFilteredProjectsAsync(statusFilter, oemFilter, customerFilter);
                }

                // Store view type and filters for the template
                ViewBag.CurrentView = view.ToLower();
                ViewBag.StatusFilter = statusFilter;
                ViewBag.OemFilter = oemFilter;
                ViewBag.CustomerFilter = customerFilter;
                ViewBag.StageFilter = stageFilter;
                ViewBag.RevenueFilter = revenueFilter;
                ViewBag.CloseDateFilter = closeDateFilter;
                ViewBag.ConfidenceFilter = confidenceFilter;
                ViewBag.ActivityFilter = activityFilter;
                ViewBag.SuccessFilter = successFilter;

                // Return appropriate view based on requested view type
                switch (view.ToLower())
                {
                    case "kanban":
                        return View("Kanban", projects);
                    case "sheet":
                        return View("Sheet", projects);
                    default:
                        return View("Index", projects);
                }
            }
            catch
            {
                ViewBag.ErrorMessage = "An error occurred while loading pipeline projects.";
                return View(new ProjectPipelineListViewModel());
            }
        }

        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var project = await _projectPipelineService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound();
                }
                return View(project);
            }
            catch
            {
                return NotFound();
            }
        }

        [Authorize(Roles = "Admin,Sales,Management")]
        public IActionResult Create()
        {
            var model = new ProjectPipelineViewModel
            {
                ExpectedLicenseDate = DateTime.Today.AddDays(30),
                ExpectedExpiryDate = DateTime.Today.AddYears(1),
                AlertDaysBefore = 45,
                AlertsEnabled = true,
                ProjectStatus = "Pipeline",
                SuccessProbability = 50
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> Create(ProjectPipelineViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var userId = currentUser?.Email ?? "Unknown";

                    var createdProject = await _projectPipelineService.CreateProjectAsync(model, userId);
                    TempData["SuccessMessage"] = "Pipeline project created successfully.";
                    return RedirectToAction(nameof(Details), new { id = createdProject.ProjectPipelineId });
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while creating the project.");
                }
            }
            return View(model);
        }

        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var project = await _projectPipelineService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound();
                }
                return View(project);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> Edit(int id, ProjectPipelineViewModel model)
        {
            if (id != model.ProjectPipelineId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var currentUser = await _userManager.GetUserAsync(User);
                    var userId = currentUser?.Email ?? "Unknown";

                    var updatedProject = await _projectPipelineService.UpdateProjectAsync(model, userId);
                    TempData["SuccessMessage"] = "Pipeline project updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = updatedProject.ProjectPipelineId });
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while updating the project.");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Management")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _projectPipelineService.DeleteProjectAsync(id);
                if (success)
                {
                    TempData["SuccessMessage"] = "Pipeline project deleted successfully.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Project not found.";
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while deleting the project.";
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var project = await _projectPipelineService.GetByIdAsync(id.Value);
                if (project == null)
                {
                    return NotFound();
                }
                return View(project);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [HttpPost]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> ConvertToLicense(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Email ?? "Unknown";

                var deal = await _projectPipelineService.ConvertToDealAsync(id, userId);
                TempData["SuccessMessage"] = $"Project successfully converted to Deal #{deal.DealId}.";
                return RedirectToAction("Details", "Deals", new { id = deal.DealId });
            }
            catch (InvalidOperationException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }
            catch
            {
                TempData["ErrorMessage"] = "An error occurred while converting the project to a license.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // API endpoints for AJAX calls
        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management,Finance")]
        public async Task<JsonResult> GetProjectSummaryData()
        {
            try
            {
                var totalRevenue = await _projectPipelineService.GetTotalProjectedRevenueAsync();
                var totalMargin = await _projectPipelineService.GetTotalProjectedMarginAsync();

                return Json(new
                {
                    totalProjectedRevenue = totalRevenue,
                    totalProjectedMargin = totalMargin
                });
            }
            catch
            {
                return Json(new { error = "Failed to load project summary data" });
            }
        }

        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> UpdateMargin(int id)
        {
            try
            {
                var project = await _projectPipelineService.GetProjectByIdAsync(id);
                if (project == null)
                {
                    return NotFound();
                }

                // Create a view model for margin update only
                var marginModel = new ProjectMarginUpdateViewModel
                {
                    ProjectPipelineId = project.ProjectPipelineId,
                    ProductName = project.ProductName,
                    ClientName = project.ClientName,
                    ExpectedAmountToReceive = project.ExpectedAmountToReceive,
                    ExpectedAmountToPay = project.ExpectedAmountToPay,
                    ProjectedMarginInput = project.ProjectedMarginInput,
                    MarginNotes = project.MarginNotes
                };

                return View(marginModel);
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> UpdateMargin(ProjectMarginUpdateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var project = await _projectPipelineService.GetProjectByIdAsync(model.ProjectPipelineId);
                    if (project == null)
                    {
                        return NotFound();
                    }

                    // Update only margin-related fields
                    project.ProjectedMarginInput = model.ProjectedMarginInput;
                    project.MarginNotes = model.MarginNotes;

                    var currentUser = await _userManager.GetUserAsync(User);
                    var userId = currentUser?.Email ?? "Unknown";

                    await _projectPipelineService.UpdateProjectAsync(project, userId);
                    TempData["SuccessMessage"] = "Project margin updated successfully.";
                    return RedirectToAction(nameof(Details), new { id = model.ProjectPipelineId });
                }
                catch
                {
                    ModelState.AddModelError("", "An error occurred while updating the margin.");
                }
            }
            return View(model);
        }

        // Week 9 Enhancement: Convert Pipeline to Deal
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> ConvertToDeal(int id)
        {
            try
            {
                var currentUser = await _userManager.GetUserAsync(User);
                var userId = currentUser?.Email ?? "Unknown";

                var deal = await _projectPipelineService.ConvertToDealAsync(id, userId);
                TempData["SuccessMessage"] = $"Pipeline project successfully converted to Deal #{deal.DealId}";
                return RedirectToAction("Details", "Deals", new { id = deal.DealId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error converting to deal: {ex.Message}";
                return RedirectToAction(nameof(Details), new { id });
            }
        }

        // Week 9 Enhancement: Bulk Actions
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> BulkUpdateStatus(int[] selectedIds, string newStatus)
        {
            try
            {
                if (selectedIds?.Length > 0 && !string.IsNullOrEmpty(newStatus))
                {
                    await _projectPipelineService.BulkUpdateStatusAsync(selectedIds, newStatus);
                    TempData["SuccessMessage"] = $"Successfully updated {selectedIds.Length} projects to {newStatus} status.";
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error updating projects: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Week 9 Enhancement: Export to Excel
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> ExportToExcel(string? statusFilter = null, string? oemFilter = null, string? customerFilter = null)
        {
            try
            {
                var excelData = await _projectPipelineService.ExportToExcelAsync(statusFilter, oemFilter, customerFilter);
                return File(excelData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                           $"Pipeline_Export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error exporting data: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        // Week 10 Enhancement: Enhanced Pipeline Analytics API
        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> GetPipelineAnalytics()
        {
            try
            {
                var analytics = await _projectPipelineService.GetPipelineAnalyticsAsync();
                return Json(analytics);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> GetPipelineAnalyticsSummary()
        {
            try
            {
                var summary = await _projectPipelineService.GetPipelineAnalyticsSummaryAsync();
                return Json(summary);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> PipelineAnalytics()
        {
            try
            {
                var analytics = await _projectPipelineService.GetPipelineAnalyticsAsync();
                return View(analytics);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading analytics: {ex.Message}";
                return View();
            }
        }

        // Week 10 Enhancement: Comprehensive Desktop Reports View
        [Authorize(Roles = "Admin,Sales,Management")]
        public IActionResult DesktopReports()
        {
            return View();
        }

        // Week 10 Enhancement: Desktop Reporting API
        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> GetDesktopReportingData()
        {
            try
            {
                var reportData = await _projectPipelineService.GetDesktopReportingDataAsync();
                return Json(reportData);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> PipelineRevenueAnalytics(string period = "next12months", string viewType = "chart")
        {
            try
            {
                // BIGIN.COM APPROACH: Pipeline Analytics = Future Estimations Only
                // Filter pipeline projects for future-looking analytics based on Expected Close Dates
                var currentDate = DateTime.Now;
                var futureStartDate = currentDate.Date; // Today onwards

                var pipelineProjects = await _context.ProjectPipelines
                    .Where(p => p.EstimatedRevenue > 0 &&
                               p.ProjectStatus == "Pipeline" && // Only active pipeline deals
                               (p.ExpectedCloseDate.HasValue && p.ExpectedCloseDate.Value >= futureStartDate))
                    .OrderBy(p => p.ExpectedCloseDate)
                    .ToListAsync();

                List<MonthlyRevenueDataPoint> revenueData = new List<MonthlyRevenueDataPoint>();

                if (!pipelineProjects.Any())
                {
                    ViewBag.NoDataMessage = "No active pipeline projects found with future Expected Close Dates. Pipeline analytics shows estimated revenue for future periods based on deal closure projections.";

                    // Create empty future periods for visualization
                    CreateEmptyFuturePeriods(revenueData, currentDate, period);
                }
                else
                {
                    switch (period.ToLower())
                    {
                        case "next12months":
                            // Future 12 months from current date
                            for (int i = 0; i < 12; i++)
                            {
                                var monthDate = currentDate.AddMonths(i);
                                var monthProjects = pipelineProjects.Where(p =>
                                    p.ExpectedCloseDate.HasValue &&
                                    p.ExpectedCloseDate.Value.Year == monthDate.Year &&
                                    p.ExpectedCloseDate.Value.Month == monthDate.Month).ToList();

                                var estimatedRevenue = monthProjects.Sum(p => p.EstimatedRevenue);
                                var weightedRevenue = monthProjects.Sum(p => p.WeightedRevenue);

                                revenueData.Add(new MonthlyRevenueDataPoint
                                {
                                    Period = monthDate.ToString("MMM yyyy"),
                                    Revenue = estimatedRevenue,
                                    WeightedRevenue = weightedRevenue,
                                    DealCount = monthProjects.Count,
                                    Month = monthDate.Month,
                                    Year = monthDate.Year,
                                    AverageSuccessProbability = monthProjects.Any() ? (decimal)monthProjects.Average(p => p.SuccessProbability) : 0
                                });
                            }
                            break;

                        case "next24months":
                            // Future 24 months from current date
                            for (int i = 0; i < 24; i++)
                            {
                                var monthDate = currentDate.AddMonths(i);
                                var monthProjects = pipelineProjects.Where(p =>
                                    p.ExpectedCloseDate.HasValue &&
                                    p.ExpectedCloseDate.Value.Year == monthDate.Year &&
                                    p.ExpectedCloseDate.Value.Month == monthDate.Month).ToList();

                                var estimatedRevenue = monthProjects.Sum(p => p.EstimatedRevenue);
                                var weightedRevenue = monthProjects.Sum(p => p.WeightedRevenue);

                                revenueData.Add(new MonthlyRevenueDataPoint
                                {
                                    Period = monthDate.ToString("MMM yyyy"),
                                    Revenue = estimatedRevenue,
                                    WeightedRevenue = weightedRevenue,
                                    DealCount = monthProjects.Count,
                                    Month = monthDate.Month,
                                    Year = monthDate.Year,
                                    AverageSuccessProbability = monthProjects.Any() ? (decimal)monthProjects.Average(p => p.SuccessProbability) : 0
                                });
                            }
                            break;

                        case "futureqoq":
                            // Future Quarters (QoQ) - Next 8 quarters
                            var futureQuarters = new[]
                            {
                                new { Name = "Q1", Months = new[] { 1, 2, 3 } },
                                new { Name = "Q2", Months = new[] { 4, 5, 6 } },
                                new { Name = "Q3", Months = new[] { 7, 8, 9 } },
                                new { Name = "Q4", Months = new[] { 10, 11, 12 } }
                            };

                            for (int yearOffset = 0; yearOffset < 2; yearOffset++) // Next 2 years
                            {
                                var targetYear = currentDate.Year + yearOffset;
                                foreach (var quarter in futureQuarters)
                                {
                                    // Skip past quarters in current year
                                    if (yearOffset == 0 && quarter.Months.Any(m => m < currentDate.Month))
                                        continue;

                                    var quarterProjects = pipelineProjects.Where(p =>
                                        p.ExpectedCloseDate.HasValue &&
                                        p.ExpectedCloseDate.Value.Year == targetYear &&
                                        quarter.Months.Contains(p.ExpectedCloseDate.Value.Month)).ToList();

                                    var estimatedRevenue = quarterProjects.Sum(p => p.EstimatedRevenue);
                                    var weightedRevenue = quarterProjects.Sum(p => p.WeightedRevenue);

                                    revenueData.Add(new MonthlyRevenueDataPoint
                                    {
                                        Period = $"{quarter.Name} {targetYear}",
                                        Revenue = estimatedRevenue,
                                        WeightedRevenue = weightedRevenue,
                                        DealCount = quarterProjects.Count,
                                        Month = quarter.Months[0],
                                        Year = targetYear,
                                        AverageSuccessProbability = quarterProjects.Any() ? (decimal)quarterProjects.Average(p => p.SuccessProbability) : 0
                                    });
                                }
                            }
                            break;

                        case "futureyoy":
                            // Future Years (YoY) - Next 3 years
                            for (int i = 0; i < 3; i++)
                            {
                                var targetYear = currentDate.Year + i;
                                var yearProjects = pipelineProjects.Where(p =>
                                    p.ExpectedCloseDate.HasValue &&
                                    p.ExpectedCloseDate.Value.Year == targetYear).ToList();

                                var estimatedRevenue = yearProjects.Sum(p => p.EstimatedRevenue);
                                var weightedRevenue = yearProjects.Sum(p => p.WeightedRevenue);

                                revenueData.Add(new MonthlyRevenueDataPoint
                                {
                                    Period = targetYear.ToString(),
                                    Revenue = estimatedRevenue,
                                    WeightedRevenue = weightedRevenue,
                                    DealCount = yearProjects.Count,
                                    Month = 1,
                                    Year = targetYear,
                                    AverageSuccessProbability = yearProjects.Any() ? (decimal)yearProjects.Average(p => p.SuccessProbability) : 0
                                });
                            }
                            break;
                    }
                }

                // Calculate month-over-month or period-over-period growth for future projections
                for (int i = 1; i < revenueData.Count; i++)
                {
                    var current = revenueData[i];
                    var previous = revenueData[i - 1];

                    if (previous.Revenue > 0)
                    {
                        current.GrowthPercentage = ((current.Revenue - previous.Revenue) / previous.Revenue) * 100;
                    }
                }

                // Calculate summary statistics
                var totalRevenue = revenueData.Sum(r => r.Revenue);
                var averageRevenue = revenueData.Count > 0 ? revenueData.Average(r => r.Revenue) : 0;
                var maxRevenue = revenueData.Count > 0 ? revenueData.Max(r => r.Revenue) : 0;
                var totalDeals = revenueData.Sum(r => r.DealCount);

                var viewModel = new MonthlyRevenueAnalyticsViewModel
                {
                    RevenueData = revenueData,
                    SelectedPeriod = period,
                    SelectedViewType = viewType,
                    TotalRevenue = totalRevenue,
                    AverageRevenue = averageRevenue,
                    MaxRevenue = maxRevenue,
                    TotalDeals = totalDeals,
                    AvailablePeriods = new Dictionary<string, string>
                    {
                        { "next12months", "Next 12 Months" },
                        { "next24months", "Next 24 Months" },
                        { "futureqoq", "Future Quarters (QoQ)" },
                        { "futureyoy", "Future Years (YoY)" }
                    },
                    AvailableViewTypes = new Dictionary<string, string>
                    {
                        { "chart", "Chart View" },
                        { "table", "Table View" },
                        { "both", "Chart & Table" }
                    }
                };

                ViewData["Title"] = "Pipeline Revenue Analytics - CBMS";
                ViewData["IsPipelineAnalytics"] = true;

                return View("~/Views/Report/MonthlyRevenueAnalytics.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Error loading pipeline revenue analytics: {ex.Message}";
                return View("~/Views/Report/MonthlyRevenueAnalytics.cshtml", new MonthlyRevenueAnalyticsViewModel());
            }
        }

        // Helper method to create empty future periods for visualization
        private void CreateEmptyFuturePeriods(List<MonthlyRevenueDataPoint> revenueData, DateTime currentDate, string period)
        {
            switch (period.ToLower())
            {
                case "next12months":
                    for (int i = 0; i < 12; i++)
                    {
                        var monthDate = currentDate.AddMonths(i);
                        revenueData.Add(new MonthlyRevenueDataPoint
                        {
                            Period = monthDate.ToString("MMM yyyy"),
                            Revenue = 0,
                            WeightedRevenue = 0,
                            DealCount = 0,
                            Month = monthDate.Month,
                            Year = monthDate.Year,
                            GrowthPercentage = 0,
                            AverageSuccessProbability = 0
                        });
                    }
                    break;

                case "next24months":
                    for (int i = 0; i < 24; i++)
                    {
                        var monthDate = currentDate.AddMonths(i);
                        revenueData.Add(new MonthlyRevenueDataPoint
                        {
                            Period = monthDate.ToString("MMM yyyy"),
                            Revenue = 0,
                            WeightedRevenue = 0,
                            DealCount = 0,
                            Month = monthDate.Month,
                            Year = monthDate.Year,
                            GrowthPercentage = 0,
                            AverageSuccessProbability = 0
                        });
                    }
                    break;

                case "futureqoq":
                    var futureQuarters = new[]
                    {
                        new { Name = "Q1", Months = new[] { 1, 2, 3 } },
                        new { Name = "Q2", Months = new[] { 4, 5, 6 } },
                        new { Name = "Q3", Months = new[] { 7, 8, 9 } },
                        new { Name = "Q4", Months = new[] { 10, 11, 12 } }
                    };

                    for (int yearOffset = 0; yearOffset < 2; yearOffset++)
                    {
                        var targetYear = currentDate.Year + yearOffset;
                        foreach (var quarter in futureQuarters)
                        {
                            if (yearOffset == 0 && quarter.Months.Any(m => m < currentDate.Month))
                                continue;

                            revenueData.Add(new MonthlyRevenueDataPoint
                            {
                                Period = $"{quarter.Name} {targetYear}",
                                Revenue = 0,
                                WeightedRevenue = 0,
                                DealCount = 0,
                                Month = quarter.Months[0],
                                Year = targetYear,
                                GrowthPercentage = 0,
                                AverageSuccessProbability = 0
                            });
                        }
                    }
                    break;

                case "futureyoy":
                    for (int i = 0; i < 3; i++)
                    {
                        var targetYear = currentDate.Year + i;
                        revenueData.Add(new MonthlyRevenueDataPoint
                        {
                            Period = targetYear.ToString(),
                            Revenue = 0,
                            WeightedRevenue = 0,
                            DealCount = 0,
                            Month = 1,
                            Year = targetYear,
                            GrowthPercentage = 0,
                            AverageSuccessProbability = 0
                        });
                    }
                    break;
            }
        }        // Week 10 Enhancement: Autocomplete APIs for Enhanced Filtering
        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> GetOemSuggestions()
        {
            try
            {
                var oems = await _projectPipelineService.GetUniqueOemNamesAsync();
                return Json(oems);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> GetCustomerSuggestions()
        {
            try
            {
                var customers = await _projectPipelineService.GetUniqueCustomerNamesAsync();
                return Json(customers);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }
    }

    // Helper ViewModel for margin updates
    public class ProjectMarginUpdateViewModel
    {
        public int ProjectPipelineId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ClientName { get; set; } = string.Empty;
        public decimal ExpectedAmountToReceive { get; set; }
        public decimal ExpectedAmountToPay { get; set; }

        [Display(Name = "Projected Margin Input")]
        [DataType(DataType.Currency)]
        public decimal? ProjectedMarginInput { get; set; }

        [Display(Name = "Margin Notes")]
        public string? MarginNotes { get; set; }

        public decimal CalculatedMargin => ExpectedAmountToReceive - ExpectedAmountToPay;
        public decimal FinalMargin => ProjectedMarginInput ?? CalculatedMargin;
    }
}
