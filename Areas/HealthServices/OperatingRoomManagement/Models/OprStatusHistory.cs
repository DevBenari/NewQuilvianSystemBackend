using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprStatusHistory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public OprCaseStatus? FromStatus { get; set; }
    public OprCaseStatus ToStatus { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public Guid ActorUserId { get; set; }
    public DateTime OccurredAt { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public OprCase? OprCase { get; set; }
}
