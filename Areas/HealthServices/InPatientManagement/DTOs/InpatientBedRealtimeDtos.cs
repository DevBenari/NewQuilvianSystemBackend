namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

/// <summary>
/// Sinyal invalidasi denah setelah satu bed berhasil dipesan, ditempati, atau menjadi tujuan
/// transfer. Payload ini bukan sumber status bed; client wajib mengambil ulang floor plan.
/// </summary>
public sealed class InpatientBedRealtimeEventResponse
{
    public Guid EventId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public Guid BedId { get; set; }
    public string BedCode { get; set; } = string.Empty;
    public string BedName { get; set; } = string.Empty;
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public Guid ServiceUnitId { get; set; }
    public string ServiceUnitName { get; set; } = string.Empty;
    public Guid EpisodeId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string ActorDisplayName { get; set; } = string.Empty;
    public DateTime OccurredAtUtc { get; set; }
    public long BedOperationalVersion { get; set; }
}
