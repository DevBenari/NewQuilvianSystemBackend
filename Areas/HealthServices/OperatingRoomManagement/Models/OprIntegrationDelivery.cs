using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprIntegrationDelivery : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public string Destination { get; set; } = string.Empty;
    public string MessageType { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public string CorrelationId { get; set; } = string.Empty;
    public string PayloadReference { get; set; } = string.Empty;
    public OprDeliveryStatus Status { get; set; } = OprDeliveryStatus.Pending;
    public int RetryCount { get; set; }
    public DateTime? LastAttemptAt { get; set; }
    public string? LastErrorCode { get; set; }
    public string? AcceptedReference { get; set; }
    public OprCase? OprCase { get; set; }
}
