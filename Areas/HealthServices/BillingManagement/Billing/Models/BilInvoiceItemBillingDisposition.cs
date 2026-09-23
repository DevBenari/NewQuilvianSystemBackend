using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models
{
    /// <summary>
    /// Disposisi penebusan obat / penagihan per baris biaya (CAP-37, MPY-DES-010).
    /// Menyimpan keputusan apakah satu baris obat masuk tagihan (INCLUDED) atau tidak ditebus (EXCLUDED).
    /// Tepat satu baris aktif per InvoiceItemId (dijaga filtered unique index).
    /// </summary>
    [Table("BilInvoiceItemBillingDisposition", Schema = "public")]
    public class BilInvoiceItemBillingDisposition : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InvoiceItemId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Disposition { get; set; } = "INCLUDED";

        [Required]
        [MaxLength(20)]
        public string DecisionSource { get; set; } = "AUTO";

        [MaxLength(500)]
        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigations
        public BilInvoiceItem? InvoiceItem { get; set; }
    }
}
