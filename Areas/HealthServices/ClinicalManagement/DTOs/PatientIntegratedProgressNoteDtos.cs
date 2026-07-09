using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs
{
    public class PatientIntegratedProgressNoteResponse
    {
        public Guid Id { get; set; }
        public string ProgressNoteNumber { get; set; } = string.Empty;

        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string MedicalRecordNumber { get; set; } = string.Empty;

        public Guid? EncounterId { get; set; }
        public string? EncounterNumber { get; set; }

        public Guid? QueueId { get; set; }
        public string? QueueCode { get; set; }

        public Guid? ConsultationId { get; set; }
        public string? ConsultationNumber { get; set; }

        public Guid? AssessmentId { get; set; }
        public string? AssessmentNumber { get; set; }

        public Guid? VitalSignId { get; set; }
        public string? VitalSignRecordNumber { get; set; }

        public Guid? DoctorId { get; set; }
        public string? DoctorName { get; set; }

        public Guid? ServiceUnitId { get; set; }
        public string? ServiceUnitName { get; set; }

        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }

        public DateTime NoteDateTime { get; set; }
        public string ProfessionType { get; set; } = string.Empty;
        public string? ProfessionName { get; set; }

        public Guid? ProviderUserId { get; set; }
        public string? ProviderUserName { get; set; }
        public string? ProviderDisplayNameSnapshot { get; set; }
        public string? ProviderRoleSnapshot { get; set; }
        public string? ServiceUnitNameSnapshot { get; set; }
        public string? LocationSnapshot { get; set; }

        public string? SourceModule { get; set; }
        public Guid? SourceReferenceId { get; set; }
        public string? SourceReferenceNumber { get; set; }

        public string? NoteText { get; set; }
        public bool IsGeneratedFromSource { get; set; }
        public bool IsReadOnlyGenerated { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreateDateTime { get; set; }
    }

    public class PatientIntegratedProgressNoteDetailResponse : PatientIntegratedProgressNoteResponse
    {
        public string? SubjectiveSummary { get; set; }
        public string? ObjectiveSummary { get; set; }
        public string? AssessmentSummary { get; set; }
        public string? PlanSummary { get; set; }
        public string? Instruction { get; set; }
        public string? Evaluation { get; set; }
        public string? PrivateNote { get; set; }

        public DateTime? CancelledAt { get; set; }
        public Guid? CancelledByUserId { get; set; }
        public string? CancelledByUserName { get; set; }
        public string? CancelReason { get; set; }
    }

    public class PatientIntegratedProgressNoteTimelineResponse
    {
        public Guid Id { get; set; }
        public string ProgressNoteNumber { get; set; } = string.Empty;
        public DateTime NoteDateTime { get; set; }
        public string ProfessionType { get; set; } = string.Empty;
        public string ProfessionName { get; set; } = string.Empty;
        public string ProfessionTone { get; set; } = "neutral";
        public Guid? ProviderUserId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ProviderRole { get; set; } = string.Empty;
        public Guid? ServiceUnitId { get; set; }
        public string ServiceUnitName { get; set; } = string.Empty;
        public Guid? ClinicId { get; set; }
        public string? ClinicName { get; set; }
        public string? SourceModule { get; set; }
        public Guid? SourceReferenceId { get; set; }
        public string? SourceReferenceNumber { get; set; }
        public string NoteText { get; set; } = string.Empty;
        public bool IsGeneratedFromSource { get; set; }
        public bool IsReadOnlyGenerated { get; set; }
    }

    public class PatientIntegratedProgressNoteFilterMetadataResponse
    {
        public string DateFormat { get; set; } = "yyyy-MM-dd";
        public PatientIntegratedProgressNoteDefaultFilterResponse DefaultFilter { get; set; } = new();
        public List<PatientIntegratedProgressNoteSortOptionResponse> SortOptions { get; set; } = new();
        public List<string> SortDirections { get; set; } = new();
        public List<int> PageSizeOptions { get; set; } = new();
        public List<PatientIntegratedProgressNoteProfessionOptionResponse> ProfessionOptions { get; set; } = new();
        public List<PatientIntegratedProgressNoteSourceOptionResponse> SourceModuleOptions { get; set; } = new();
    }

    public class PatientIntegratedProgressNoteDefaultFilterResponse
    {
        public string? Search { get; set; }
        public Guid? PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? VitalSignId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }
        public Guid? ProviderUserId { get; set; }
        public string? ProfessionType { get; set; }
        public string? SourceModule { get; set; }
        public bool? IsGeneratedFromSource { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string SortBy { get; set; } = "noteDateTime";
        public string SortDirection { get; set; } = "desc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 25;
    }

    public class PatientIntegratedProgressNoteSortOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class PatientIntegratedProgressNoteProfessionOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Tone { get; set; } = "neutral";
    }

    public class PatientIntegratedProgressNoteSourceOptionResponse
    {
        public string Value { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
    }

    public class CreatePatientIntegratedProgressNoteRequest
    {
        [Required]
        public Guid PatientId { get; set; }

        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? ConsultationId { get; set; }
        public Guid? AssessmentId { get; set; }
        public Guid? VitalSignId { get; set; }
        public Guid? DoctorId { get; set; }
        public Guid? ServiceUnitId { get; set; }
        public Guid? ClinicId { get; set; }

        public DateTime? NoteDateTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProfessionType { get; set; } = "Doctor";

        [MaxLength(100)]
        public string? ProfessionName { get; set; }

        public Guid? ProviderUserId { get; set; }

        [MaxLength(150)]
        public string? ProviderDisplayNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? ProviderRoleSnapshot { get; set; }

        [MaxLength(150)]
        public string? ServiceUnitNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? LocationSnapshot { get; set; }

        [MaxLength(80)]
        public string? SourceModule { get; set; }

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(80)]
        public string? SourceReferenceNumber { get; set; }

        public string? SubjectiveSummary { get; set; }
        public string? ObjectiveSummary { get; set; }
        public string? AssessmentSummary { get; set; }
        public string? PlanSummary { get; set; }
        public string? Instruction { get; set; }
        public string? Evaluation { get; set; }
        public string? NoteText { get; set; }
        public string? PrivateNote { get; set; }

        public bool IsGeneratedFromSource { get; set; } = false;
        public bool IsReadOnlyGenerated { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class UpdatePatientIntegratedProgressNoteRequest
    {
        public DateTime? NoteDateTime { get; set; }

        [Required]
        [MaxLength(50)]
        public string ProfessionType { get; set; } = "Doctor";

        [MaxLength(100)]
        public string? ProfessionName { get; set; }

        public Guid? ProviderUserId { get; set; }

        [MaxLength(150)]
        public string? ProviderDisplayNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? ProviderRoleSnapshot { get; set; }

        [MaxLength(150)]
        public string? ServiceUnitNameSnapshot { get; set; }

        [MaxLength(150)]
        public string? LocationSnapshot { get; set; }

        [MaxLength(80)]
        public string? SourceModule { get; set; }

        public Guid? SourceReferenceId { get; set; }

        [MaxLength(80)]
        public string? SourceReferenceNumber { get; set; }

        public string? SubjectiveSummary { get; set; }
        public string? ObjectiveSummary { get; set; }
        public string? AssessmentSummary { get; set; }
        public string? PlanSummary { get; set; }
        public string? Instruction { get; set; }
        public string? Evaluation { get; set; }
        public string? NoteText { get; set; }
        public string? PrivateNote { get; set; }

        public bool IsGeneratedFromSource { get; set; } = false;
        public bool IsReadOnlyGenerated { get; set; } = false;
        public bool IsActive { get; set; } = true;
    }

    public class PatientIntegratedProgressNoteCreateResponse
    {
        public Guid Id { get; set; }
        public string ProgressNoteNumber { get; set; } = string.Empty;
        public Guid PatientId { get; set; }
        public Guid? EncounterId { get; set; }
        public Guid? QueueId { get; set; }
        public Guid? ConsultationId { get; set; }
        public DateTime NoteDateTime { get; set; }
        public string ProfessionType { get; set; } = string.Empty;
        public string? ProfessionName { get; set; }
        public string? SourceModule { get; set; }
        public Guid? SourceReferenceId { get; set; }
        public bool IsGeneratedFromSource { get; set; }
        public bool IsReadOnlyGenerated { get; set; }
        public bool IsActive { get; set; }
    }

    public class PatientIntegratedProgressNoteUpdateResponse : PatientIntegratedProgressNoteCreateResponse
    {
    }

    public class CreatePatientIntegratedProgressNoteFromConsultationRequest
    {
        public DateTime? NoteDateTime { get; set; }
        public bool UseCurrentUserAsProvider { get; set; } = true;
        public bool IsReadOnlyGenerated { get; set; } = false;
        public string? AdditionalInstruction { get; set; }
        public string? Evaluation { get; set; }
        public string? PrivateNote { get; set; }
    }

    public class CancelPatientIntegratedProgressNoteRequest
    {
        [Required]
        [MaxLength(250)]
        public string CancelReason { get; set; } = string.Empty;
    }
}
