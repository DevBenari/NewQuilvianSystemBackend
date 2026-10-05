using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;
namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
public sealed class InpatientDepartureResponse
{
    public Guid EpisodeId { get; set; }
    public InpEpisodeStatus EpisodeStatus { get; set; }
    public DateTime PhysicallyLeftAt { get; set; }
    public string? PhysicallyLeftByUserName { get; set; }
    public InpClearanceObservation ClearanceObserved { get; set; }
    public DateTime ClearanceObservedAt { get; set; }
    public string? ReleasedBedCode { get; set; }
    public int RunningEquipmentUsageClosedCount { get; set; }
    public List<string> FollowUpWarnings { get; set; } = [];
}
public sealed class DepartureBeforeClearanceQuery
{
    public Guid? ServiceUnitId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public bool IncludeClosed { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public sealed class DepartureBeforeClearanceItem
{
    public Guid EpisodeId { get; set; }
    public string EpisodeNumber { get; set; } = "";
    public Guid EncounterId { get; set; }
    public string? PatientName { get; set; }
    public string? MedicalRecordNumber { get; set; }
    public Guid ServiceUnitId { get; set; }
    public DateTime? PhysicallyLeftAt { get; set; }
    public InpClearanceObservation? DepartureClearanceObserved { get; set; }
    public string? CurrentClearanceStatus { get; set; }
    public bool IsReadable { get; set; }
    public InpEpisodeStatus EpisodeStatus { get; set; }
}
