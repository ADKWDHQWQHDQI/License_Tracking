using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using License_Tracking.Services;
using License_Tracking.ViewModels;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class ProjectPipelineController : Controller
    {
        private readonly IProjectPipelineService _projectPipelineService;
        private readonly UserManager<IdentityUser> _userManager;

        public ProjectPipelineController(
            IProjectPipelineService projectPipelineService,
            UserManager<IdentityUser> userManager)
        {
            _projectPipelineService = projectPipelineService;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Sales,Management")]
        public async Task<IActionResult> Index(string? statusFilter = null, string? oemFilter = null, string? customerFilter = null, string view = "list")
        {
            try
            {
                var projects = await _projectPipelineService.GetFilteredProjectsAsync(statusFilter, oemFilter, customerFilter);

                // Store view type for the template
                ViewBag.CurrentView = view.ToLower();
                ViewBag.StatusFilter = statusFilter;
                ViewBag.OemFilter = oemFilter;
                ViewBag.CustomerFilter = customerFilter;

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
