using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprAnesthesiaRecord : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public OprRecordStatus Status { get; set; } = OprRecordStatus.Draft;
    public string AssessmentSummary { get; set; } = string.Empty;
    public string Technique { get; set; } = string.Empty;
    public string MedicationFluidSummary { get; set; } = string.Empty;
    public string AirwaySummary { get; set; } = string.Empty;
    public string MonitoringSummary { get; set; } = string.Empty;
    public string? EventSummary { get; set; }
    public string FinalCondition { get; set; } = string.Empty;
    public Guid? FinalizedBy { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int Version { get; set; }
    public OprCase? OprCase { get; set; }
}
