using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class Renewal
    {
        public int RenewalId { get; set; }

        [Column("LicenseId")]
        public int DealId { get; set; }

        [Required]
        public DateTime RenewalDate { get; set; }

        public decimal RenewalAmount { get; set; }

        [StringLength(200)]
        public string Remarks { get; set; }

        public Deal Deal { get; set; }
    }
}
