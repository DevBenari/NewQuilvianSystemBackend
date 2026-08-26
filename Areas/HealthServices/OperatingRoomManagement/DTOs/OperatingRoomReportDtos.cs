using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;

public class OprReportQuery
{
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 200)] public int PageSize { get; set; } = 20;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public OprCaseStatus? Status { get; set; }
    public OprCaseType? CaseType { get; set; }
    public OprPriority? Priority { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? PrimarySurgeonId { get; set; }
    [MaxLength(100)] public string? Search { get; set; }
}

public class OprOperationReportRow
{
    public Guid OprCaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string PrimaryProcedureName { get; set; } = string.Empty;
    public string PrimarySurgeonName { get; set; } = string.Empty;
    public OprCaseType CaseType { get; set; }
    public OprPriority Priority { get; set; }
    public OprCaseStatus Status { get; set; }
    public OprCaseOutcome? Outcome { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public DateTime? ScheduledStartAt { get; set; }
    public DateTime? ScheduledEndAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? FinishedAt { get; set; }
    public int? ActualDurationMinutes { get; set; }
    public int EstimatedMinutes { get; set; }
}

public class OprUtilizationQuery
{
    [Required] public DateTime From { get; set; }
    [Required] public DateTime To { get; set; }
    public Guid? RoomId { get; set; }
}

public class OprRoomUtilizationRow
{
    public Guid RoomId { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public int ScheduledCases { get; set; }
    public int ScheduledMinutes { get; set; }
    public int ActualMinutes { get; set; }

    /// <summary>Menit terpakai dibanding menit terjadwal, dibulatkan dua desimal.</summary>
    public decimal RealizationPercent { get; set; }
}

public class OprUtilizationReport
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public List<OprRoomUtilizationRow> Rooms { get; set; } = [];
    public int TotalScheduledCases { get; set; }
    public int CompletedCases { get; set; }
    public int PostponedCases { get; set; }
    public int CancelledCases { get; set; }
}

public class OprMaterialReportQuery
{
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 200)] public int PageSize { get; set; } = 20;
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public Guid? ExternalItemId { get; set; }
    public OprMaterialItemType? ItemType { get; set; }
    public OprMaterialOutcome? Outcome { get; set; }
    [MaxLength(100)] public string? BatchNumber { get; set; }
    [MaxLength(150)] public string? SerialNumber { get; set; }
}

public class OprMaterialReportRow
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public string CaseNumber { get; set; } = string.Empty;
    public Guid ExternalItemId { get; set; }
    public OprMaterialItemType ItemType { get; set; }
    public decimal Quantity { get; set; }
    public string UnitCode { get; set; } = string.Empty;
    public OprMaterialOutcome Outcome { get; set; }
    public string? BatchNumber { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime OccurredAt { get; set; }
    public int Revision { get; set; }
}
