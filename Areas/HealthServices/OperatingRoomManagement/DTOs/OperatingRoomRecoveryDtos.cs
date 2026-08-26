using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class SaveOprAnesthesiaRecordRequest
{
    [MaxLength(4000)] public string AssessmentSummary { get; set; } = string.Empty;
    [MaxLength(4000)] public string Technique { get; set; } = string.Empty;
    [MaxLength(8000)] public string MedicationFluidSummary { get; set; } = string.Empty;
    [MaxLength(4000)] public string AirwaySummary { get; set; } = string.Empty;
    [MaxLength(8000)] public string MonitoringSummary { get; set; } = string.Empty;
    [MaxLength(4000)] public string? EventSummary { get; set; }
    [MaxLength(4000)] public string FinalCondition { get; set; } = string.Empty;
    public bool Finalize { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedRecordVersion { get; set; }
}

public class OprAnesthesiaRecordResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public OprRecordStatus Status { get; set; }
    public string AssessmentSummary { get; set; } = string.Empty;
    public string Technique { get; set; } = string.Empty;
    public string MedicationFluidSummary { get; set; } = string.Empty;
    public string AirwaySummary { get; set; } = string.Empty;
    public string MonitoringSummary { get; set; } = string.Empty;
    public string? EventSummary { get; set; }
    public string FinalCondition { get; set; } = string.Empty;
    public Guid? FinalizedBy { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int Version { get; set; }
}

public class OprRecoveryObservationRequest
{
    [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(200)] public string Label { get; set; } = string.Empty;
    [MaxLength(200)] public string? Value { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public class SaveOprRecoveryRequest
{
    [Required, MaxLength(100)] public string ScoreSystem { get; set; } = string.Empty;
    public decimal? ScoreValue { get; set; }
    public List<OprRecoveryObservationRequest> Observations { get; set; } = [];

    /// <summary>`Monitoring`, `ReadyForRelease`, atau `Released`.</summary>
    [Required] public OprRecoveryStatus Status { get; set; }

    /// <summary>Tujuan pasien setelah recovery; wajib saat status `Released`.</summary>
    public OprRecoveryDecision? Decision { get; set; }

    [MaxLength(2000)] public string? DecisionNote { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
    [Range(0, int.MaxValue)] public int ExpectedRecordVersion { get; set; }
}

public class OprRecoveryObservationResponse
{
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Value { get; set; }
    public DateTime? RecordedAt { get; set; }
}

public class OprRecoveryResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public OprRecoveryStatus Status { get; set; }
    public string ScoreSystem { get; set; } = string.Empty;
    public decimal? ScoreValue { get; set; }
    public List<OprRecoveryObservationResponse> Observations { get; set; } = [];
    public OprRecoveryDecision? Decision { get; set; }
    public string? DecisionNote { get; set; }
    public Guid? ReleasedBy { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public int Version { get; set; }
}

public class CreateOprHandoverRequest
{
    [Required] public Guid DestinationUnitId { get; set; }
    [Required, MaxLength(4000)] public string ConditionSummary { get; set; } = string.Empty;
    [MaxLength(4000)] public string? DeviceTherapySummary { get; set; }
    [MaxLength(4000)] public string? RiskSummary { get; set; }
    [MaxLength(4000)] public string? InstructionSummary { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class AcceptOprHandoverRequest
{
    /// <summary>True menerima serah terima; false menolak dan alasan wajib diisi.</summary>
    public bool Accept { get; set; } = true;

    [MaxLength(2000)] public string? RejectionReason { get; set; }
    [Required, MaxLength(100)] public string IdempotencyKey { get; set; } = string.Empty;
}

public class OprHandoverResponse
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public Guid DestinationUnitId { get; set; }
    public string DestinationUnitName { get; set; } = string.Empty;
    public OprHandoverStatus Status { get; set; }
    public string ConditionSummary { get; set; } = string.Empty;
    public string? DeviceTherapySummary { get; set; }
    public string? RiskSummary { get; set; }
    public string? InstructionSummary { get; set; }
    public Guid SentBy { get; set; }
    public DateTime SentAt { get; set; }
    public Guid? ReceivedBy { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public string? RejectionReason { get; set; }
    public int Revision { get; set; }
    public OprCaseStatus CaseStatus { get; set; }
}
