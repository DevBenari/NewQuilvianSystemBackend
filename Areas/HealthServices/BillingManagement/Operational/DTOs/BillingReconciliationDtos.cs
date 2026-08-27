using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Operational.DTOs
{
    public class BillingReconciliationCaseResponse
    {
        public Guid Id { get; set; }
        public string CaseNumber { get; set; } = string.Empty;
        public BillingReconciliationCaseType CaseType { get; set; }
        public string CaseTypeName { get; set; } = string.Empty;

        public string SourceContext { get; set; } = string.Empty;
        public Guid MilestoneFactId { get; set; }
        public int MilestoneFactVersion { get; set; }
        public string EffectType { get; set; } = string.Empty;
        public Guid? ProcessingEffectId { get; set; }

        public Guid EncounterId { get; set; }
        public Guid? FolioId { get; set; }
        public Guid? ChargeLineId { get; set; }

        public decimal ImpactAmount { get; set; }
        public string ImpactDescription { get; set; } = string.Empty;
        public bool BlocksFolioClosure { get; set; }

        public BillingReconciliationCaseStatus CaseStatus { get; set; }
        public string CaseStatusName { get; set; } = string.Empty;
        public BillingReconciliationPriority Priority { get; set; }
        public string PriorityName { get; set; } = string.Empty;

        public Guid? OwnerUserId { get; set; }
        public DateTime? AssignedAt { get; set; }

        public DateTime DetectedAt { get; set; }
        public DateTime? SlaDueAt { get; set; }
        public DateTime? SlaBreachedAt { get; set; }

        /// <summary>
        /// Umur case dalam menit, dihitung saat dibaca. Bukan kolom, karena umur berubah setiap
        /// detik dan menyimpannya hanya akan membuatnya cepat usang.
        /// </summary>
        public int AgeMinutes { get; set; }

        public bool SlaBreached { get; set; }

        public int AttemptCount { get; set; }
        public DateTime? LastAttemptAt { get; set; }
        public string? NextAction { get; set; }
        public string? FailureReason { get; set; }

        public BillingReconciliationResolutionType? ResolutionType { get; set; }
        public string? ResolutionTypeName { get; set; }
        public string? ResolutionNote { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? ResolvedByUserId { get; set; }

        public Guid? CorrelationId { get; set; }
        public int Version { get; set; }
    }

    public class AssignReconciliationCaseRequest
    {
        [Required]
        public Guid OwnerUserId { get; set; }

        [MaxLength(500)]
        public string? NextAction { get; set; }

        public BillingReconciliationPriority? Priority { get; set; }
    }

    public class ResolveReconciliationCaseRequest
    {
        [Required]
        public BillingReconciliationResolutionType ResolutionType { get; set; }

        [Required]
        [MaxLength(1000)]
        public string ResolutionNote { get; set; } = string.Empty;
    }

    /// <summary>
    /// Jawaban gerbang penutupan folio. Sengaja mengembalikan daftar alasan, bukan sekadar
    /// boleh atau tidak: petugas yang ditolak berhak tahu persis apa yang menahannya.
    /// </summary>
    public class FolioClosureReadinessResponse
    {
        public Guid FolioId { get; set; }
        public Guid EncounterId { get; set; }
        public bool CanClose { get; set; }
        public List<FolioClosureBlocker> Blockers { get; set; } = new();
    }

    public class FolioClosureBlocker
    {
        public string BlockerCode { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ReconciliationCaseId { get; set; }
        public string? CaseNumber { get; set; }
        public decimal ImpactAmount { get; set; }
    }

    /// <summary>
    /// Laporan pemulihan sesuai <c>RJ-BIL-GATE-DEC-008</c>: jumlah per outcome, encounter dan
    /// folio yang terdampak, dampak yang belum selesai, pemilik, dan tindakan berikutnya.
    /// </summary>
    public class BillingRecoveryReportResponse
    {
        public DateTime GeneratedAt { get; set; }
        public Guid? EncounterId { get; set; }

        public List<RecoveryOutcomeCount> OutcomeCounts { get; set; } = new();
        public List<RecoveryCaseTypeCount> CaseTypeCounts { get; set; } = new();

        public int UnresolvedCaseCount { get; set; }
        public int UnassignedCaseCount { get; set; }
        public int SlaBreachedCaseCount { get; set; }
        public decimal UnresolvedImpactAmount { get; set; }

        public List<Guid> AffectedEncounterIds { get; set; } = new();
        public List<Guid> AffectedFolioIds { get; set; } = new();

        public List<BillingReconciliationCaseResponse> UnresolvedCases { get; set; } = new();
    }

    public class RecoveryOutcomeCount
    {
        public BillingProcessingOutcome Outcome { get; set; }
        public string OutcomeName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class RecoveryCaseTypeCount
    {
        public BillingReconciliationCaseType CaseType { get; set; }
        public string CaseTypeName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal ImpactAmount { get; set; }
    }

    /// <summary>
    /// Pencarian status pemrosesan kanonik berdasarkan identitas sumber yang stabil, sesuai
    /// <c>RJ-BIL-GATE-DEC-008</c>. Inilah yang dipakai modul klinis untuk memastikan hasil
    /// sebuah pengiriman yang jawabannya hilang, alih-alih mengirim ulang secara buta.
    /// </summary>
    public class BillingProcessingStatusResponse
    {
        public string SourceContext { get; set; } = string.Empty;
        public Guid MilestoneFactId { get; set; }
        public int MilestoneFactVersion { get; set; }
        public string EffectType { get; set; } = string.Empty;

        public bool Found { get; set; }
        public BillingProcessingOutcome? Outcome { get; set; }
        public string? OutcomeName { get; set; }

        public Guid? FolioId { get; set; }
        public Guid? ChargeLineId { get; set; }
        public BillingChargeCalculationStatus? CalculationStatus { get; set; }
        public int? AppliedFactVersion { get; set; }

        public string? ErrorCode { get; set; }
        public string? ErrorMessage { get; set; }

        public Guid? ReconciliationCaseId { get; set; }
        public string? ReconciliationCaseNumber { get; set; }
        public BillingReconciliationCaseStatus? ReconciliationCaseStatus { get; set; }

        public DateTime? OccurredAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// Apakah pengirim boleh mencoba ulang dengan kunci yang sama. Bernilai <c>false</c>
        /// ketika hasilnya belum pasti, karena percobaan ulang atas hasil yang belum diverifikasi
        /// adalah cara paling umum melahirkan tagihan ganda.
        /// </summary>
        public bool SafeToRetryWithSameKey { get; set; }

        public string Guidance { get; set; } = string.Empty;
    }

    public class ReconciliationScanResponse
    {
        public DateTime ScannedAt { get; set; }
        public int EffectsExamined { get; set; }
        public int CasesOpened { get; set; }
        public int CasesReused { get; set; }
        public int CasesAutoResolved { get; set; }
        public int SlaBreachesMarked { get; set; }
        public List<BillingReconciliationCaseResponse> OpenedCases { get; set; } = new();
    }
}
