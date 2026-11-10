using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;
using QuilvianSystemBackend.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Rehab Episode — contracts/api-contract.md v1, BE-RHB-008. Baca saja.
    // Daftar tidak memuat kolom bertanda S; isi klinis hanya ada di rincian episode dan dokumen.
    // =====================================================================

    public class RehabEpisodeQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public RehabEpisodeStatus? Status { get; set; }
        public Guid? DisciplineId { get; set; }
        public Guid? ResponsibleDoctorId { get; set; }

        /// <summary>Hanya episode yang punya program aktif bertanda perlu ditinjau.</summary>
        public bool? NeedsReview { get; set; }

        /// <summary>Hanya episode yang punya program aktif dengan tanggal evaluasi hari ini atau sudah lewat.</summary>
        public bool? EvaluationDue { get; set; }

        /// <summary>Nomor episode, nomor permintaan, nama pasien, atau nomor rekam medis.</summary>
        public string? Search { get; set; }

        public string? SortBy { get; set; } = "openedAt";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class RehabEpisodeListItem
    {
        public Guid Id { get; set; }
        public string EpisodeNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public Guid RequestId { get; set; }
        public string? RequestNumber { get; set; }
        public Guid ResponsibleDoctorId { get; set; }
        public string? ResponsibleDoctorName { get; set; }

        /// <summary>Layanan yang diminta dan, bila sudah ada asesmen medis, yang diresepkan.</summary>
        public List<string> DisciplineNames { get; set; } = new();

        public RehabEpisodeStatus EpisodeStatus { get; set; }
        public string EpisodeStatusName { get; set; } = string.Empty;
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
        public bool NeedsReview { get; set; }

        /// <summary>Tanggal evaluasi program aktif yang paling dekat.</summary>
        public DateOnly? NextEvaluationDueDate { get; set; }

        public int Version { get; set; }
    }

    public class RehabEpisodeSummaryResponse
    {
        public int Total { get; set; }
        public int AwaitingMedicalAssessment { get; set; }
        public int Planning { get; set; }
        public int Active { get; set; }
        public int Closed { get; set; }
        public int NeedsReview { get; set; }
        public int EvaluationDue { get; set; }
    }

    public class RehabEpisodeFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public List<RehabCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<RehabOptionResponse> StatusOptions { get; set; } = new();
        public List<RehabOptionResponse> DisciplineOptions { get; set; } = new();
        public List<RehabOptionResponse> ResponsibleDoctorOptions { get; set; } = new();
        public List<RehabOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RehabQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class RehabEpisodePatientInfo
    {
        public Guid Id { get; set; }
        public string? FullName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public DateTime? BirthDate { get; set; }
        public Gender? Gender { get; set; }
    }

    public class RehabEpisodeRequestInfo
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public RehabRequestSource SourceType { get; set; }
        public string SourceTypeName { get; set; } = string.Empty;
        public Guid? SourceEncounterId { get; set; }
        public string? SourceEncounterNumber { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? RequestingDoctorName { get; set; }
        public string? RequestingPractitionerName { get; set; }
        public string IndicationText { get; set; } = string.Empty;
    }

    public class RehabEpisodeDisciplineInfo
    {
        public Guid DisciplineId { get; set; }
        public string DisciplineCode { get; set; } = string.Empty;
        public string DisciplineName { get; set; } = string.Empty;
        public bool IsRequested { get; set; }
        public bool IsPrescribed { get; set; }
        public string? MedicalPrecautionText { get; set; }
        public string? ActiveProgramStatusName { get; set; }
        public bool ActiveProgramNeedsReview { get; set; }
        public DateOnly? ActiveProgramEvaluationDueDate { get; set; }
    }

    /// <summary>Jumlah dokumen per jenis. Jenis yang belum punya slice bernilai nol, bukan galat.</summary>
    public class RehabEpisodeDocumentSummary
    {
        public int MedicalAssessmentDraft { get; set; }
        public int MedicalAssessmentFinal { get; set; }
        public int DisciplineAssessmentDraft { get; set; }
        public int DisciplineAssessmentFinal { get; set; }
        public int ActiveGoal { get; set; }
        public int ProgramDraft { get; set; }
        public int ProgramActive { get; set; }
        public int EducationRecord { get; set; }
        public int SessionInProgress { get; set; }
        public int SessionFinal { get; set; }
        public int ProgressReviewFinal { get; set; }
        public RehabRecordStatus? DischargeSummaryStatus { get; set; }
    }

    /// <summary>
    /// Satu aksi yang boleh ditawarkan layar: sah menurut status episode <b>dan</b> pengguna
    /// memegang butir hak aksesnya. Kewenangan yang melekat pada data (profesi, penulis) tetap
    /// diperiksa endpoint aksinya.
    /// </summary>
    public class RehabEpisodeAction
    {
        public string Action { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
    }

    public class RehabEpisodeDetailResponse : RehabEpisodeListItem
    {
        public RehabEpisodeClosureReason? ClosureReason { get; set; }
        public Guid? ClosedByUserId { get; set; }
        public RehabEpisodePatientInfo Patient { get; set; } = new();
        public RehabEpisodeRequestInfo Request { get; set; } = new();
        public List<RehabEpisodeDisciplineInfo> Disciplines { get; set; } = new();
        public RehabEpisodeDocumentSummary Documents { get; set; } = new();
        public List<RehabEpisodeAction> AvailableActions { get; set; } = new();
    }

    /// <summary>Satu kejadian di timeline episode. Tidak memuat isi klinis; isi dibuka dari dokumennya.</summary>
    public class RehabTimelineItem
    {
        /// <summary>
        /// <c>MedicalAssessment</c>, <c>DisciplineAssessment</c>, <c>Program</c>, <c>Education</c>,
        /// <c>Session</c>, <c>ProgressReview</c>, atau <c>DischargeSummary</c>.
        /// </summary>
        public string ItemType { get; set; } = string.Empty;
        public string ItemTypeName { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string? Reference { get; set; }
        public Guid? DisciplineId { get; set; }
        public string? DisciplineName { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public Guid? AuthorUserId { get; set; }
        public string? AuthorName { get; set; }
    }
}
