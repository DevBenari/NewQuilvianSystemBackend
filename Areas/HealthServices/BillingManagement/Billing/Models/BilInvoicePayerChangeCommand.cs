using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models
{
    /// <summary>
    /// Jejak perintah perubahan payer kunjungan (MPY-DES-003).
    /// Append-only: mencatat setiap perubahan payer kunjungan yang dilakukan dari kasir
    /// beserta snapshot sebelum dan sesudahnya, versi kalkulasi, dan kunci idempotensi.
    /// Baris pada tabel ini tidak boleh diubah atau dihapus dalam keadaan apa pun.
    /// </summary>
    [Table("BilInvoicePayerChangeCommand", Schema = "public")]
    public class BilInvoicePayerChangeCommand : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InvoiceId { get; set; }

        [Required]
        public Guid EncounterId { get; set; }

        [Required]
        [MaxLength(30)]
        public string PreviousPayerKind { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string NewPayerKind { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? PreviousPayerNameSnapshot { get; set; }

        [MaxLength(250)]
        public string? NewPayerNameSnapshot { get; set; }

        public Guid? PreviousCalculationVersionId { get; set; }

        [Required]
        public Guid NewCalculationVersionId { get; set; }

        public int ResetAssignmentCount { get; set; } = 0;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string IdempotencyKey { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? CorrelationId { get; set; }

        [MaxLength(100)]
        public string? CausationId { get; set; }

        // Navigations
        public BilInvoice? Invoice { get; set; }
        public BilCalculationVersion? PreviousCalculationVersion { get; set; }
        public BilCalculationVersion? NewCalculationVersion { get; set; }
    }
}
