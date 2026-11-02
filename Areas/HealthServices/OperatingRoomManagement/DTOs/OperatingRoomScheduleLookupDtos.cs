namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

/// <summary>
/// Ringkasan data dokter untuk lookup booking operasi (BE-OPR-013, OPS-DEC-040).
/// </summary>
public class OprDoctorSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data ringkasan rencana operasi terkonfirmasi untuk dikonsumsi Admisi Rawat Inap (BE-OPR-013, OPS-DEC-040).
/// </summary>
public class OprPatientBookingLookupResponse
{
    public string BookingNumber { get; set; } = string.Empty;
    public Guid CaseId { get; set; }
    public Guid PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public string ClinicalCategory { get; set; } = string.Empty;
    public string OperatingRoomName { get; set; } = string.Empty;
    public string ScheduledDate { get; set; } = string.Empty;
    public string ScheduledTime { get; set; } = string.Empty;
    public DateTime ScheduledStartAtUtc { get; set; }
    public DateTime ScheduledEndAtUtc { get; set; }
    public OprDoctorSummaryDto? LeadSurgeon { get; set; }
    public OprDoctorSummaryDto? Anesthesiologist { get; set; }
    public OprDoctorSummaryDto? Pediatrician { get; set; }
    public string? PreOpInstructions { get; set; }
}
