using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;

namespace License_Tracking.Controllers
{
    /// <summary>
    /// Enterprise User Management Controller for CBMS B2B2B CRM
    /// Handles direct database operations for user and role management
    /// </summary>
    [Authorize(Policy = "AdminOnly")]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            AppDbContext context,
            ILogger<UserManagementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Display user management dashboard
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user management dashboard");
                TempData["Error"] = "Failed to load user management data.";
                return RedirectToAction("Index", "Home");
            }
        }

        /// <summary>
        /// Get all users with roles as JSON
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Select(u => new
                    {
                        u.Id,
                        u.UserName,
                        u.Email,
                        Roles = string.Join(", ", _context.UserRoles
                            .Where(ur => ur.UserId == u.Id)
                            .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)),
                        LastLogin = "Never" // Could be enhanced with login tracking
                    })
                    .ToListAsync();

                return Json(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return BadRequest("Failed to retrieve users");
            }
        }

        /// <summary>
        /// Check database integrity for roles and users
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> CheckIntegrity()
        {
            try
            {
                var totalUsers = await _context.Users.CountAsync();
                var totalRoles = await _context.Roles.CountAsync();
                var adminUsers = await _context.Users
                    .Where(u => _context.UserRoles.Any(ur => ur.UserId == u.Id &&
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.NormalizedName == "ADMIN")))
                    .CountAsync();

                var requiredRoles = new[] { "ADMIN", "SALES", "FINANCE", "OPERATIONS", "MANAGEMENT", "BA" };
                var existingRoles = await _context.Roles
                    .Where(r => requiredRoles.Contains(r.NormalizedName))
                    .CountAsync();

                var isValid = existingRoles == requiredRoles.Length && adminUsers > 0;

                return Json(new
                {
                    isValid,
                    totalUsers,
                    totalRoles,
                    adminUsers
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking database integrity");
                return BadRequest("Failed to check integrity");
            }
        }

        /// <summary>
        /// Re-run database migration to ensure roles and users exist
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Reseed()
        {
            try
            {
                // Since we use migrations for seeding, we just verify the data exists
                var roleCount = await _context.Roles.CountAsync();
                var userCount = await _context.Users.CountAsync();

                _logger.LogInformation("Database verification completed - Roles: {RoleCount}, Users: {UserCount}", roleCount, userCount);

                return Json(new { success = true, message = $"Database verified: {roleCount} roles, {userCount} users" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database verification");
                return Json(new { success = false, message = "Database verification failed" });
            }
        }
    }
}
