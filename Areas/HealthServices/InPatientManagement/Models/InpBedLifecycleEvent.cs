using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

public class InpBedLifecycleEvent : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReadinessId { get; set; }
    public Guid CycleId { get; set; }
    public Guid? OperationReceiptId { get; set; }
    public Guid? PlacementId { get; set; }
    public Guid? ReservationId { get; set; }
    public Guid? CleaningAttemptId { get; set; }
    public string Action { get; set; } = string.Empty;
    public InpBedReadinessState BeforeState { get; set; }
    public InpBedReadinessState AfterState { get; set; }
    public Guid ActorUserId { get; set; }
    public DateTime OccurredAtUtc { get; set; }
    public DateTime RecordedAtUtc { get; set; }
    public string? Reason { get; set; }
    public string? EvidenceReference { get; set; }
}
