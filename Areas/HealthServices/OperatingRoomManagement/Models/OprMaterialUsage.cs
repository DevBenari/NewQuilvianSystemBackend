using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprMaterialUsage : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OprCaseId { get; set; }
    public Guid ExternalItemId { get; set; }
    public OprMaterialItemType ItemType { get; set; }
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public OprMaterialOutcome Outcome { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime OccurredAt { get; set; }
    public Guid RecordedBy { get; set; }
    public int Revision { get; set; }
    public string? CorrectionReason { get; set; }
    public OprCase? OprCase { get; set; }
}
