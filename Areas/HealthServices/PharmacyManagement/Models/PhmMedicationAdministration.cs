using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Models
{
    /// <summary>
    /// Satu baris = satu dosis MAR — <c>BE-RWI-114</c> s.d. <c>BE-RWI-118</c>, kamus data 0.4 bagian 11.12.
    /// </summary>
    [Table("PhmMedicationAdministration", Schema = "public")]
    public class PhmMedicationAdministration : IdentityModel
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [MaxLength(30)]
        public string AdministrationNumber { get; set; } = string.Empty;

        public Guid PrescriptionId { get; set; }

        public Guid PrescriptionItemId { get; set; }

        public Guid EncounterId { get; set; }

        public Guid InpEpisodeId { get; set; }

        public Guid PatientId { get; set; }

        public Guid DrugId { get; set; }

        public MedicationDoseSource DoseSource { get; set; }

        /// <summary>Wajib untuk dosis <c>Scheduled</c>.</summary>
        public DateTime? ScheduledAt { get; set; }

        public MedicationDoseStatus DoseStatus { get; set; } = MedicationDoseStatus.Due;

        /// <summary>Wajib untuk Held, Refused, Missed, Cancelled. SENSITIF.</summary>
        [MaxLength(500)]
        public string? StatusReason { get; set; }

        public decimal? PlannedDose { get; set; }

        [MaxLength(50)]
        public string? PlannedDoseUnitSnapshot { get; set; }

        public decimal? ActualDose { get; set; }

        [MaxLength(50)]
        public string? ActualDoseUnitSnapshot { get; set; }

        [MaxLength(100)]
        public string? ActualRouteSnapshot { get; set; }

        public DateTime? AdministeredAt { get; set; }

        public Guid? RecordedByEmployeeId { get; set; }

        public Guid? RecordedByUserId { get; set; }

        public DateTime? RecordedAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? DeviationNote { get; set; }

        /// <summary>Salinan penanda high-alert butir resep saat dosis lahir.</summary>
        public bool IsHighAlertSnapshot { get; set; }

        public MedicationDoubleCheckStatus DoubleCheckStatus { get; set; }

        public Guid? DoubleCheckedByEmployeeId { get; set; }

        public Guid? DoubleCheckedByUserId { get; set; }

        public DateTime? DoubleCheckedAt { get; set; }

        /// <summary>Wajib bila ditolak. SENSITIF.</summary>
        [MaxLength(500)]
        public string? DoubleCheckNote { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(500)]
        public string? PrnIndication { get; set; }

        public DateTime? PrnEvaluationDueAt { get; set; }

        /// <summary>SENSITIF.</summary>
        [MaxLength(1000)]
        public string? PrnEvaluationNote { get; set; }

        public DateTime? PrnEvaluatedAt { get; set; }

        public Guid? PrnEvaluatedByUserId { get; set; }

        /// <summary>Naik setiap koreksi dan penolakan cek ganda; penjaga keserentakan.</summary>
        public int RevisionNumber { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }
}
