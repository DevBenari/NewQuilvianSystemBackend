using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprExecutionRecord : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public OprRecordStatus Status { get; set; } = OprRecordStatus.Draft;
    public string PreDiagnosis { get; set; } = string.Empty;
    public string PostDiagnosis { get; set; } = string.Empty;
    public string Findings { get; set; } = string.Empty;
    public string Technique { get; set; } = string.Empty;
    public string? Complications { get; set; }
    public decimal? BloodLossMl { get; set; }
    public string? SpecimenNote { get; set; }
    public string? ImplantDrainNote { get; set; }
    public string PostPlan { get; set; } = string.Empty;
    public DateTime StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public Guid? FinalizedBy { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int Version { get; set; }
    public OprCase? OprCase { get; set; }
    public ICollection<OprExecutionAddendum> Addenda { get; set; } = [];
}
