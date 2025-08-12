using License_Tracking.Models;

namespace License_Tracking.ViewModels
{
    public class BAAnalyticsViewModel
    {
        public string CurrentUser { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string CurrentPeriod { get; set; } = string.Empty;
        public List<BAPerformanceSummaryViewModel> BAPerformanceSummary { get; set; } = new List<BAPerformanceSummaryViewModel>();
        public List<BATarget> CurrentTargets { get; set; } = new List<BATarget>();
        public List<object> RecentAchievements { get; set; } = new List<object>();
        public List<object> TeamLeaderboard { get; set; } = new List<object>();
    }

    public class BAPerformanceSummaryViewModel
    {
        public string BAName { get; set; } = string.Empty;
        public string TargetType { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; }
        public decimal AchievementPercentage { get; set; }
        public string Period { get; set; } = string.Empty;
        public string PeriodType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class BADetailedPerformanceViewModel
    {
        public string BAName { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public string CurrentUser { get; set; } = string.Empty;
        public List<object> MonthlyPerformance { get; set; } = new List<object>();
        public List<BATarget> CurrentTargets { get; set; } = new List<BATarget>();
        public int CurrentYear { get; set; }
    }

    public class BATargetViewModel
    {
        public int TargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string AssignedTo { get; set; } = string.Empty;
        public decimal TargetValue { get; set; }
        public decimal ActualValue { get; set; }
        public string TargetPeriod { get; set; } = string.Empty;
        public string PeriodType { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal AchievementPercentage { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class TeamPerformanceViewModel
    {
        public List<object> TeamMembers { get; set; } = new List<object>();
        public int CurrentYear { get; set; }
        public string Period { get; set; } = string.Empty;
    }

    public class BADashboardStatsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalMargin { get; set; }
        public int TotalDeals { get; set; }
        public int ActiveTargets { get; set; }
        public decimal OverallAchievementRate { get; set; }
        public int TeamSize { get; set; }
    }
}
