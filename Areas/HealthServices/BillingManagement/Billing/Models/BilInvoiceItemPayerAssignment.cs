using QuilvianSystemBackend.Areas.HealthServices.RegistrationManagement.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Models
{
    /// <summary>
    /// Penanggung per baris biaya (CAP-37, MPY-DES-008).
    /// Menyimpan siapa yang menanggung satu baris biaya: CASH, INSURANCE, atau COMPANY_GUARANTOR.
    /// Tepat satu baris aktif per InvoiceItemId (dijaga filtered unique index).
    /// Append-only saat perubahan: baris lama dinonaktifkan, baris baru disisipkan.
    /// </summary>
    [Table("BilInvoiceItemPayerAssignment", Schema = "public")]
    public class BilInvoiceItemPayerAssignment : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid InvoiceItemId { get; set; }

        public Guid? EncounterGuarantorId { get; set; }

        [Required]
        [MaxLength(30)]
        public string PayerKind { get; set; } = "CASH";

        [Required]
        [MaxLength(20)]
        public string AssignmentSource { get; set; } = "AUTO";

        [MaxLength(500)]
        public string? Reason { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigations
        public BilInvoiceItem? InvoiceItem { get; set; }
        public RegPatientEncounterGuarantor? EncounterGuarantor { get; set; }
    }
}
