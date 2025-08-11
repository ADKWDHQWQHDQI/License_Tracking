namespace License_Tracking.ViewModels
{
    public class MarketTrendAnalysisViewModel
    {
        public string AnalysisPeriod { get; set; } = string.Empty;
        public decimal OverallGrowthRate { get; set; }
        public string TrendDirection { get; set; } = "Stable"; // Growing, Declining, Stable
        public decimal MarketVolatility { get; set; }
        public List<MarketSegmentTrendViewModel> SegmentTrends { get; set; } = new();
        public List<SeasonalityPatternViewModel> SeasonalPatterns { get; set; } = new();
        public string MarketOutlook { get; set; } = string.Empty;
        public int ConfidenceLevel { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class MarketSegmentTrendViewModel
    {
        public string SegmentName { get; set; } = string.Empty;
        public decimal GrowthRate { get; set; }
        public decimal MarketShare { get; set; }
        public string TrendAnalysis { get; set; } = string.Empty;
        public List<string> KeyFactors { get; set; } = new();
    }

    public class SeasonalityPatternViewModel
    {
        public string Period { get; set; } = string.Empty;
        public decimal SeasonalMultiplier { get; set; }
        public string Pattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
