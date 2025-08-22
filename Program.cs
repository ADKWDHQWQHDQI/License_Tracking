using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using License_Tracking.Data;
using License_Tracking.Services;
using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Hangfire;
using Hangfire.SqlServer;
using Hangfire.Dashboard;
using OfficeOpenXml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Identity with Enhanced Role Support
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Password settings for production security
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Optimize password hashing for better performance
builder.Services.Configure<PasswordHasherOptions>(options =>
{
    options.IterationCount = 10000; // Default is 10000, this is optimized for development
});

// Configure Authentication with Enhanced Security
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.Cookie.SameSite = SameSiteMode.Lax;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
        };
    });

// Configure Authorization Policies for CBMS Roles
builder.Services.AddAuthorization(options =>
{
    // Admin: Full system access
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    // Sales: Customer and Deal management
    options.AddPolicy("SalesAccess", policy =>
        policy.RequireRole("Admin", "Sales", "Management"));

    // Finance: Billing and Payment access
    options.AddPolicy("FinanceAccess", policy =>
        policy.RequireRole("Admin", "Finance", "Management"));

    // Operations: OEM and Product management
    options.AddPolicy("OperationsAccess", policy =>
        policy.RequireRole("Admin", "Operations", "Management"));

    // Management: Analytics and Reports
    options.AddPolicy("ManagementAccess", policy =>
        policy.RequireRole("Admin", "Management"));

    // BA (Business Analyst): Pipeline and Analytics
    options.AddPolicy("BAAccess", policy =>
        policy.RequireRole("Admin", "BA", "Management"));

    // Deal Management: Combined access for deal operations
    options.AddPolicy("DealManagement", policy =>
        policy.RequireRole("Admin", "Sales", "Operations", "Management"));
});

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
builder.Services.AddHangfireServer();

// Register Services
builder.Services.AddMemoryCache(); // Add memory cache
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IProjectPipelineService, ProjectPipelineService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<RoleSeederService>(); // Add the seeding service

// Register Background Services
builder.Services.AddHostedService<NotificationBackgroundService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Configure Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireCustomAuthorizationFilter() }
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Initialize database and seed default users
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var seederService = scope.ServiceProvider.GetRequiredService<RoleSeederService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        // Ensure database is created
        context.Database.EnsureCreated();

        // Seed roles and users with proper password hashes
        seederService.SeedRolesAndAdminAsync().Wait();
        seederService.SeedSampleUsersAsync().Wait();

        logger.LogInformation("CBMS B2B2B CRM application started successfully - default users seeded with proper password hashes");
        logger.LogInformation("Admin login: admin@cbms.com / Admin@123");
        logger.LogInformation("Sample users: sales@cbms.com / Sales@123, finance@cbms.com / Finance@123, etc.");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during application startup or user seeding");
    }
}

app.Run();

// Custom authorization filter for Hangfire Dashboard
public class HangfireCustomAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true && httpContext.User.IsInRole("Admin");
    }
}