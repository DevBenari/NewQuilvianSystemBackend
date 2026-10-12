using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

public class InpBedCleaningAttempt : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReadinessId { get; set; }
    public Guid CycleId { get; set; }
    public InpBedCleaningAttemptState State { get; set; }
    public DateTime StartedAtUtc { get; set; }
    public Guid StartedByUserId { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? RejectionReason { get; set; }
    public string SopReference { get; set; } = string.Empty;
}
