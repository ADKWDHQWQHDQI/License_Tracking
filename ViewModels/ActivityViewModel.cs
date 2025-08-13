using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class DealTimelineViewModel
    {
        public Deal Deal { get; set; } = null!;
        public List<Activity> Activities { get; set; } = new List<Activity>();
    }

    public class WeeklyActivityReportViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<Activity> Activities { get; set; } = new List<Activity>();
        public int TotalActivities { get; set; }
        public int CompletedActivities { get; set; }
        public int PendingActivities { get; set; }
        public int OverdueActivities { get; set; }

        // Chart data properties
        public Dictionary<string, int> ActivitiesByType { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ActivitiesByStatus { get; set; } = new Dictionary<string, int>();

        public decimal CompletionRate => TotalActivities > 0 ? (decimal)CompletedActivities / TotalActivities * 100 : 0;
    }

    public class ActivitySummaryViewModel
    {
        public int TotalThisWeek { get; set; }
        public int CompletedThisWeek { get; set; }
        public int PendingThisWeek { get; set; }
        public int OverdueCount { get; set; }
        public List<Activity> RecentActivities { get; set; } = new List<Activity>();
        public List<Activity> UpcomingActivities { get; set; } = new List<Activity>();
    }
}
