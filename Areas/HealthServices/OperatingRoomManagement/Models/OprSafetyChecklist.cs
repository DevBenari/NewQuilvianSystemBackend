using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprSafetyChecklist : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public OprChecklistPhase Phase { get; set; }
    public string TemplateVersion { get; set; } = string.Empty;
    public int Revision { get; set; }
    public OprChecklistStatus Status { get; set; } = OprChecklistStatus.Draft;
    public string ItemsJson { get; set; } = "{}";
    public Guid? SignedByUserId { get; set; }
    public DateTime? SignedAt { get; set; }
    public bool IsEmergencyBypass { get; set; }
    public string? BypassReason { get; set; }
    public Guid? BypassResponsibleUserId { get; set; }
    public DateTime? CompletedAfterStableAt { get; set; }
    public OprCase? OprCase { get; set; }
}
