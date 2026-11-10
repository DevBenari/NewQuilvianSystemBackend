using QuilvianSystemBackend.Areas.HealthServices.MedicalRecordManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Tujuan — api-contract v1 grup Rehab Goal (BE-RHB-011).
    // =====================================================================

    public class RehabGoalQuery
    {
        public Guid? DisciplineId { get; set; }

        /// <summary>Sertakan versi lama (<c>Superseded</c>). Bawaan: tidak.</summary>
        public bool? IncludeSuperseded { get; set; }
    }

    /// <summary>Isian tujuan. Dipakai <c>POST /by-episode/{episodeId}</c> dan <c>PUT /{id}</c>.</summary>
    public class SaveRehabGoalRequest
    {
        /// <summary>Wajib untuk <c>PUT</c>; diabaikan saat membuat.</summary>
        public int ExpectedVersion { get; set; }

        /// <summary>Layanan yang dituju. Kosong = tujuan episode secara umum. Tidak dapat diganti.</summary>
        public Guid? DisciplineId { get; set; }

        public RehabGoalTerm? Term { get; set; }
        public string? DescriptionText { get; set; }
        public string? BaselineText { get; set; }
        public string? TargetText { get; set; }
        public DateOnly? TargetDate { get; set; }
        public bool AgreedWithPatient { get; set; }
    }

    /// <summary>Body <c>POST /{id}/revise</c>: isian versi baru beserta alasan.</summary>
    public class ReviseRehabGoalRequest : SaveRehabGoalRequest
    {
        public string? Reason { get; set; }
    }

    public class RehabGoalItem
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public Guid? DisciplineId { get; set; }
        public string? DisciplineName { get; set; }
        public RehabGoalTerm Term { get; set; }
        public string TermName { get; set; } = string.Empty;
        public string DescriptionText { get; set; } = string.Empty;
        public string BaselineText { get; set; } = string.Empty;
        public string TargetText { get; set; } = string.Empty;
        public DateOnly TargetDate { get; set; }
        public RehabGoalStatus GoalStatus { get; set; }
        public string GoalStatusName { get; set; } = string.Empty;
        public Guid? SupersedesGoalId { get; set; }
        public bool AgreedWithPatient { get; set; }
        public Guid AuthorUserId { get; set; }
        public string? AuthorName { get; set; }

        /// <summary><c>true</c> bila dirujuk program aktif: ubah ditolak, gunakan revisi.</summary>
        public bool UsedByActiveProgram { get; set; }

        public DateTime CreateDateTime { get; set; }
        public int Version { get; set; }

        /// <summary><c>update</c> (penulis, belum dipakai program aktif), <c>revise</c>, <c>discontinue</c> selama aktif.</summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    // =====================================================================
    // Program — api-contract v1 grup Rehab Program (BE-RHB-011).
    // =====================================================================

    public class RehabProgramInterventionRequest
    {
        public Guid ProcedureId { get; set; }
        public string? ParametersText { get; set; }
    }

    /// <summary>Isian program. Dipakai <c>POST /by-episode/{episodeId}</c> dan <c>PUT /{id}</c>.</summary>
    public class SaveRehabProgramRequest
    {
        /// <summary>Wajib untuk <c>PUT</c>; diabaikan saat membuat.</summary>
        public int ExpectedVersion { get; set; }

        /// <summary>Wajib saat membuat; tidak dapat diganti.</summary>
        public Guid? DisciplineId { get; set; }

        /// <summary>Kunjungan tempat program disusun. Wajib saat membuat; tidak dapat diganti.</summary>
        public Guid? AuthoredEncounterId { get; set; }

        public string? FrequencyText { get; set; }
        public int? PlannedSessionCount { get; set; }
        public string? PrecautionText { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EvaluationDueDate { get; set; }

        /// <summary>Daftar ini menggantikan intervensi draft yang tersimpan.</summary>
        public List<RehabProgramInterventionRequest> Interventions { get; set; } = new();

        /// <summary>Daftar ini menggantikan tautan tujuan draft yang tersimpan.</summary>
        public List<Guid> GoalIds { get; set; } = new();
    }

    /// <summary>Body <c>POST /{id}/revise</c>.</summary>
    public class ReviseRehabProgramRequest
    {
        public int ExpectedVersion { get; set; }
        public string? ModificationReason { get; set; }

        /// <summary>Kunjungan penyusunan versi baru. Kosong = sama dengan versi aktif.</summary>
        public Guid? AuthoredEncounterId { get; set; }
    }

    public class RehabProgramListItem
    {
        public Guid Id { get; set; }
        public Guid EpisodeId { get; set; }
        public Guid DisciplineId { get; set; }
        public string DisciplineName { get; set; } = string.Empty;
        public int ProgramVersion { get; set; }
        public Guid? SupersedesProgramId { get; set; }
        public RehabProgramStatus ProgramStatus { get; set; }
        public string ProgramStatusName { get; set; } = string.Empty;
        public bool NeedsReview { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EvaluationDueDate { get; set; }
        public DateTime? ActivatedAt { get; set; }
        public Guid AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
        public int Version { get; set; }
    }

    public class RehabProgramInterventionItem
    {
        /// <summary>Id baris intervensi rencana; dirujuk sesi sebagai <c>programInterventionId</c>.</summary>
        public Guid ProgramInterventionId { get; set; }

        public Guid ProcedureId { get; set; }
        public string? ProcedureCode { get; set; }
        public string? ProcedureName { get; set; }
        public string ParametersText { get; set; } = string.Empty;
    }

    public class RehabProgramGoalItem
    {
        public Guid GoalId { get; set; }
        public RehabGoalTerm Term { get; set; }
        public string DescriptionText { get; set; } = string.Empty;
        public RehabGoalStatus GoalStatus { get; set; }
        public string GoalStatusName { get; set; } = string.Empty;
    }

    /// <summary>Satu prasyarat aktivasi yang belum terpenuhi (V-PRG-01..06).</summary>
    public class RehabActivationIssue
    {
        public string Code { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }

    public class RehabProgramDetailResponse : RehabProgramListItem
    {
        public Guid AuthoredEncounterId { get; set; }
        public string? AuthoredEncounterNumber { get; set; }
        public string? FrequencyText { get; set; }
        public int? PlannedSessionCount { get; set; }
        public string? PrecautionText { get; set; }
        public string? ModificationReason { get; set; }
        public DateTime? StoppedAt { get; set; }
        public string? StopReason { get; set; }
        public List<RehabProgramInterventionItem> Interventions { get; set; } = new();
        public List<RehabProgramGoalItem> Goals { get; set; } = new();

        /// <summary>Persetujuan pada edukasi final terakhir untuk program ini, bila ada.</summary>
        public RehabConsentStatus? LatestConsentStatus { get; set; }
        public string? LatestConsentStatusName { get; set; }

        public ClinicalDocumentIntegrityStatus? IntegrityStatus { get; set; }
        public string? IntegrityStatusName { get; set; }

        /// <summary>Jenis dokumen RM (<c>RehabProgram</c> = 48) untuk addendum sesudah aktif.</summary>
        public ClinicalDocumentKind DocumentKind { get; set; } = ClinicalDocumentKind.RehabProgram;

        /// <summary>
        /// Draft: <c>update</c>, <c>activate</c>, <c>cancel</c> untuk penulis. Aktif: <c>revise</c>, <c>stop</c>,
        /// <c>addendum</c>. Hak akses tetap diperiksa layar dan server.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();
    }
}
