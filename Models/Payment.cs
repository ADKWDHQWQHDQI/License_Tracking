using System;
using System.ComponentModel.DataAnnotations;

namespace License_Tracking.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }

        public int InvoiceId { get; set; }

        [Required(ErrorMessage = "Payment Date is required")]
        public DateTime PaymentDate { get; set; }

        [Required(ErrorMessage = "Payment Amount is required")]
        public decimal Amount { get; set; }

        [StringLength(50)]
        public string PaymentMethod { get; set; }

        [StringLength(200)]
        public string Remarks { get; set; }

        public CbmsInvoice CbmsInvoice { get; set; }
    }
}
