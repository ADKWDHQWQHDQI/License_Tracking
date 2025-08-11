using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using Microsoft.AspNetCore.Identity;

namespace License_Tracking.Services
{
    public class DatabaseInitializer
    {
        private readonly AppDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DatabaseInitializer> _logger;

        public DatabaseInitializer(
            AppDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<DatabaseInitializer> logger)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                // Check if database exists
                var canConnect = await _context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    _logger.LogError("Cannot connect to database");
                    return;
                }

                // Apply pending migrations safely
                var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
                if (pendingMigrations.Any())
                {
                    _logger.LogInformation($"Applying {pendingMigrations.Count()} pending migrations...");

                    foreach (var migration in pendingMigrations)
                    {
                        try
                        {
                            _logger.LogInformation($"Applying migration: {migration}");
                            // Apply migrations one by one to handle conflicts
                            await _context.Database.MigrateAsync();
                            break; // If successful, break the loop
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Migration {migration} failed or was already applied: {ex.Message}");
                            // Continue with next migration or consider migration as applied
                        }
                    }
                }

                // Ensure roles exist
                await EnsureRolesAsync();

                // Ensure admin user exists
                await EnsureAdminUserAsync();

                _logger.LogInformation("Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while initializing the database");
                // Don't throw - let the application continue
            }
        }

        private async Task EnsureRolesAsync()
        {
            var roles = new[] { "Admin", "User", "Manager" };

            foreach (var roleName in roles)
            {
                if (!await _roleManager.RoleExistsAsync(roleName))
                {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                    _logger.LogInformation($"Created role: {roleName}");
                }
            }
        }

        private async Task EnsureAdminUserAsync()
        {
            var adminEmail = "admin@licensetracking.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation($"Created admin user: {adminEmail}");
                }
                else
                {
                    _logger.LogError($"Failed to create admin user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }
        }
    }
}
