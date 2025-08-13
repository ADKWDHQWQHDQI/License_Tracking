using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    public class AgingSummaryViewModel
    {
        [Display(Name = "Invoice Type")]
        public string InvoiceType { get; set; } = "all";

        [Display(Name = "Total Outstanding Amount")]
        public decimal TotalOutstandingAmount { get; set; }

        [Display(Name = "Current (Not Due)")]
        public AgingBucketViewModel Current { get; set; } = new AgingBucketViewModel();

        [Display(Name = "1-30 Days")]
        public AgingBucketViewModel Days1To30 { get; set; } = new AgingBucketViewModel();

        [Display(Name = "31-60 Days")]
        public AgingBucketViewModel Days31To60 { get; set; } = new AgingBucketViewModel();

        [Display(Name = "61-90 Days")]
        public AgingBucketViewModel Days61To90 { get; set; } = new AgingBucketViewModel();

        [Display(Name = "90+ Days")]
        public AgingBucketViewModel Days90Plus { get; set; } = new AgingBucketViewModel();

        [Display(Name = "Detailed Invoices")]
        public List<AgingInvoiceDetailViewModel> InvoiceDetails { get; set; } = new List<AgingInvoiceDetailViewModel>();
    }

    public class AgingBucketViewModel
    {
        [Display(Name = "Count")]
        public int Count { get; set; }

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Percentage")]
        public decimal Percentage { get; set; }
    }

    public class AgingInvoiceDetailViewModel
    {
        public int Id { get; set; }

        [Display(Name = "Invoice Number")]
        public string InvoiceNumber { get; set; } = string.Empty;

        [Display(Name = "Invoice Type")]
        public string InvoiceType { get; set; } = string.Empty;

        [Display(Name = "Deal Name")]
        public string DealName { get; set; } = string.Empty;

        [Display(Name = "Amount")]
        public decimal Amount { get; set; }

        [Display(Name = "Invoice Date")]
        public DateTime InvoiceDate { get; set; }

        [Display(Name = "Due Date")]
        public DateTime DueDate { get; set; }

        [Display(Name = "Days Overdue")]
        public int DaysOverdue { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Aging Category")]
        public string AgingCategory { get; set; } = string.Empty;

        [Display(Name = "Aging CSS Class")]
        public string AgingCssClass { get; set; } = string.Empty;
    }
}
