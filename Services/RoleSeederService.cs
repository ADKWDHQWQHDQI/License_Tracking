using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using License_Tracking.Data;
using System.Security.Cryptography;
using System.Text;

namespace License_Tracking.Services
{
    public class RoleSeederService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleSeederService> _logger;

        public RoleSeederService(AppDbContext context, ILogger<RoleSeederService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Seeds default roles and admin user directly to database for CBMS system
        /// </summary>
        public async Task SeedRolesAndAdminAsync()
        {
            try
            {
                // Define CBMS B2B2B Roles based on BRD requirements
                var roles = new[]
                {
                    new { Id = Guid.NewGuid().ToString(), Name = "Admin", NormalizedName = "ADMIN" },
                    new { Id = Guid.NewGuid().ToString(), Name = "Sales", NormalizedName = "SALES" },
                    new { Id = Guid.NewGuid().ToString(), Name = "Finance", NormalizedName = "FINANCE" },
                    new { Id = Guid.NewGuid().ToString(), Name = "Operations", NormalizedName = "OPERATIONS" },
                    new { Id = Guid.NewGuid().ToString(), Name = "Management", NormalizedName = "MANAGEMENT" },
                    new { Id = Guid.NewGuid().ToString(), Name = "BA", NormalizedName = "BA" }
                };

                // Insert roles directly into database
                foreach (var roleData in roles)
                {
                    var existingRole = await _context.Roles
                        .FirstOrDefaultAsync(r => r.NormalizedName == roleData.NormalizedName);

                    if (existingRole == null)
                    {
                        var role = new IdentityRole
                        {
                            Id = roleData.Id,
                            Name = roleData.Name,
                            NormalizedName = roleData.NormalizedName,
                            ConcurrencyStamp = Guid.NewGuid().ToString()
                        };

                        _context.Roles.Add(role);
                        _logger.LogInformation($"Role '{roleData.Name}' added to database");
                    }
                    else
                    {
                        _logger.LogInformation($"Role '{roleData.Name}' already exists in database");
                    }
                }

                await _context.SaveChangesAsync();

                // Create default admin user
                await CreateDefaultAdminAsync();

                _logger.LogInformation("CBMS enterprise role seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during enterprise role seeding");
                throw;
            }
        }

        /// <summary>
        /// Creates default admin user with direct database operations
        /// </summary>
        private async Task CreateDefaultAdminAsync()
        {
            const string adminEmail = "admin@cbms.com";
            const string adminUserName = "admin@cbms.com";
            const string adminPassword = "Admin@123";

            var existingAdmin = await _context.Users
                .FirstOrDefaultAsync(u => u.NormalizedEmail == adminEmail.ToUpperInvariant());

            if (existingAdmin == null)
            {
                var adminUserId = Guid.NewGuid().ToString();
                var passwordHash = HashPassword(adminPassword);

                var admin = new IdentityUser
                {
                    Id = adminUserId,
                    UserName = adminUserName,
                    NormalizedUserName = adminUserName.ToUpperInvariant(),
                    Email = adminEmail,
                    NormalizedEmail = adminEmail.ToUpperInvariant(),
                    EmailConfirmed = true,
                    PasswordHash = passwordHash,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    PhoneNumberConfirmed = false,
                    TwoFactorEnabled = false,
                    LockoutEnabled = true,
                    AccessFailedCount = 0
                };

                _context.Users.Add(admin);
                await _context.SaveChangesAsync();

                // Assign Admin role to user
                var adminRole = await _context.Roles
                    .FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");

                if (adminRole != null)
                {
                    var userRole = new IdentityUserRole<string>
                    {
                        UserId = adminUserId,
                        RoleId = adminRole.Id
                    };

                    _context.UserRoles.Add(userRole);
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Default admin user created in database: {adminEmail}");
            }
            else
            {
                _logger.LogInformation($"Admin user already exists in database: {adminEmail}");
            }
        }

        /// <summary>
        /// Creates sample users for development and testing with direct database operations
        /// </summary>
        public async Task SeedSampleUsersAsync()
        {
            var sampleUsers = new[]
            {
                new { Email = "sales@cbms.com", Password = "Sales@123", Role = "SALES" },
                new { Email = "finance@cbms.com", Password = "Finance@123", Role = "FINANCE" },
                new { Email = "operations@cbms.com", Password = "Operations@123", Role = "OPERATIONS" },
                new { Email = "management@cbms.com", Password = "Management@123", Role = "MANAGEMENT" },
                new { Email = "ba@cbms.com", Password = "BA@123", Role = "BA" }
            };

            foreach (var userData in sampleUsers)
            {
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.NormalizedEmail == userData.Email.ToUpperInvariant());

                if (existingUser == null)
                {
                    var userId = Guid.NewGuid().ToString();
                    var passwordHash = HashPassword(userData.Password);

                    var user = new IdentityUser
                    {
                        Id = userId,
                        UserName = userData.Email,
                        NormalizedUserName = userData.Email.ToUpperInvariant(),
                        Email = userData.Email,
                        NormalizedEmail = userData.Email.ToUpperInvariant(),
                        EmailConfirmed = true,
                        PasswordHash = passwordHash,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        PhoneNumberConfirmed = false,
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0
                    };

                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // Assign role to user
                    var role = await _context.Roles
                        .FirstOrDefaultAsync(r => r.NormalizedName == userData.Role);

                    if (role != null)
                    {
                        var userRole = new IdentityUserRole<string>
                        {
                            UserId = userId,
                            RoleId = role.Id
                        };

                        _context.UserRoles.Add(userRole);
                        await _context.SaveChangesAsync();
                    }

                    _logger.LogInformation($"Sample user created in database: {userData.Email} with role {userData.Role}");
                }
                else
                {
                    _logger.LogInformation($"Sample user already exists: {userData.Email}");
                }
            }
        }

        /// <summary>
        /// Hash password using ASP.NET Core Identity compatible algorithm
        /// </summary>
        private static string HashPassword(string password)
        {
            // Use Identity V3 compatible password hashing
            var hasher = new PasswordHasher<IdentityUser>();
            return hasher.HashPassword(null!, password);
        }

        /// <summary>
        /// Gets role permissions mapping for frontend display
        /// </summary>
        public Dictionary<string, string[]> GetRolePermissions()
        {
            return new Dictionary<string, string[]>
            {
                ["Admin"] = new[] { "Full System Access", "User Management", "System Configuration", "Database Operations" },
                ["Sales"] = new[] { "Customer Management", "Deal Creation", "Contact Management", "Pipeline Access" },
                ["Finance"] = new[] { "Invoice Management", "Payment Tracking", "Financial Reports", "Billing Operations" },
                ["Operations"] = new[] { "OEM Management", "Product Catalog", "Procurement", "Vendor Relations" },
                ["Management"] = new[] { "Analytics Dashboard", "Performance Reports", "Strategic Overview", "Team Management" },
                ["BA"] = new[] { "Pipeline Analytics", "Target Tracking", "Performance Metrics", "Business Intelligence" }
            };
        }

        /// <summary>
        /// Get all users with their roles from database
        /// </summary>
        public async Task<List<object>> GetAllUsersWithRolesAsync()
        {
            var usersWithRoles = await _context.Users
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    u.EmailConfirmed,
                    u.LockoutEnabled,
                    u.AccessFailedCount,
                    Roles = _context.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                        .ToList()
                })
                .ToListAsync();

            return usersWithRoles.Cast<object>().ToList();
        }

        /// <summary>
        /// Verify database integrity for roles and users
        /// </summary>
        public async Task<bool> VerifyDatabaseIntegrityAsync()
        {
            try
            {
                var rolesCount = await _context.Roles.CountAsync();
                var usersCount = await _context.Users.CountAsync();
                var userRolesCount = await _context.UserRoles.CountAsync();

                _logger.LogInformation($"Database integrity check: {rolesCount} roles, {usersCount} users, {userRolesCount} user-role assignments");

                return rolesCount >= 6 && usersCount >= 1; // At least 6 roles and 1 admin user
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Database integrity check failed");
                return false;
            }
        }
    }
}