using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs
{
    /// <summary>
    /// Perawat mencatat satu obat yang sedang dipakai pasien — BE-RWI-101, api-contract 0.6.0
    /// bagian 12.8. Tidak ada isian nama obat teks bebas (RWI-DEC-134).
    /// </summary>
    public class CreateReconciliationItemRequest
    {
        [Required]
        public Guid InpEpisodeId { get; set; }

        public Guid DrugId { get; set; }

        [Range(typeof(decimal), "0", "99999999")]
        public decimal? Dose { get; set; }

        public Guid? DoseUnitMeasurementId { get; set; }

        [MaxLength(100)]
        public string? FrequencyText { get; set; }

        public HomeMedicationRoute? Route { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        [MaxLength(100)]
        public string? IdempotencyKey { get; set; }
    }

    /// <summary>
    /// Pembatalan baris obat bawaan yang salah catat, sebelum ada keputusan dokter — BE-RWI-101.
    /// </summary>
    public class CancelReconciliationItemRequest
    {
        [MaxLength(250)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Keputusan dokter atas satu obat bawaan — BE-RWI-101, api-contract 0.6.0 bagian 12.8.
    /// </summary>
    public class CreateReconciliationDecisionRequest
    {
        public ReconciliationDecisionType DecisionType { get; set; }

        [MaxLength(500)]
        public string? DecisionNote { get; set; }

        /// <summary>
        /// Draft resep episode yang sedang dibuka dokter. Kosong berarti draft resep milik dokter
        /// login pada episode ini dipakai, atau dibuat baru pada catatan dokternya yang masih terbuka.
        /// </summary>
        public Guid? TargetDraftPrescriptionId { get; set; }
    }

    public class ReconciliationItemResponse
    {
        public Guid Id { get; set; }
        public Guid EncounterId { get; set; }
        public Guid InpEpisodeId { get; set; }
        public Guid PatientId { get; set; }
        public Guid DrugId { get; set; }
        public string DrugNameSnapshot { get; set; } = string.Empty;
        public bool IsFormularySnapshot { get; set; }
        public decimal? Dose { get; set; }
        public Guid? DoseUnitMeasurementId { get; set; }
        public string? DoseUnitName { get; set; }
        public string? DrugFormSnapshot { get; set; }
        public string? FrequencyText { get; set; }
        public HomeMedicationRoute Route { get; set; }
        public string? Note { get; set; }
        public DateTime RecordedAt { get; set; }
        public Guid RecordedByEmployeeId { get; set; }
        public string? RecordedByEmployeeName { get; set; }
        public Guid RecordedByUserId { get; set; }
        public ReconciliationDecisionType CurrentDecision { get; set; }
        public bool IsActive { get; set; }
        public ReconciliationDecisionResponse? LatestDecision { get; set; }

        /// <summary>
        /// Bantuan tampilan: aksi yang sedang boleh dijalankan menurut keadaan data. Setiap endpoint
        /// tetap memeriksa ulang kelayakan dan kewenangan di backend.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class ReconciliationDecisionResponse
    {
        public Guid Id { get; set; }
        public Guid ReconciliationItemId { get; set; }
        public int SequenceNumber { get; set; }
        public ReconciliationDecisionType DecisionType { get; set; }
        public string? DecisionNote { get; set; }
        public Guid DecidedByDoctorId { get; set; }
        public string? DecidedByDoctorName { get; set; }
        public Guid DecidedByUserId { get; set; }
        public DateTime DecidedAt { get; set; }
        public Guid? ResultPrescriptionId { get; set; }
        public Guid? ResultPrescriptionItemId { get; set; }
        public Guid? SupersedesDecisionId { get; set; }
    }
}
