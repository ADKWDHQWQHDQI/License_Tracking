using System.Collections.Generic;
using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class DashboardViewModel
    {
        public string UserEmail { get; set; } = string.Empty;
        public List<string> UserRoles { get; set; } = new List<string>();

        // CBMS Key Metrics
        public int TotalActiveDeals { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalCompanies { get; set; } // Added for new dashboard
        public int TotalDeals { get; set; } // Added for new dashboard
        public int TotalOEMs { get; set; }
        public decimal TotalPipelineValue { get; set; }
        public decimal MonthlyRevenue { get; set; }

        // Legacy License Tracking (for backward compatibility)
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiringLicenses { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalMargin { get; set; }
        public int PendingPayments { get; set; }
        public int PendingInvoices { get; set; } // Added for new dashboard
        public int ActiveCustomers { get; set; } // Added for new dashboard
        public int TotalUsers { get; set; }

        // Deal Stage Distribution
        public Dictionary<string, int> DealsByStage { get; set; } = new Dictionary<string, int>();

        // Recent Activities
        public List<Activity> RecentActivities { get; set; } = new List<Activity>();

        // Recent Deals
        public List<Deal> RecentDeals { get; set; } = new List<Deal>();

        // Pipeline Deals
        public List<Deal> PipelineDeals { get; set; } = new List<Deal>();

        // Upcoming Activities
        public List<Activity> UpcomingActivities { get; set; } = new List<Activity>();

        // Invoice Status
        public Dictionary<string, int> InvoicesByStatus { get; set; } = new Dictionary<string, int>();

        // Revenue by Month (Last 6 months)
        public Dictionary<string, decimal> MonthlyRevenueTrend { get; set; } = new Dictionary<string, decimal>();

        // Project Pipeline Properties (legacy)
        public int TotalPipelineProjects { get; set; }
        public int ActivePipelineProjects { get; set; }
        public decimal ProjectedPipelineRevenue { get; set; }
        public decimal ProjectedPipelineMargin { get; set; }
    }

    public class UserManagementViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new List<string>();
    }

    public class OperationsViewModel
    {
        public int TotalLicenses { get; set; }
        public int ActiveLicenses { get; set; }
        public int ExpiredLicenses { get; set; }
        public int ExpiringIn30Days { get; set; }
        public int ExpiringIn60Days { get; set; }
        public int ExpiringIn90Days { get; set; }
    }
}