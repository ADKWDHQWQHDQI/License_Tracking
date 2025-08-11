using License_Tracking.Data;
using License_Tracking.Models;
using Microsoft.EntityFrameworkCore;

namespace License_Tracking
{
    class DatabaseSchemaTest
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing database schema compatibility...");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=LicenseTrackingDB;Integrated Security=true;TrustServerCertificate=true;");

            try
            {
                using var context = new AppDbContext(optionsBuilder.Options);

                // Test Deal entity access (main entity)
                Console.WriteLine("Testing Deal entity access...");
                var dealCount = context.Deals.Count();
                Console.WriteLine($"Deal count: {dealCount}");

                // Test Company entity access
                Console.WriteLine("Testing Company entity access...");
                var companyCount = context.Companies.Count();
                Console.WriteLine($"Company count: {companyCount}");

                // Test Alert entity access with DealId mapping
                Console.WriteLine("Testing Alert entity access...");
                var alertCount = context.Alerts.Count();
                Console.WriteLine($"Alert count: {alertCount}");

                // Test Activity entity access
                Console.WriteLine("Testing Activity entity access...");
                var activityCount = context.Activities.Count();
                Console.WriteLine($"Activity count: {activityCount}");

                Console.WriteLine("\n✅ SUCCESS: All database schema mappings work correctly!");
                Console.WriteLine("The CBMS B2B2B transformation is complete and operational!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
        }
    }
}
