using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.HumanResource.BenefitManagement.Models
{
    /// <summary>
    /// Buku mutasi plafon benefit layanan RS (BE-FIN-104; kamus data 9.2; FIN-INTEGRATION-1.9 R.3).
    /// Hanya menambah: baris tidak pernah diubah atau dihapus. Satu baris USE per serah terima PAYER,
    /// paling banyak satu baris RETURN yang menunjuk USE itu.
    /// </summary>
    [Table("HrdBenefitBalanceMovement", Schema = "public")]
    public class HrdBenefitBalanceMovement : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Enrollment pemilik saldo; untuk tanggungan, enrollment induknya.</summary>
        public Guid EmployeeBenefitEnrollmentId { get; set; }

        /// <summary>Terisi bila orangnya tanggungan (BenefitPersonKind = DEPENDENT).</summary>
        public Guid? EmployeeBenefitDependentId { get; set; }

        [Required]
        [MaxLength(20)]
        public string BenefitPersonKind { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string MovementType { get; set; } = string.Empty;

        /// <summary>HandoffKey baris serah terima PAYER; kunci idempotensi.</summary>
        public Guid SourceHandoffKey { get; set; }

        /// <summary>Invoice Billing asal; rujukan logis tanpa FK lintas area.</summary>
        public Guid InvoiceId { get; set; }

        public decimal Amount { get; set; }

        public decimal BalanceBefore { get; set; }

        public decimal BalanceAfter { get; set; }

        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>Wajib pada RETURN; menunjuk mutasi USE yang dikembalikan.</summary>
        public Guid? ReversesMovementId { get; set; }

        [MaxLength(500)]
        public string? ReasonText { get; set; }

        public Guid? ActorUserId { get; set; }

        public TrxEmployeeBenefitEnrollment? EmployeeBenefitEnrollment { get; set; }
        public TrxEmployeeBenefitDependent? EmployeeBenefitDependent { get; set; }
        public HrdBenefitBalanceMovement? ReversesMovement { get; set; }
    }
}
