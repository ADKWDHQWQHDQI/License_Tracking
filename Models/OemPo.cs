using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace License_Tracking.Models
{
    public class OemPo
    {
        public int OemPoId { get; set; }

        [Column("LicenseId")]
        public int DealId { get; set; }

        [Required(ErrorMessage = "PO Number is required")]
        [StringLength(50)]
        public string PoNumber { get; set; } = string.Empty;

        [StringLength(200)]
        public string ItemDescription { get; set; }

        [Required(ErrorMessage = "PO Amount is required")]
        public decimal PoAmount { get; set; }

        public Deal Deal { get; set; }
    }
}
