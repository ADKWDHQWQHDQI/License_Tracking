using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using License_Tracking.ViewModels;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly AppDbContext _context;

        public ActivityController(AppDbContext context)
        {
            _context = context;
        }

        // Activity Dashboard - CRM-style follow-ups and interaction tracking
        public async Task<IActionResult> Index(string type = "all", string status = "all", int page = 1)
        {
            var activitiesQuery = _context.Activities.AsQueryable();

            // Filter by activity type
            if (type != "all")
            {
                activitiesQuery = activitiesQuery.Where(a => a.ActivityType == type);
            }

            // Filter by status
            if (status != "all")
            {
                activitiesQuery = activitiesQuery.Where(a => a.Status == status);
            }

            // Order by due date
            activitiesQuery = activitiesQuery.OrderBy(a => a.DueDate);

            var activities = await activitiesQuery
                .Skip((page - 1) * 20)
                .Take(20)
                .ToListAsync();

            // Manually load related entities for display
            await LoadRelatedEntitiesAsync(activities);

            ViewBag.ActivityTypes = new[]
            {
                "Call", "Email", "Meeting", "Follow-up", "Demo", "Proposal", "Contract"
            };

            ViewBag.CurrentType = type;
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentPage = page;

            return View(activities);
        }

        // Activity Timeline for specific Deal
        public async Task<IActionResult> DealTimeline(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Include(d => d.Oem)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            var activities = await _context.Activities
                .Where(a => a.RelatedEntityType == "Deal" && a.RelatedEntityId == dealId)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();

            var viewModel = new DealTimelineViewModel
            {
                Deal = deal,
                Activities = activities
            };

            return View(viewModel);
        }

        // Create new activity
        [HttpGet]
        public IActionResult Create(string entityType = "Deal", int entityId = 0)
        {
            var activity = new Activity
            {
                RelatedEntityType = entityType,
                RelatedEntityId = entityId,
                ActivityDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(1),
                Priority = "Medium",
                Status = "Pending"
            };

            ViewBag.ActivityTypes = new[]
            {
                "Call", "Email", "Meeting", "Follow-up", "Demo", "Proposal", "Contract"
            };

            ViewBag.Priorities = new[] { "Low", "Medium", "High" };

            // Load related entities for dropdowns
            LoadEntityDropdowns();

            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Activity activity)
        {
            // Validate RelatedEntityId exists in the correct table
            if (activity.RelatedEntityId > 0 && !string.IsNullOrEmpty(activity.RelatedEntityType))
            {
                bool entityExists = false;
                switch (activity.RelatedEntityType.ToLower())
                {
                    case "deal":
                        entityExists = await _context.Deals.AnyAsync(d => d.DealId == activity.RelatedEntityId);
                        break;
                    case "company":
                        entityExists = await _context.Companies.AnyAsync(c => c.CompanyId == activity.RelatedEntityId);
                        break;
                    case "contact":
                        entityExists = await _context.ContactPersons.AnyAsync(cp => cp.ContactId == activity.RelatedEntityId);
                        break;
                }

                if (!entityExists)
                {
                    ModelState.AddModelError("RelatedEntityId", $"The specified {activity.RelatedEntityType} does not exist.");
                }
            }

            if (ModelState.IsValid)
            {
                activity.CreatedDate = DateTime.Now;
                activity.CreatedBy = User.Identity?.Name;
                activity.AssignedTo = User.Identity?.Name; // Default to current user

                _context.Add(activity);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Activity created successfully!";

                // Redirect back to entity if specified
                if (activity.RelatedEntityType == "Deal" && activity.RelatedEntityId > 0)
                {
                    return RedirectToAction("Details", "Deals", new { id = activity.RelatedEntityId });
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.ActivityTypes = new[]
            {
                "Call", "Email", "Meeting", "Follow-up", "Demo", "Proposal", "Contract"
            };

            ViewBag.Priorities = new[] { "Low", "Medium", "High" };
            LoadEntityDropdowns();

            return View(activity);
        }

        // Edit activity
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            ViewBag.ActivityTypes = new[]
            {
                "Call", "Email", "Meeting", "Follow-up", "Demo", "Proposal", "Contract"
            };

            ViewBag.Priorities = new[] { "Low", "Medium", "High" };
            ViewBag.Statuses = new[] { "Pending", "Completed", "Cancelled" };

            LoadEntityDropdowns();

            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Activity activity)
        {
            if (id != activity.ActivityId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingActivity = await _context.Activities.FindAsync(id);
                    if (existingActivity == null)
                    {
                        return NotFound();
                    }

                    // Update fields
                    existingActivity.Subject = activity.Subject;
                    existingActivity.Description = activity.Description;
                    existingActivity.ActivityType = activity.ActivityType;
                    existingActivity.ActivityDate = activity.ActivityDate;
                    existingActivity.DueDate = activity.DueDate;
                    existingActivity.Status = activity.Status;
                    existingActivity.Priority = activity.Priority;
                    existingActivity.AssignedTo = activity.AssignedTo;

                    if (activity.Status == "Completed" && existingActivity.CompletedDate == null)
                    {
                        existingActivity.CompletedDate = DateTime.Now;
                        existingActivity.CompletedBy = User.Identity?.Name;
                    }

                    _context.Update(existingActivity);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Activity updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ActivityExists(activity.ActivityId))
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

            ViewBag.ActivityTypes = new[]
            {
                "Call", "Email", "Meeting", "Follow-up", "Demo", "Proposal", "Contract"
            };

            ViewBag.Priorities = new[] { "Low", "Medium", "High" };
            ViewBag.Statuses = new[] { "Pending", "Completed", "Cancelled" };

            LoadEntityDropdowns();

            return View(activity);
        }

        [HttpGet]
        public async Task<IActionResult> GetEntitiesByType(string entityType)
        {
            try
            {
                var entities = new List<object>();

                switch (entityType?.ToLower())
                {
                    case "deal":
                        var deals = await _context.Deals
                            .Select(d => new { value = d.DealId, text = d.DealName })
                            .ToListAsync();
                        entities.AddRange(deals);
                        break;

                    case "company":
                        var companies = await _context.Companies
                            .Select(c => new { value = c.CompanyId, text = c.CompanyName })
                            .ToListAsync();
                        entities.AddRange(companies);
                        break;

                    case "contact":
                        var contacts = await _context.ContactPersons
                            .Select(cp => new { value = cp.ContactId, text = cp.Name })
                            .ToListAsync();
                        entities.AddRange(contacts);
                        break;
                }

                return Json(entities);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error loading entities: {ex.Message}");
            }
        }

        // Activity Details
        public async Task<IActionResult> Details(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity == null)
            {
                return NotFound();
            }

            return View(activity);
        }

        // Delete activity
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var activity = await _context.Activities.FindAsync(id);
            if (activity != null)
            {
                _context.Activities.Remove(activity);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Activity deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // Weekly Activity Report
        public async Task<IActionResult> WeeklyReport()
        {
            var startOfWeek = DateTime.Now.AddDays(-(int)DateTime.Now.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var activities = await _context.Activities
                .Where(a => a.ActivityDate >= startOfWeek && a.ActivityDate < endOfWeek)
                .OrderBy(a => a.ActivityDate)
                .ToListAsync();

            var report = new WeeklyActivityReportViewModel
            {
                StartDate = startOfWeek,
                EndDate = endOfWeek,
                Activities = activities,
                TotalActivities = activities.Count,
                CompletedActivities = activities.Count(a => a.Status == "Completed"),
                PendingActivities = activities.Count(a => a.Status == "Pending"),
                OverdueActivities = activities.Count(a => a.Status == "Pending" && a.DueDate < DateTime.Now)
            };

            return View(report);
        }

        private bool ActivityExists(int id)
        {
            return _context.Activities.Any(e => e.ActivityId == id);
        }

        private void LoadEntityDropdowns()
        {
            // Load deals for selection
            ViewBag.Deals = _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Product)
                .Select(d => new
                {
                    Id = d.DealId,
                    Name = $"{d.Company!.CompanyName} - {d.Product!.ProductName}"
                })
                .ToList();

            // Load companies for selection
            ViewBag.Companies = _context.Companies
                .Select(c => new
                {
                    Id = c.CompanyId,
                    Name = c.CompanyName
                })
                .ToList();

            // Load users for assignment
            ViewBag.Users = new[]
            {
                "john.doe@canarys.com",
                "jane.smith@canarys.com",
                "mike.johnson@canarys.com",
                "sarah.wilson@canarys.com"
            };
        }

        // API endpoint for Deal Details timeline integration
        [HttpGet]
        public async Task<JsonResult> GetDealActivities(int dealId)
        {
            try
            {
                var activities = await _context.Activities
                    .Where(a => a.RelatedEntityType == "Deal" && a.RelatedEntityId == dealId)
                    .OrderByDescending(a => a.ActivityDate)
                    .Take(10) // Show latest 10 activities
                    .Select(a => new
                    {
                        a.ActivityId,
                        a.Subject,
                        a.Description,
                        a.ActivityType,
                        a.ActivityDate,
                        a.Status,
                        a.AssignedTo,
                        a.Priority
                    })
                    .ToListAsync();

                return Json(activities);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Week 11: Enhanced CRM-style Activity Management Methods

        [HttpPost]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            try
            {
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    return Json(new { success = false, message = "Activity not found" });
                }

                activity.Status = "Completed";
                activity.CompletedDate = DateTime.Now;
                activity.CompletedBy = User.Identity?.Name;
                activity.UpdatedDate = DateTime.Now;
                activity.UpdatedBy = User.Identity?.Name;

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Activity marked as completed" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Advanced filtering for desktop workflow
        [HttpGet]
        public async Task<IActionResult> GetFilteredActivities(
            string? type = null,
            string? status = null,
            string? priority = null,
            string? assignedTo = null,
            string? dateRange = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string? entityType = null,
            string? search = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.Activities.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(type) && type != "all")
                    query = query.Where(a => a.Type == type);

                if (!string.IsNullOrEmpty(status) && status != "all")
                    query = query.Where(a => a.Status == status);

                if (!string.IsNullOrEmpty(priority) && priority != "all")
                    query = query.Where(a => a.Priority == priority);

                if (!string.IsNullOrEmpty(assignedTo))
                    query = query.Where(a => a.AssignedTo != null && a.AssignedTo.Contains(assignedTo));

                if (!string.IsNullOrEmpty(entityType) && entityType != "all")
                    query = query.Where(a => a.EntityType == entityType);

                // Date range filtering
                if (!string.IsNullOrEmpty(dateRange))
                {
                    var today = DateTime.Today;
                    switch (dateRange)
                    {
                        case "today":
                            query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date == today);
                            break;
                        case "thisWeek":
                            var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                            var endOfWeek = startOfWeek.AddDays(6);
                            query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date >= startOfWeek && a.DueDate.Value.Date <= endOfWeek);
                            break;
                        case "thisMonth":
                            var startOfMonth = new DateTime(today.Year, today.Month, 1);
                            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                            query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date >= startOfMonth && a.DueDate.Value.Date <= endOfMonth);
                            break;
                        case "overdue":
                            query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.Status != "Completed");
                            break;
                        case "custom":
                            if (dateFrom.HasValue)
                                query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date >= dateFrom.Value.Date);
                            if (dateTo.HasValue)
                                query = query.Where(a => a.DueDate.HasValue && a.DueDate.Value.Date <= dateTo.Value.Date);
                            break;
                    }
                }

                // Global search
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(a =>
                        a.Subject.Contains(search) ||
                        (a.Description != null && a.Description.Contains(search)) ||
                        (a.Notes != null && a.Notes.Contains(search)) ||
                        (a.AssignedTo != null && a.AssignedTo.Contains(search)));
                }

                // Get total count for pagination
                var totalCount = await query.CountAsync();

                // Apply pagination and ordering
                var activities = await query
                    .OrderByDescending(a => a.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.ActivityId,
                        a.Subject,
                        a.Description,
                        a.Type,
                        a.Status,
                        a.Priority,
                        a.DueDate,
                        a.AssignedTo,
                        a.EntityType,
                        a.EntityId,
                        a.CreatedDate,
                        IsOverdue = a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.Status != "Completed",
                        IsDueToday = a.DueDate.HasValue && a.DueDate.Value.Date == DateTime.Today
                    })
                    .ToListAsync();

                return Json(new
                {
                    success = true,
                    data = activities,
                    totalCount = totalCount,
                    pageSize = pageSize,
                    currentPage = page,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Activity export functionality
        [HttpGet]
        public async Task<IActionResult> Export(
            string? type = null,
            string? status = null,
            string? priority = null,
            string? assignedTo = null,
            string? dateRange = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string? entityType = null,
            string? search = null)
        {
            try
            {
                // Use the same filtering logic as GetFilteredActivities
                var query = _context.Activities.AsQueryable();

                // Apply all the same filters...
                if (!string.IsNullOrEmpty(type) && type != "all")
                    query = query.Where(a => a.Type == type);

                if (!string.IsNullOrEmpty(status) && status != "all")
                    query = query.Where(a => a.Status == status);

                // ... (repeat other filters)

                var activities = await query
                    .OrderByDescending(a => a.CreatedDate)
                    .ToListAsync();

                // Create CSV content
                var csv = new System.Text.StringBuilder();
                csv.AppendLine("Subject,Type,Status,Priority,Due Date,Assigned To,Description,Created Date");

                foreach (var activity in activities)
                {
                    csv.AppendLine($"\"{activity.Subject}\",\"{activity.Type}\",\"{activity.Status}\",\"{activity.Priority}\"," +
                                 $"\"{activity.DueDate?.ToString("yyyy-MM-dd HH:mm")}\",\"{activity.AssignedTo}\"," +
                                 $"\"{activity.Description?.Replace("\"", "\"\"")}\",\"{activity.CreatedDate:yyyy-MM-dd HH:mm}\"");
                }

                var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
                return File(bytes, "text/csv", $"activities_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Export failed: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // Autocomplete for assignees
        [HttpGet]
        public async Task<IActionResult> GetAssigneeOptions(string term = "")
        {
            try
            {
                var assignees = await _context.Activities
                    .Where(a => !string.IsNullOrEmpty(a.AssignedTo) &&
                               (string.IsNullOrEmpty(term) || a.AssignedTo.Contains(term)))
                    .Select(a => a.AssignedTo)
                    .Distinct()
                    .Take(10)
                    .ToListAsync();

                return Json(assignees);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Activity analytics for desktop dashboard
        [HttpGet]
        public async Task<IActionResult> GetActivityAnalytics()
        {
            try
            {
                var today = DateTime.Today;
                var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
                var startOfMonth = new DateTime(today.Year, today.Month, 1);

                var analytics = new
                {
                    TotalActivities = await _context.Activities.CountAsync(),
                    PendingActivities = await _context.Activities.CountAsync(a => a.Status == "Pending"),
                    CompletedToday = await _context.Activities.CountAsync(a =>
                        a.Status == "Completed" && a.CompletedDate.HasValue && a.CompletedDate.Value.Date == today),
                    DueToday = await _context.Activities.CountAsync(a =>
                        a.DueDate.HasValue && a.DueDate.Value.Date == today && a.Status != "Completed"),
                    Overdue = await _context.Activities.CountAsync(a =>
                        a.DueDate.HasValue && a.DueDate.Value < DateTime.Now && a.Status != "Completed"),
                    ThisWeekCompleted = await _context.Activities.CountAsync(a =>
                        a.Status == "Completed" && a.CompletedDate.HasValue && a.CompletedDate.Value >= startOfWeek),
                    ThisMonthCreated = await _context.Activities.CountAsync(a => a.CreatedDate >= startOfMonth),
                    ByType = await _context.Activities
                        .GroupBy(a => a.Type)
                        .Select(g => new { Type = g.Key, Count = g.Count() })
                        .OrderByDescending(x => x.Count)
                        .ToListAsync(),
                    ByStatus = await _context.Activities
                        .GroupBy(a => a.Status)
                        .Select(g => new { Status = g.Key, Count = g.Count() })
                        .ToListAsync(),
                    ByPriority = await _context.Activities
                        .GroupBy(a => a.Priority)
                        .Select(g => new { Priority = g.Key, Count = g.Count() })
                        .ToListAsync()
                };

                return Json(analytics);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // Bulk actions for selected activities
        [HttpPost]
        public async Task<IActionResult> BulkAction(string action, int[] activityIds)
        {
            try
            {
                if (activityIds == null || activityIds.Length == 0)
                {
                    return Json(new { success = false, message = "No activities selected" });
                }

                var activities = await _context.Activities
                    .Where(a => activityIds.Contains(a.ActivityId))
                    .ToListAsync();

                var affectedCount = 0;

                switch (action.ToLower())
                {
                    case "complete":
                        foreach (var activity in activities.Where(a => a.Status != "Completed"))
                        {
                            activity.Status = "Completed";
                            activity.CompletedDate = DateTime.Now;
                            activity.CompletedBy = User.Identity?.Name;
                            activity.UpdatedDate = DateTime.Now;
                            activity.UpdatedBy = User.Identity?.Name;
                            affectedCount++;
                        }
                        break;

                    case "delete":
                        _context.Activities.RemoveRange(activities);
                        affectedCount = activities.Count;
                        break;

                    case "reassign":
                        // This would need additional parameter for new assignee
                        return Json(new { success = false, message = "Reassign action requires new assignee parameter" });

                    default:
                        return Json(new { success = false, message = "Unknown action" });
                }

                await _context.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = $"{affectedCount} activities {action}d successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Helper method to load related entities for activities
        private async Task LoadRelatedEntitiesAsync(IEnumerable<Activity> activities)
        {
            foreach (var activity in activities)
            {
                if (!string.IsNullOrEmpty(activity.RelatedEntityType) && activity.RelatedEntityId > 0)
                {
                    switch (activity.RelatedEntityType.ToLower())
                    {
                        case "deal":
                            activity.Deal = await _context.Deals
                                .Include(d => d.Company)
                                .Include(d => d.Product)
                                .FirstOrDefaultAsync(d => d.DealId == activity.RelatedEntityId);
                            break;
                        case "company":
                            activity.Company = await _context.Companies
                                .FirstOrDefaultAsync(c => c.CompanyId == activity.RelatedEntityId);
                            break;
                        case "contact":
                            activity.ContactPerson = await _context.ContactPersons
                                .Include(cp => cp.Company)
                                .FirstOrDefaultAsync(cp => cp.ContactId == activity.RelatedEntityId);
                            break;
                    }
                }
            }
        }
    }
}
