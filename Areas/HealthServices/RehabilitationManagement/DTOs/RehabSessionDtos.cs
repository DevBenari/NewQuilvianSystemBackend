using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Sesi terapi — api-contract v1 grup Rehab Session (BE-RHB-012). Handoff billing (BE-RHB-013)
    // belum termasuk: BillingHandoffStatus tetap NotRequired.
    // =====================================================================

    /// <summary>Saringan worklist <c>GET /</c> dan <c>GET /summary</c>. Tanggal sesi = mulai, atau dicatat bila tidak terlaksana.</summary>
    public class RehabSessionQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public RehabSessionStatus? Status { get; set; }
        public RehabRecordStatus? RecordStatus { get; set; }
        public Guid? DisciplineId { get; set; }
        public Guid? TherapistUserId { get; set; }

        /// <summary>Hanya sesi yang dilaksanakan pengguna yang sedang masuk.</summary>
        public bool? Mine { get; set; }

        public RehabBillingHandoffStatus? BillingHandoffStatus { get; set; }
        public string? Search { get; set; }
        public string? SortBy { get; set; } = "sessionAt";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class RehabSessionByEpisodeQuery
    {
        public Guid? ProgramId { get; set; }
    }

    /// <summary>Body <c>POST /by-program/{programId}/start</c> — cek keselamatan sebelum memulai (V-SES-01..05).</summary>
    public class StartRehabSessionRequest
    {
        public Guid? EncounterId { get; set; }

        /// <summary>Opsional; bila diisi harus perawatan rawat inap dari kunjungan itu.</summary>
        public Guid? InpEpisodeId { get; set; }

        public bool IdentityVerified { get; set; }
        public bool ProgramVerified { get; set; }
        public string? CurrentConditionText { get; set; }
        public bool ContraindicationFound { get; set; }
        public string? ContraindicationText { get; set; }
    }

    /// <summary>Body <c>POST /by-program/{programId}/not-done</c>.</summary>
    public class RecordNotDoneRehabSessionRequest
    {
        /// <summary>Kosong = kunjungan penyusunan program (dokumen RM tetap butuh kunjungan).</summary>
        public Guid? EncounterId { get; set; }

        public RehabNotDoneReason? NotDoneReason { get; set; }
        public string? Note { get; set; }
    }

    public class RehabSessionInterventionRequest
    {
        /// <summary>Intervensi rencana yang dilaksanakan. Kosong = di luar rencana (wajib <c>DeviationType = Modified</c>).</summary>
        public Guid? ProgramInterventionId { get; set; }

        public Guid ProcedureId { get; set; }
        public string? ActualParametersText { get; set; }
        public int? DurationMinutes { get; set; }
        public int? Quantity { get; set; }
    }

    /// <summary>Body <c>PUT /{id}</c> — isi sesi yang sedang berjalan.</summary>
    public class UpdateRehabSessionRequest
    {
        public int ExpectedVersion { get; set; }

        /// <summary>Daftar ini menggantikan intervensi aktual yang tersimpan.</summary>
        public List<RehabSessionInterventionRequest> Interventions { get; set; } = new();

        public string? ResponseText { get; set; }
        public string? ImmediateOutcomeText { get; set; }

        /// <summary><c>None</c>, <c>Modified</c>, atau <c>Escalated</c>. Henti dini lewat <c>POST /{id}/stop-early</c>.</summary>
        public RehabDeviationType DeviationType { get; set; } = RehabDeviationType.None;

        public string? DeviationReasonText { get; set; }
        public string? ImmediateActionText { get; set; }
        public string? ContactedPartyText { get; set; }

        /// <summary>Deviasi memengaruhi sesi berikutnya: program ditandai perlu telaah.</summary>
        public bool NeedsProgramReview { get; set; }
    }

    /// <summary>Body <c>POST /{id}/complete</c>.</summary>
    public class RehabVersionRequest
    {
        public int ExpectedVersion { get; set; }
    }

    /// <summary>Body <c>POST /{id}/stop-early</c> — henti dini atau eskalasi (V-SES-07).</summary>
    public class StopEarlyRehabSessionRequest
    {
        public int ExpectedVersion { get; set; }

        /// <summary><c>StoppedEarly</c> (bawaan) atau <c>Escalated</c>.</summary>
        public RehabDeviationType? DeviationType { get; set; }

        public string? DeviationReasonText { get; set; }
        public string? ImmediateActionText { get; set; }
        public string? ContactedPartyText { get; set; }

        /// <summary>Bawaan <c>true</c>: henti dini menandai program perlu telaah (UAT-12).</summary>
        public bool? NeedsProgramReview { get; set; }
    }

    public class RehabSessionListItem
    {
        public Guid Id { get; set; }
        public string SessionNumber { get; set; } = string.Empty;
        public Guid EpisodeId { get; set; }
        public string EpisodeNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid ProgramId { get; set; }
        public int ProgramVersion { get; set; }
        public Guid DisciplineId { get; set; }
        public string DisciplineName { get; set; } = string.Empty;
        public Guid EncounterId { get; set; }
        public string? EncounterNumber { get; set; }
        public Guid TherapistUserId { get; set; }
        public string? TherapistName { get; set; }

        /// <summary>Waktu mulai, atau waktu dicatat bila tidak terlaksana.</summary>
        public DateTime SessionAt { get; set; }

        public DateTime? EndedAt { get; set; }
        public RehabSessionStatus SessionStatus { get; set; }
        public string SessionStatusName { get; set; } = string.Empty;
        public RehabRecordStatus RecordStatus { get; set; }
        public string RecordStatusName { get; set; } = string.Empty;
        public RehabDeviationType DeviationType { get; set; }
        public RehabBillingHandoffStatus BillingHandoffStatus { get; set; }
        public string BillingHandoffStatusName { get; set; } = string.Empty;
        public int InterventionCount { get; set; }
        public int Version { get; set; }
    }

    public class RehabSessionInterventionItem
    {
        public Guid Id { get; set; }
        public Guid? ProgramInterventionId { get; set; }
        public bool IsPlanned { get; set; }
        public Guid ProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public string? ProcedureName { get; set; }
        public string ActualParametersText { get; set; } = string.Empty;
        public int? DurationMinutes { get; set; }
        public int Quantity { get; set; }
        public Guid? PatientProcedureId { get; set; }
    }

    public class RehabSessionDetailResponse : RehabSessionListItem
    {
        public Guid? InpEpisodeId { get; set; }
        public RehabNotDoneReason? NotDoneReason { get; set; }
        public string? NotDoneReasonName { get; set; }
        public bool IdentityVerified { get; set; }
        public bool ProgramVerified { get; set; }
        public string? CurrentConditionText { get; set; }
        public bool ContraindicationFound { get; set; }
        public string? ContraindicationText { get; set; }
        public DateTime? StartedAt { get; set; }
        public string? ResponseText { get; set; }
        public string? ImmediateOutcomeText { get; set; }
        public string DeviationTypeName { get; set; } = string.Empty;
        public string? DeviationReasonText { get; set; }
        public string? ImmediateActionText { get; set; }
        public string? ContactedPartyText { get; set; }
        public DateTime? SignedAt { get; set; }
        public DateTime? BillingHandoffAt { get; set; }
        public string? BillingHandoffError { get; set; }
        public bool ProgramNeedsReview { get; set; }

        public List<RehabSessionInterventionItem> Interventions { get; set; } = new();

        /// <summary>Intervensi rencana program untuk diisi layar sebagai titik awal (bukan salinan otomatis).</summary>
        public List<RehabProgramInterventionItem> PlannedInterventions { get; set; } = new();

        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }
        public string? IntegrityStatusName { get; set; }

        /// <summary>Jenis dokumen RM (<c>RehabSession</c> = 49) untuk addendum sesudah final.</summary>
        public ClinicalDocumentKind DocumentKind { get; set; } = ClinicalDocumentKind.RehabSession;

        /// <summary>Menurut status dan pelaksana: <c>update</c>, <c>complete</c>, <c>stop-early</c>, <c>sign</c>, <c>cancel</c>, <c>addendum</c>.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class RehabSessionSummaryResponse
    {
        public int Total { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int StoppedEarly { get; set; }
        public int NotDone { get; set; }

        /// <summary>Sesi selesai/henti dini/tidak terlaksana yang belum ditandatangani.</summary>
        public int AwaitingSignature { get; set; }
    }

    public class RehabSessionFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public List<RehabCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<RehabOptionResponse> StatusOptions { get; set; } = new();
        public List<RehabOptionResponse> RecordStatusOptions { get; set; } = new();
        public List<RehabOptionResponse> DisciplineOptions { get; set; } = new();
        public List<RehabOptionResponse> TherapistOptions { get; set; } = new();
        public List<RehabOptionResponse> BillingHandoffStatusOptions { get; set; } = new();
        public List<RehabOptionResponse> NotDoneReasonOptions { get; set; } = new();
        public List<RehabOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RehabQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }
}
