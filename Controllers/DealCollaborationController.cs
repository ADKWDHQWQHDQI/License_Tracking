using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace License_Tracking.Controllers
{
    [Authorize]
    public class DealCollaborationController : Controller
    {
        private readonly AppDbContext _context;

        public DealCollaborationController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Deal Collaboration Dashboard
        public async Task<IActionResult> Index(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .Include(d => d.ContactPerson)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            // Get recent activities
            var activities = await _context.DealActivities
                .Where(a => a.DealId == dealId)
                .OrderByDescending(a => a.ActivityDate)
                .Take(20)
                .ToListAsync();

            // Get team members assigned to this deal
            var teamMembers = await _context.DealActivities
                .Where(a => a.DealId == dealId && !string.IsNullOrEmpty(a.AssignedTo))
                .Select(a => a.AssignedTo)
                .Distinct()
                .ToListAsync();

            // Add current deal assignee if not in list
            if (!string.IsNullOrEmpty(deal.AssignedTo) && !teamMembers.Contains(deal.AssignedTo))
            {
                teamMembers.Add(deal.AssignedTo);
            }

            ViewBag.Deal = deal;
            ViewBag.Activities = activities;
            ViewBag.TeamMembers = teamMembers;

            return View();
        }

        // POST: Add Activity/Comment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddActivity(int dealId, string activityType, string title, string description, string assignedTo, string priority, DateTime? dueDate)
        {
            var deal = await _context.Deals.FindAsync(dealId);
            if (deal == null)
            {
                return NotFound();
            }

            var activity = new DealCollaborationActivity
            {
                DealId = dealId,
                ActivityType = activityType,
                ActivityTitle = title,
                ActivityDescription = description,
                AssignedTo = assignedTo,
                Priority = priority,
                DueDate = dueDate,
                PerformedBy = User.Identity?.Name,
                CreatedBy = User.Identity?.Name
            };

            _context.DealActivities.Add(activity);
            await _context.SaveChangesAsync();

            // If it's an assignment, update deal assigned to
            if (activityType == "Assignment" && !string.IsNullOrEmpty(assignedTo))
            {
                deal.AssignedTo = assignedTo;
                deal.LastModifiedDate = DateTime.Now;
                deal.LastModifiedBy = User.Identity?.Name;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { dealId });
        }

        // POST: Update Activity Status
        [HttpPost]
        public async Task<IActionResult> UpdateActivityStatus(int activityId, string status)
        {
            var activity = await _context.DealActivities.FindAsync(activityId);
            if (activity == null)
            {
                return NotFound();
            }

            activity.Status = status;
            activity.LastModifiedDate = DateTime.Now;
            activity.LastModifiedBy = User.Identity?.Name;

            if (status == "Completed")
            {
                activity.ActivityDate = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { dealId = activity.DealId });
        }

        // GET: Team Performance Dashboard
        public async Task<IActionResult> TeamDashboard()
        {
            var teamPerformance = await _context.DealActivities
                .Where(a => !string.IsNullOrEmpty(a.AssignedTo))
                .GroupBy(a => a.AssignedTo)
                .Select(g => new TeamPerformanceViewModel
                {
                    TeamMember = g.Key!,
                    TotalActivities = g.Count(),
                    CompletedActivities = g.Count(a => a.Status == "Completed"),
                    PendingActivities = g.Count(a => a.Status == "Pending"),
                    OverdueActivities = g.Count(a => a.IsOverdue),
                    HighPriorityActivities = g.Count(a => a.Priority == "High" || a.Priority == "Critical")
                })
                .ToListAsync();

            // Get deal assignments per team member
            var dealAssignments = await _context.Deals
                .Where(d => d.IsActive && !string.IsNullOrEmpty(d.AssignedTo))
                .GroupBy(d => d.AssignedTo)
                .Select(g => new
                {
                    TeamMember = g.Key,
                    ActiveDeals = g.Count(),
                    TotalValue = g.Sum(d => d.CustomerInvoiceAmount ?? 0)
                })
                .ToListAsync();

            foreach (var performance in teamPerformance)
            {
                var assignment = dealAssignments.FirstOrDefault(a => a.TeamMember == performance.TeamMember);
                if (assignment != null)
                {
                    performance.ActiveDeals = assignment.ActiveDeals;
                    performance.TotalDealValue = assignment.TotalValue;
                }
            }

            return View(teamPerformance);
        }

        // GET: Activity Timeline for Deal
        public async Task<IActionResult> Timeline(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            var activities = await _context.DealActivities
                .Where(a => a.DealId == dealId)
                .OrderBy(a => a.ActivityDate)
                .ToListAsync();

            ViewBag.Deal = deal;
            return View(activities);
        }

        // POST: Reassign Deal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReassignDeal(int dealId, string newAssignee, string reason)
        {
            var deal = await _context.Deals.FindAsync(dealId);
            if (deal == null)
            {
                return NotFound();
            }

            var previousAssignee = deal.AssignedTo;
            deal.AssignedTo = newAssignee;
            deal.LastModifiedDate = DateTime.Now;
            deal.LastModifiedBy = User.Identity?.Name;

            // Create activity for reassignment
            var activity = new DealCollaborationActivity
            {
                DealId = dealId,
                ActivityType = "Assignment",
                ActivityTitle = $"Deal Reassigned",
                ActivityDescription = $"Deal reassigned from {previousAssignee} to {newAssignee}. Reason: {reason}",
                AssignedTo = newAssignee,
                PerformedBy = User.Identity?.Name,
                CreatedBy = User.Identity?.Name,
                Status = "Completed"
            };

            _context.DealActivities.Add(activity);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { dealId });
        }

        // POST: Add Quick Comment
        [HttpPost]
        public async Task<IActionResult> AddQuickComment(int dealId, string comment)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                return BadRequest("Comment cannot be empty");
            }

            var activity = new DealCollaborationActivity
            {
                DealId = dealId,
                ActivityType = "Comment",
                ActivityTitle = "Quick Comment",
                ActivityDescription = comment,
                PerformedBy = User.Identity?.Name,
                CreatedBy = User.Identity?.Name,
                Status = "Completed"
            };

            _context.DealActivities.Add(activity);
            await _context.SaveChangesAsync();

            return Ok(new { success = true, message = "Comment added successfully" });
        }

        // GET: Export Deal Activity Report
        public async Task<IActionResult> ExportActivityReport(int dealId)
        {
            var deal = await _context.Deals
                .Include(d => d.Company)
                .Include(d => d.Oem)
                .Include(d => d.Product)
                .FirstOrDefaultAsync(d => d.DealId == dealId);

            if (deal == null)
            {
                return NotFound();
            }

            var activities = await _context.DealActivities
                .Where(a => a.DealId == dealId)
                .OrderByDescending(a => a.ActivityDate)
                .ToListAsync();

            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Deal Activity Report - {deal.DealName}");
            csv.AppendLine($"Company: {deal.Company?.CompanyName}");
            csv.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            csv.AppendLine("");
            csv.AppendLine("Date,Type,Title,Description,Performed By,Assigned To,Status,Priority");

            foreach (var activity in activities)
            {
                csv.AppendLine($"\"{activity.ActivityDate:yyyy-MM-dd HH:mm}\",\"{activity.ActivityType}\",\"{activity.ActivityTitle}\",\"{activity.ActivityDescription}\",\"{activity.PerformedBy}\",\"{activity.AssignedTo}\",\"{activity.Status}\",\"{activity.Priority}\"");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"Deal_{dealId}_Activity_Report_{DateTime.Now:yyyyMMdd}.csv");
        }
    }

    // Team Performance ViewModel
    public class TeamPerformanceViewModel
    {
        public string TeamMember { get; set; } = string.Empty;
        public int TotalActivities { get; set; }
        public int CompletedActivities { get; set; }
        public int PendingActivities { get; set; }
        public int OverdueActivities { get; set; }
        public int HighPriorityActivities { get; set; }
        public int ActiveDeals { get; set; }
        public decimal TotalDealValue { get; set; }

        public double CompletionRate => TotalActivities > 0 ? (double)CompletedActivities / TotalActivities * 100 : 0;
        public string PerformanceIndicator => CompletionRate >= 80 ? "Excellent" : CompletionRate >= 60 ? "Good" : "Needs Improvement";
    }
}
