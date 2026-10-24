using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

/// <summary>
/// Respons timeline jadwal per ruang operasi (BE-OPR-012, OPS-DEC-038, OPS-DEC-039).
/// </summary>
public class OprRoomTimelineResponse
{
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string? Floor { get; set; }
    public List<OprTimelineSlotResponse> Slots { get; set; } = [];
}

/// <summary>
/// Slot waktu individual dalam timeline: Occupied, SterilizationBuffer, atau Available.
/// </summary>
public class OprTimelineSlotResponse
{
    /// <summary>
    /// Jenis slot: "Occupied", "SterilizationBuffer", atau "Available"
    /// </summary>
    public string SlotType { get; set; } = "Available";

    public Guid? CaseId { get; set; }
    public string? BookingNumber { get; set; }
    public string? PatientName { get; set; }
    public string? ProcedureName { get; set; }

    public DateTimeOffset StartAt { get; set; }
    public DateTimeOffset EndAt { get; set; }

    public string? LeadSurgeonName { get; set; }
    public string? AnesthesiologistName { get; set; }

    /// <summary>
    /// Durasi dalam menit, khususnya untuk SterilizationBuffer.
    /// </summary>
    public int? DurationMinutes { get; set; }

    /// <summary>
    /// Catatan slot, misal "Pembersihan & Dekontaminasi Ruangan".
    /// </summary>
    public string? Note { get; set; }
}
