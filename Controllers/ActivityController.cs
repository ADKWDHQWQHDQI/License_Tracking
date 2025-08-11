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

        // Mark activity as completed
        [HttpPost]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            try
            {
                var activity = await _context.Activities.FindAsync(id);
                if (activity == null)
                {
                    return NotFound();
                }

                activity.Status = "Completed";
                activity.CompletedDate = DateTime.Now;

                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest($"Error marking activity as completed: {ex.Message}");
            }
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
    }
}
