using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprRecovery : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public OprRecoveryStatus Status { get; set; } = OprRecoveryStatus.Monitoring;
    public string ScoreSystem { get; set; } = string.Empty;
    public decimal? ScoreValue { get; set; }
    public string ObservationJson { get; set; } = "{}";
    public OprRecoveryDecision Decision { get; set; }
    public string? DecisionNote { get; set; }
    public Guid? ReleasedBy { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public int Version { get; set; }
    public OprCase? OprCase { get; set; }
}
