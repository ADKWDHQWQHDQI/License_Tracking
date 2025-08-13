using System.ComponentModel.DataAnnotations;

namespace License_Tracking.ViewModels
{
    public class BillingDashboardViewModel
    {
        [Display(Name = "Total Customer Invoices")]
        public int TotalCustomerInvoices { get; set; }

        [Display(Name = "Total OEM Invoices")]
        public int TotalOemInvoices { get; set; }

        [Display(Name = "Customer Pending Amount")]
        public decimal PendingCustomerAmount { get; set; }

        [Display(Name = "OEM Pending Amount")]
        public decimal PendingOemAmount { get; set; }

        [Display(Name = "Overdue Customer Invoices")]
        public int OverdueCustomerInvoices { get; set; }

        [Display(Name = "Overdue OEM Invoices")]
        public int OverdueOemInvoices { get; set; }

        [Display(Name = "Total Revenue")]
        public decimal TotalRevenue { get; set; }

        [Display(Name = "Customer Revenue")]
        public decimal CustomerRevenue { get; set; }

        [Display(Name = "OEM Revenue")]
        public decimal OemRevenue { get; set; }

        [Display(Name = "Customer Paid Amount")]
        public decimal CustomerPaidAmount { get; set; }

        [Display(Name = "OEM Paid Amount")]
        public decimal OemPaidAmount { get; set; }

        [Display(Name = "Recent Invoices")]
        public List<RecentInvoiceViewModel> RecentInvoices { get; set; } = new List<RecentInvoiceViewModel>();

        [Display(Name = "Overdue Invoices")]
        public List<OverdueInvoiceViewModel> OverdueInvoices { get; set; } = new List<OverdueInvoiceViewModel>();
    }

    public class RecentInvoiceViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string DealName { get; set; } = string.Empty;
    }

    public class OverdueInvoiceViewModel
    {
        public int Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public string InvoiceType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysOverdue { get; set; }
        public string DealName { get; set; } = string.Empty;
    }
}
