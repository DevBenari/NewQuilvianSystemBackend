using QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.RehabilitationManagement.DTOs
{
    // =====================================================================
    // Rehab Request — contracts/api-contract.md v1, BE-RHB-007.
    // Kolom bertanda S di data-dictionary.md (indikasi, nama fasilitas/perujuk, nomor rujukan
    // eksternal, alasan duplikasi/pengembalian/disposisi) hanya ada di respons rincian, tidak
    // pernah di daftar maupun log.
    // =====================================================================

    public class RehabRequestQuery
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? CustomPeriod { get; set; }
        public RehabRequestStatus? Status { get; set; }
        public RehabRequestSource? SourceType { get; set; }
        public Guid? DisciplineId { get; set; }

        /// <summary>Nomor permintaan, nama pasien, atau nomor rekam medis.</summary>
        public string? Search { get; set; }

        public string? SortBy { get; set; } = "requestedAt";
        public string? SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    /// <summary>
    /// Isian draft. Hanya field ini yang dibaca server; nomor, status, dan pelaku diisi server.
    /// </summary>
    public class CreateRehabRequestRequest
    {
        public Guid PatientId { get; set; }
        public RehabRequestSource SourceType { get; set; }
        public Guid? SourceEncounterId { get; set; }

        /// <summary>Boleh kosong; untuk sumber rawat inap server menurunkannya dari kunjungan.</summary>
        public Guid? InpEpisodeId { get; set; }

        public Guid? InternalReferralLetterId { get; set; }
        public string? ExternalInstitutionName { get; set; }
        public string? ExternalReferralNumber { get; set; }
        public Guid? RequestingDoctorId { get; set; }
        public string? RequestingPractitionerName { get; set; }
        public string? IndicationText { get; set; }

        /// <summary>Kosong berarti waktu server.</summary>
        public DateTime? RequestedAt { get; set; }

        public List<Guid> DisciplineIds { get; set; } = new();
    }

    public class UpdateRehabRequestRequest : CreateRehabRequestRequest
    {
        public int ExpectedVersion { get; set; }
    }

    public class SubmitRehabRequestRequest
    {
        public int ExpectedVersion { get; set; }

        /// <summary>Wajib bila ada permintaan atau episode berjalan yang beririsan (V-REQ-07).</summary>
        public string? DuplicateOverrideReason { get; set; }
    }

    public class RehabReasonRequest
    {
        public int ExpectedVersion { get; set; }
        public string? Reason { get; set; }
    }

    public class AcceptRehabRequestRequest
    {
        public int ExpectedVersion { get; set; }
        public Guid? ResponsibleDoctorId { get; set; }
        public string? Note { get; set; }
    }

    public class RedirectRehabRequestRequest
    {
        public int ExpectedVersion { get; set; }
        public string? Reason { get; set; }
        public string? RedirectTargetText { get; set; }
    }

    public class RehabDuplicateCheckQuery
    {
        public Guid? PatientId { get; set; }
        public List<Guid> DisciplineIds { get; set; } = new();
        public Guid? ExcludeRequestId { get; set; }
    }

    /// <summary>Permintaan atau episode berjalan milik pasien yang sama dengan layanan beririsan.</summary>
    public class RehabDuplicateCandidate
    {
        /// <summary><c>Request</c> atau <c>Episode</c>.</summary>
        public string CandidateType { get; set; } = string.Empty;
        public Guid Id { get; set; }
        public string Number { get; set; } = string.Empty;
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public List<string> DisciplineNames { get; set; } = new();
    }

    public class RehabRequestListItem
    {
        public Guid Id { get; set; }
        public string RequestNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public string? PatientName { get; set; }
        public string? MedicalRecordNumber { get; set; }
        public RehabRequestSource SourceType { get; set; }
        public string SourceTypeName { get; set; } = string.Empty;
        public List<string> DisciplineNames { get; set; } = new();
        public DateTime RequestedAt { get; set; }
        public string? RequestingDoctorName { get; set; }
        public RehabRequestStatus RequestStatus { get; set; }
        public string RequestStatusName { get; set; } = string.Empty;
        public int Version { get; set; }
    }

    public class RehabRequestDisciplineItem
    {
        public Guid DisciplineId { get; set; }
        public string DisciplineCode { get; set; } = string.Empty;
        public string DisciplineName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class RehabRequestDetailResponse : RehabRequestListItem
    {
        public Guid? SourceEncounterId { get; set; }
        public string? SourceEncounterNumber { get; set; }
        public Guid? InpEpisodeId { get; set; }
        public Guid? InternalReferralLetterId { get; set; }
        public string? InternalReferralLetterNumber { get; set; }
        public string? ExternalInstitutionName { get; set; }
        public string? ExternalReferralNumber { get; set; }
        public Guid? RequestingDoctorId { get; set; }
        public string? RequestingPractitionerName { get; set; }
        public string IndicationText { get; set; } = string.Empty;
        public string? DuplicateOverrideReason { get; set; }
        public string? ReturnReason { get; set; }
        public string? DispositionReason { get; set; }
        public string? RedirectTargetText { get; set; }
        public Guid? DispositionByUserId { get; set; }
        public string? DispositionByName { get; set; }
        public DateTime? DispositionAt { get; set; }
        public Guid? SubmittedByUserId { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public string? CancelReason { get; set; }
        public Guid? EpisodeId { get; set; }
        public string? EpisodeNumber { get; set; }
        public DateTime CreateDateTime { get; set; }
        public List<RehabRequestDisciplineItem> Disciplines { get; set; } = new();

        /// <summary>
        /// Aksi yang sah menurut status saat ini (<c>update</c>, <c>submit</c>, <c>return</c>,
        /// <c>request-clarification</c>, <c>accept</c>, <c>redirect</c>, <c>reject</c>,
        /// <c>cancel</c>). Hak akses pengguna tetap diperiksa di setiap endpoint.
        /// </summary>
        public List<string> AvailableActions { get; set; } = new();
    }

    public class RehabRequestSummaryResponse
    {
        public int Total { get; set; }
        public int Draft { get; set; }
        public int Submitted { get; set; }
        public int NeedsClarification { get; set; }
        public int Accepted { get; set; }
        public int Redirected { get; set; }
        public int Rejected { get; set; }
        public int Cancelled { get; set; }
    }

    public class RehabRequestFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public string ResetButtonLabel { get; set; } = "Reset";
        public List<RehabCustomPeriodOptionResponse> CustomPeriods { get; set; } = new();
        public List<RehabOptionResponse> StatusOptions { get; set; } = new();
        public List<RehabOptionResponse> SourceOptions { get; set; } = new();
        public List<RehabOptionResponse> DisciplineOptions { get; set; } = new();
        public List<RehabOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<RehabQueryParameterInfoResponse> QueryParameters { get; set; } = new();
    }

    public class RehabStatusHistoryItem
    {
        public Guid Id { get; set; }
        public string AggregateType { get; set; } = string.Empty;
        public string? FromStatus { get; set; }
        public string ToStatus { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? ReasonText { get; set; }
        public Guid ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public DateTime OccurredAt { get; set; }
    }
}
