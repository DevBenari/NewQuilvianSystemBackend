using QuilvianSystemBackend.Models;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Models;

public class OprExecutionAddendum : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExecutionRecordId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public Guid AuthoredBy { get; set; }
    public DateTime AuthoredAt { get; set; }
    public OprExecutionRecord? ExecutionRecord { get; set; }
}
