using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Reasesmen — api-contract v1 grup Rehab Progress Review (BE-RHB-014).
    // =====================================================================

    public class RehabGoalReviewRequest
    {
        public Guid GoalId { get; set; }

        /// <summary><c>Active</c> (masih berjalan), <c>Achieved</c>, <c>PartiallyAchieved</c>, atau <c>NotAchieved</c>.</summary>
        public RehabGoalStatus AchievementStatus { get; set; }

        public string? NoteText { get; set; }
    }

    /// <summary>Isian reasesmen. Dipakai <c>POST /by-episode/{episodeId}</c> dan <c>PUT /{id}</c>.</summary>
    /// <remarks>
    /// <c>DisciplineId</c> kosong = reasesmen medis (Sp.KFR). Bila diisi, <c>ProgramId</c> wajib dan harus program
    /// aktif layanan itu. Layanan, program, dan kunjungan tidak dapat diganti sesudah draft dibuat.
    /// </remarks>
    public class SaveRehabProgressReviewRequest
    {
        /// <summary>Wajib untuk <c>PUT</c>; diabaikan saat membuat.</summary>
        public int ExpectedVersion { get; set; }

        public Guid? DisciplineId { get; set; }
        public Guid? ProgramId { get; set; }
        public Guid? EncounterId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ProgressSummaryText { get; set; }
        public string? BarrierText { get; set; }

        /// <summary>Daftar ini menggantikan pengukuran draft yang tersimpan.</summary>
        public List<RehabMeasurementRequest> Measurements { get; set; } = new();

        /// <summary>Daftar ini menggantikan penilaian tujuan draft yang tersimpan.</summary>
        public List<RehabGoalReviewRequest> GoalReviews { get; set; } = new();

        public RehabProgressDecision? Decision { get; set; }
        public string? DecisionReasonText { get; set; }

        /// <summary>Wajib bila keputusan <c>Refer</c>.</summary>
        public string? ReferralTargetText { get; set; }

        /// <summary>Wajib bila instrumen atau satuan pembanding berbeda dari baseline (V-MEA-02).</summary>
        public string? InstrumentChangeNote { get; set; }
    }

    public class RehabProgressReviewItem
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public Guid? DisciplineId { get; set; }
        public string? DisciplineName { get; set; }

        /// <summary><c>true</c> = reasesmen medis (tanpa layanan).</summary>
        public bool IsMedicalReview { get; set; }

        public Guid? ProgramId { get; set; }
        public int? ProgramVersion { get; set; }
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public DateTime ReviewedAt { get; set; }
        public RehabProgressDecision? Decision { get; set; }
        public string? DecisionName { get; set; }
        public Guid AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public RehabRecordStatus RecordStatus { get; set; }
        public string RecordStatusName { get; set; } = string.Empty;
        public DateTime? SignedAt { get; set; }
        public int Version { get; set; }
    }

    public class RehabGoalReviewItem
    {
        public Guid GoalId { get; set; }
        public RehabGoalTerm Term { get; set; }
        public string DescriptionText { get; set; } = string.Empty;
        public string TargetText { get; set; } = string.Empty;
        public RehabGoalStatus AchievementStatus { get; set; }
        public string AchievementStatusName { get; set; } = string.Empty;
        public string? NoteText { get; set; }
    }

    /// <summary>Satu baris pembanding: nilai reasesmen terhadap baseline asesmen awal yang final.</summary>
    public class RehabMeasurementComparison
    {
        public Guid MeasurementId { get; set; }
        public string InstrumentName { get; set; } = string.Empty;
        public string? BodyPartOrDomain { get; set; }
        public ClinicalInstrumentKind? InstrumentKind { get; set; }
        public decimal? CurrentValue { get; set; }
        public string? CurrentValueText { get; set; }
        public string? CurrentUnit { get; set; }

        /// <summary><c>false</c> bila asesmen awal tidak punya pengukuran yang sepadan.</summary>
        public bool HasBaseline { get; set; }

        public Guid? BaselineMeasurementId { get; set; }
        public decimal? BaselineValue { get; set; }
        public string? BaselineValueText { get; set; }
        public string? BaselineUnit { get; set; }
        public DateTime? BaselineMeasuredAt { get; set; }

        /// <summary><c>true</c> bila versi instrumen (berinstrumen) dan satuan sama dengan baseline.</summary>
        public bool IsComparable { get; set; }

        /// <summary>Selisih nilai (sekarang − baseline); hanya diisi bila <see cref="IsComparable"/>.</summary>
        public decimal? Difference { get; set; }
    }

    public class RehabProgressReviewDetailResponse : RehabProgressReviewItem
    {
        public string EpisodeNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string? ProgressSummaryText { get; set; }
        public string? BarrierText { get; set; }
        public string? DecisionReasonText { get; set; }
        public string? ReferralTargetText { get; set; }
        public string? InstrumentChangeNote { get; set; }
        public RehabProgramStatus? ProgramStatus { get; set; }
        public string? ProgramStatusName { get; set; }

        /// <summary>Asesmen awal final yang dipakai sebagai baseline (medis bila reasesmen medis, profesi bila layanan).</summary>
        public Guid? BaselineAssessmentId { get; set; }

        public List<RehabMeasurementItem> Measurements { get; set; } = new();
        public List<RehabMeasurementComparison> Comparisons { get; set; } = new();
        public List<RehabGoalReviewItem> GoalReviews { get; set; } = new();

        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }
        public string? IntegrityStatusName { get; set; }

        /// <summary>Jenis dokumen RM (<c>RehabProgressReview</c> = 50) untuk addendum sesudah final.</summary>
        public ClinicalDocumentKind DocumentKind { get; set; } = ClinicalDocumentKind.RehabProgressReview;

        /// <summary><c>update</c>/<c>sign</c> untuk penulis draft; <c>addendum</c> sesudah final.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }
}
