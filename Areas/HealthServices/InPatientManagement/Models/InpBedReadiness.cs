using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Models;

public class InpBedReadiness : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BedId { get; set; }
    public Guid CycleId { get; set; } = Guid.NewGuid();
    public InpBedReadinessState State { get; set; } = InpBedReadinessState.Unverified;
    public long Version { get; set; } = 1;
    public Guid? LastReleasedPlacementId { get; set; }
    public DateTime? VerifiedAtUtc { get; set; }
    public Guid? VerifiedByUserId { get; set; }
    public string? VerificationReference { get; set; }
}
