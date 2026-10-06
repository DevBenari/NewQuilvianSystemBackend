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

    /// <summary>
    /// Opsional (<c>BE-RWI-174</c>, kontrak <c>0.10.0</c> API 11.5.1). Kosong berarti <c>General</c>,
    /// sehingga layar OK lama yang belum mengirimnya tetap berfungsi.
    /// </summary>
    public OprSurgicalServiceType? SurgicalServiceType { get; set; }

    /// <summary>Opsional. Rencana anestesi saat dipesan.</summary>
    public OprPlannedAnesthesiaType? PlannedAnesthesiaType { get; set; }
}

/// <summary>
/// Permintaan menolak order operasi berstatus Diminta (<c>BE-RWI-174</c>, API 11.5.1
/// <c>PATCH cases/{id}/reject</c>).
/// </summary>
/// <remarks>
/// Kunci idempotensi dibaca dari header <c>Idempotency-Key</c> sesuai kontrak; isian
/// <c>IdempotencyKey</c> pada body hanya cadangan bagi pemanggil yang mengikuti pola perintah OK lain.
/// </remarks>
public class RejectOprCaseRequest
{
    [Required(ErrorMessage = "Alasan penolakan wajib diisi.")]
    [MinLength(10, ErrorMessage = "Alasan penolakan minimal 10 karakter.")]
    [MaxLength(500, ErrorMessage = "Alasan penolakan maksimal 500 karakter.")]
    public string Reason { get; set; } = string.Empty;

    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }

    [MaxLength(100)] public string? IdempotencyKey { get; set; }
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

    // BE-RWI-174 (kontrak 0.10.0 API 11.5.1): isian tambahan, dibaca juga oleh bangsal (FE-INP-26).
    public OprSurgicalServiceType SurgicalServiceType { get; set; } = OprSurgicalServiceType.General;
    public OprPlannedAnesthesiaType? PlannedAnesthesiaType { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? RejectedByName { get; set; }
    public string? RejectionReason { get; set; }

    /// <summary>Alasan tunda, batal, atau tolak terakhir dari histori status.</summary>
    public string? LastStatusReason { get; set; }

    /// <summary>Status versi Catatan Pra-Operasi bangsal terbaru; kosong bila belum ada.</summary>
    public OprWardPreOpStatus? WardPreOpStatus { get; set; }

    /// <summary>Status serah terima pasca operasi revisi terakhir; kosong bila belum ada.</summary>
    public OprHandoverStatus? HandoverStatus { get; set; }
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
