using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class OprCasePagedQuery
{
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 20;
    [MaxLength(100)] public string? Search { get; set; }
    public OprCaseStatus? Status { get; set; }
    public Guid? PatientId { get; set; }
    public Guid? EncounterId { get; set; }
    public DateTime? RequestedFrom { get; set; }
    public DateTime? RequestedTo { get; set; }
}

public class OprCaseProcedureRequest
{
    [Required] public Guid PatientProcedureId { get; set; }
    public bool IsPrimary { get; set; }
}

public class CreateOprCaseRequest
{
    [Required] public Guid PatientId { get; set; }
    [Required] public Guid EncounterId { get; set; }
    [Required] public Guid RequesterDoctorId { get; set; }
    [Required] public Guid PrimarySurgeonId { get; set; }
    [Required] public OprCaseType CaseType { get; set; }
    [Required] public OprPriority Priority { get; set; }
    [Required, MaxLength(4000)] public string Indication { get; set; } = string.Empty;
    [MaxLength(30)] public string? Laterality { get; set; }
    [Range(1, 1440)] public int EstimatedMinutes { get; set; }
    public DateTime? PreferredAt { get; set; }
    [Required, MinLength(1)] public List<OprCaseProcedureRequest> Procedures { get; set; } = [];
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, 0)] public int ExpectedVersion { get; set; }
}

public class UpdateOprCaseRequest
{
    [Required] public Guid RequesterDoctorId { get; set; }
    [Required] public Guid PrimarySurgeonId { get; set; }
    [Required] public OprCaseType CaseType { get; set; }
    [Required] public OprPriority Priority { get; set; }
    [Required, MaxLength(4000)] public string Indication { get; set; } = string.Empty;
    [MaxLength(30)] public string? Laterality { get; set; }
    [Range(1, 1440)] public int EstimatedMinutes { get; set; }
    public DateTime? PreferredAt { get; set; }
    [Required, MinLength(1)] public List<OprCaseProcedureRequest> Procedures { get; set; } = [];
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class OprCaseProcedureResponse
{
    public Guid PatientProcedureId { get; set; }
    public string ProcedureCode { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public int Sequence { get; set; }
}

public class OprCaseSummaryResponse
{
    public Guid Id { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid EncounterId { get; set; }
    public OprCaseType CaseType { get; set; }
    public OprPriority Priority { get; set; }
    public OprCaseStatus Status { get; set; }
    public string PrimaryProcedureName { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public int Version { get; set; }
}

public class OprCaseDetailResponse : OprCaseSummaryResponse
{
    public Guid RequesterDoctorId { get; set; }
    public string RequesterDoctorName { get; set; } = string.Empty;
    public Guid PrimarySurgeonId { get; set; }
    public string PrimarySurgeonName { get; set; } = string.Empty;
    public OprCaseOutcome? Outcome { get; set; }
    public string Indication { get; set; } = string.Empty;
    public string? Laterality { get; set; }
    public int EstimatedMinutes { get; set; }
    public DateTime? PreferredAt { get; set; }
    public List<OprCaseProcedureResponse> Procedures { get; set; } = [];
    public List<string> AvailableActions { get; set; } = [];
}
