using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
public class StartTransfusionMonitoringRequest { public Guid EpisodeId { get; set; } public Guid BloodUnitId { get; set; } public DateTime ReceivedAtWardAt { get; set; } public DateTime TransfusionStartedAt { get; set; } }
public class PutTransfusionPointRequest
{
    public DateTime MeasuredAt { get; set; }
    [Range(1, 400)] public int? SystolicBp { get; set; }
    [Range(1, 300)] public int? DiastolicBp { get; set; }
    [Range(20, 50)] public decimal? TemperatureCelsius { get; set; }
    [Range(1, 400)] public int? PulseRate { get; set; }
    [MaxLength(500)] public string? LateNote { get; set; }
    [MaxLength(500)] public string? CorrectionReason { get; set; }
    public int? ExpectedRevision { get; set; }
}
public class RecordTransfusionReactionRequest { public DateTime OccurredAt { get; set; } public CliTransfusionPointType? PointType { get; set; } [Required, MaxLength(250)] public string ReactionSummary { get; set; } = string.Empty; [MaxLength(1000)] public string? ReactionDetail { get; set; } }
public class StopTransfusionRequest { public DateTime StoppedAt { get; set; } [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty; }
public class CompleteTransfusionRequest { public DateTime CompletedAt { get; set; } }
public class SelectableBloodUnit { public Guid BloodUnitId { get; set; } public string PmiBagNumber { get; set; } = string.Empty; public string ComponentName { get; set; } = string.Empty; public DateTime? IssuedAt { get; set; } }
public class TransfusionMonitoringResponse
{
    public Guid Id { get; set; }
    public Guid EpisodeId { get; set; }
    public Guid BloodUnitId { get; set; }
    public string PmiBagNumber { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public DateTime ReceivedAtWardAt { get; set; }
    public DateTime TransfusionStartedAt { get; set; }
    public CliTransfusionMonitoringStatus Status { get; set; }
    public DateTime? StoppedAt { get; set; }
    public string? StopReason { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int Version { get; set; }
    public int LateToleranceMinutes { get; set; }
    public List<TransfusionPointResponse> Points { get; set; } = new();
    public List<TransfusionReactionResponse> Reactions { get; set; } = new();
}
public class TransfusionPointResponse
{
    public Guid Id { get; set; }
    public CliTransfusionPointType PointType { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? MeasuredAt { get; set; }
    public int? SystolicBp { get; set; }
    public int? DiastolicBp { get; set; }
    public decimal? TemperatureCelsius { get; set; }
    public int? PulseRate { get; set; }
    public bool IsLate { get; set; }
    public bool IsStopped { get; set; }
    public string? LateNote { get; set; }
    public int RevisionNumber { get; set; }
}
public class TransfusionReactionResponse
{
    public Guid Id { get; set; }
    public DateTime OccurredAt { get; set; }
    public CliTransfusionPointType? PointType { get; set; }
    public string ReactionSummary { get; set; } = string.Empty;
    public string? ReactionDetail { get; set; }
    public CliReactionNoticeDelivery NoticeDelivery { get; set; }
    public int NoticeAttemptCount { get; set; }
    public Guid? BloodBankNoticeId { get; set; }
}
