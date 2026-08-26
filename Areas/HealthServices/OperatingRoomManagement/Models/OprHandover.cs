using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprHandover : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public Guid DestinationUnitId { get; set; }
    public OprHandoverStatus Status { get; set; } = OprHandoverStatus.Draft;
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
    public OprCase? OprCase { get; set; }
}
