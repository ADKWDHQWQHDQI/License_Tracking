using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class CustomerPo
    {
        public int CustomerPoId { get; set; }

        [Column("LicenseId")]
        public int DealId { get; set; }

        [Required]
        [StringLength(50)]
        public string PoNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string ItemDescription { get; set; }

        [Required]
        public decimal PoAmount { get; set; }

        public Deal Deal { get; set; }
    }
}
