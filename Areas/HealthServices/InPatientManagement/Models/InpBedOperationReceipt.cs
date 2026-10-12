using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

public class InpBedOperationReceipt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActorUserId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public string RequestHash { get; set; } = string.Empty;
    public string ResultKind { get; set; } = string.Empty;
    public Guid ResultEntityId { get; set; }
    public long? ResultVersion { get; set; }
    public DateTime CommittedAtUtc { get; set; }
}
