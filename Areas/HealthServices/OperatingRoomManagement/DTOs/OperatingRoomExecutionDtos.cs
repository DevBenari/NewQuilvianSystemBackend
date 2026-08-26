using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class StartOprCaseRequest
{
    /// <summary>Konfirmasi identitas pasien oleh dokter bedah sebelum insisi.</summary>
    public bool ConfirmedPatientIdentity { get; set; }

    /// <summary>Konfirmasi tindakan dan sisi tindakan sesuai permintaan.</summary>
    public bool ConfirmedProcedure { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class CancelOprCaseRequest
{
    [Required, MaxLength(2000)] public string Reason { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedVersion { get; set; }
}

public class SaveOprExecutionRecordRequest
{
    [MaxLength(4000)] public string PreDiagnosis { get; set; } = string.Empty;
    [MaxLength(4000)] public string PostDiagnosis { get; set; } = string.Empty;
    [MaxLength(8000)] public string Findings { get; set; } = string.Empty;
    [MaxLength(8000)] public string Technique { get; set; } = string.Empty;
    [MaxLength(4000)] public string? Complications { get; set; }
    [Range(0, 100000)] public decimal? BloodLossMl { get; set; }
    [MaxLength(2000)] public string? SpecimenNote { get; set; }
    [MaxLength(2000)] public string? ImplantDrainNote { get; set; }
    [MaxLength(4000)] public string PostPlan { get; set; } = string.Empty;

    /// <summary>Bila true catatan difinalisasi dan hanya dapat diperbaiki lewat addendum.</summary>
    public bool Finalize { get; set; }

    /// <summary>Wajib saat finalisasi: `Completed` atau `StoppedEarly`.</summary>
    public OprCaseOutcome? Outcome { get; set; }

    public DateTime? FinishedAt { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedRecordVersion { get; set; }
}

public class CreateOprExecutionAddendumRequest
{
    [Required, MaxLength(8000)] public string Content { get; set; } = string.Empty;
    [Required, MaxLength(2000)] public string Reason { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class OprExecutionAddendumResponse
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public Guid AuthoredBy { get; set; }
    public DateTime AuthoredAt { get; set; }
}

public class OprExecutionRecordResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public OprRecordStatus Status { get; set; }
    public string PreDiagnosis { get; set; } = string.Empty;
    public string PostDiagnosis { get; set; } = string.Empty;
    public string Findings { get; set; } = string.Empty;
    public string Technique { get; set; } = string.Empty;
    public string? Complications { get; set; }
    public decimal? BloodLossMl { get; set; }
    public string? SpecimenNote { get; set; }
    public string? ImplantDrainNote { get; set; }
    public string PostPlan { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public Guid? FinalizedBy { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int Version { get; set; }
    public OprCaseStatus CaseStatus { get; set; }
    public OprCaseOutcome? CaseOutcome { get; set; }
    public List<OprExecutionAddendumResponse> Addenda { get; set; } = [];
}
