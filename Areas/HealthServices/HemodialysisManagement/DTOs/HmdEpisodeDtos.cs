using QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.Enums;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.HemodialysisManagement.DTOs
{
    public class HmdEpisodePagedQuery
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? DpjpDoctorId { get; set; }
        public HmdEpisodeStatus? EpisodeStatus { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class CreateHmdEpisodeRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        [Required]
        public Guid ServiceUnitId { get; set; }

        /// <summary>Dokter penanggung jawab program; wajib (<c>HMD-VAL-010</c>).</summary>
        public Guid? DpjpDoctorId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }
    }

    public class UpdateHmdEpisodeRequest
    {
        [Required]
        public Guid DpjpDoctorId { get; set; }

        [Required]
        public DateOnly StartDate { get; set; }
    }

    /// <summary>
    /// Mengaktifkan, menangguhkan, mengaktifkan kembali, atau menutup episode — mengikuti kontrak
    /// <c>PATCH /{id}/status</c> pada <c>HMD-CONTRACT-v1</c>.
    /// </summary>
    public class ChangeHmdEpisodeStatusRequest
    {
        [Required]
        public HmdEpisodeStatus TargetStatus { get; set; }

        /// <summary>Alasan penangguhan; wajib bila menangguhkan (<c>HMD-VAL-012</c>).</summary>
        [MaxLength(500)]
        public string? SuspendReason { get; set; }

        /// <summary>Sebab penutupan; wajib bila menutup.</summary>
        public HmdEpisodeClosureReason? ClosureReason { get; set; }

        [MaxLength(1000)]
        public string? ClosureNote { get; set; }

        public DateOnly? EndDate { get; set; }
    }

    public class HmdEpisodeResponse
    {
        public Guid Id { get; set; }
        public string EpisodeNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }
        public Guid DpjpDoctorId { get; set; }
        public string? DpjpDoctorName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public HmdEpisodeStatus EpisodeStatus { get; set; }
        public string EpisodeStatusName { get; set; } = string.Empty;
        public DateTime? ActivatedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

    /// <summary>Baris daftar pasien HD. Tidak memuat status serologi maupun diagnosis.</summary>
    public class HmdEpisodeListResponse : HmdEpisodeResponse
    {
        /// <summary>Penanda ringkas saja; jenis isolasinya tidak ditampilkan pada daftar.</summary>
        public bool IsIsolationRequired { get; set; }
        public bool HasActivePrescription { get; set; }
    }

    public class HmdEpisodeDetailResponse : HmdEpisodeListResponse
    {
        public HmdPatientHeaderResponse Patient { get; set; } = new();
        public string? SuspendReason { get; set; }
        public HmdEpisodeClosureReason? ClosureReason { get; set; }
        public string? ClosureReasonName { get; set; }
        public string? ClosureNote { get; set; }
        public Guid? ActivePrescriptionId { get; set; }
        public HmdIsolationRequirement ActiveIsolationRequirement { get; set; }
        public string ActiveIsolationRequirementName { get; set; } = string.Empty;
        public Guid? PrimaryVascularAccessId { get; set; }
        public string? PrimaryVascularAccessName { get; set; }
        public HmdEligibilityOutcome? LatestEligibilityOutcome { get; set; }
        public int SessionCount { get; set; }
        public int UnfinalizedSessionCount { get; set; }
        public List<string> AvailableActions { get; set; } = new();
    }

    public class HmdEpisodeSummaryResponse
    {
        public int TotalEpisode { get; set; }
        public int DraftEpisode { get; set; }
        public int ActiveEpisode { get; set; }
        public int SuspendedEpisode { get; set; }
        public int ClosedEpisode { get; set; }
    }

    public class HmdEpisodeDefaultFilterResponse
    {
        public string? Search { get; set; }
        public HmdEpisodeStatus? EpisodeStatus { get; set; }
        public string SortBy { get; set; } = "startDate";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class HmdEpisodeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public HmdEpisodeDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<HmdOptionItemResponse> EpisodeStatusOptions { get; set; } = new();
        public List<HmdOptionItemResponse> ClosureReasonOptions { get; set; } = new();
        public List<HmdSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
    }

    /// <summary>Sesi yang menahan penutupan episode, disertakan pada rincian penolakan <c>422</c>.</summary>
    public class HmdBlockingSessionItem
    {
        public Guid SessionId { get; set; }
        public string SessionNumber { get; set; } = string.Empty;
        public DateOnly ScheduledDate { get; set; }
        public HmdSessionStatus SessionStatus { get; set; }
        public string SessionStatusName { get; set; } = string.Empty;
    }

    // ------------------------------------------------------------ kelayakan

    public class CreateHmdEligibilityRequest
    {
        [Required]
        public HmdEligibilityOutcome Outcome { get; set; }

        [Required, MaxLength(1000)]
        public string IndicationSummary { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string DecisionReason { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? FollowUpInstruction { get; set; }
    }

    public class HmdEligibilityResponse
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public Guid AssessedByDoctorId { get; set; }
        public string? AssessedByDoctorName { get; set; }
        public DateTime AssessedAt { get; set; }
        public HmdEligibilityOutcome Outcome { get; set; }
        public string OutcomeName { get; set; } = string.Empty;
        public string IndicationSummary { get; set; } = string.Empty;
        public string DecisionReason { get; set; } = string.Empty;
        public string? FollowUpInstruction { get; set; }
    }

    // ------------------------------------------------------------ akses vaskular

    public class CreateHmdVascularAccessRequest
    {
        [Required]
        public HmdVascularAccessType AccessType { get; set; }

        [Required, MaxLength(200)]
        public string AccessSite { get; set; } = string.Empty;

        public HmdVascularAccessStatus AccessStatus { get; set; } = HmdVascularAccessStatus.Usable;

        public bool IsPrimary { get; set; }

        public DateOnly? EstablishedDate { get; set; }

        [MaxLength(1000)]
        public string? ConditionNote { get; set; }
    }

    public class ChangeHmdVascularAccessStatusRequest
    {
        [Required]
        public HmdVascularAccessStatus AccessStatus { get; set; }

        public bool? IsPrimary { get; set; }

        [MaxLength(1000)]
        public string? ConditionNote { get; set; }
    }

    public class HmdVascularAccessResponse
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public HmdVascularAccessType AccessType { get; set; }
        public string AccessTypeName { get; set; } = string.Empty;
        public string AccessSite { get; set; } = string.Empty;
        public HmdVascularAccessStatus AccessStatus { get; set; }
        public string AccessStatusName { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public DateOnly? EstablishedDate { get; set; }
        public DateTime? LastAssessedAt { get; set; }
        public string? ConditionNote { get; set; }
    }

    // ------------------------------------------------------------ serologi

    public class CreateHmdSerologyReviewRequest
    {
        [Required]
        public HmdSerologyTestType TestType { get; set; }

        /// <summary>Rujukan hasil Laboratorium bila diketahui; tanpa itu dicatat manual (<c>HMD-DEP-001</c>).</summary>
        public Guid? LabExaminationId { get; set; }

        [Required]
        public DateOnly ResultDate { get; set; }

        [Required]
        public HmdSerologyResultFlag ResultFlag { get; set; }

        [MaxLength(500)]
        public string? ResultSummary { get; set; }

        /// <summary>Tandai langsung sudah ditinjau oleh pengguna yang mencatat. Bawaan <c>true</c>.</summary>
        public bool IsReviewed { get; set; } = true;

        [MaxLength(1000)]
        public string? ReviewNote { get; set; }
    }

    public class HmdSerologyReviewResponse
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public HmdSerologyTestType TestType { get; set; }
        public string TestTypeName { get; set; } = string.Empty;
        public Guid? LabExaminationId { get; set; }
        public DateOnly ResultDate { get; set; }
        public HmdSerologyResultFlag ResultFlag { get; set; }
        public string ResultFlagName { get; set; } = string.Empty;
        public string? ResultSummary { get; set; }
        public HmdSerologyReviewStatus ReviewStatus { get; set; }
        public string ReviewStatusName { get; set; } = string.Empty;
        public Guid? ReviewedByUserId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ReviewNote { get; set; }
    }

    // ------------------------------------------------------------ isolasi

    public class CreateHmdIsolationDecisionRequest
    {
        [Required]
        public HmdIsolationRequirement Requirement { get; set; }

        [Required]
        public DateOnly EffectiveFrom { get; set; }

        public DateOnly? EffectiveTo { get; set; }

        [Required, MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;

        public Guid? SerologyReviewId { get; set; }
    }

    public class HmdIsolationDecisionResponse
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public HmdIsolationRequirement Requirement { get; set; }
        public string RequirementName { get; set; } = string.Empty;
        public DateOnly EffectiveFrom { get; set; }
        public DateOnly? EffectiveTo { get; set; }
        public bool IsActive { get; set; }
        public Guid DecidedByUserId { get; set; }
        public string? DecidedByName { get; set; }
        public DateTime DecidedAt { get; set; }
        public string Reason { get; set; } = string.Empty;
        public Guid? SerologyReviewId { get; set; }
    }
}
