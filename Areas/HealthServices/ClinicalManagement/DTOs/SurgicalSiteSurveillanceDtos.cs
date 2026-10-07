using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
public class SurveillanceQuery
{
    public Guid? EpisodeId { get; set; }
    public CliSurveillanceStatus? Status { get; set; }
    public Guid? ServiceUnitId { get; set; }
    public bool? OnlySuspected { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}
public class PutSurveillanceEntryRequest { [Required] public string ResponsesJson { get; set; } = "{}"; [MaxLength(500)] public string? CorrectionReason { get; set; } public int? ExpectedRevision { get; set; } }
public class PutSurveillanceSummaryRequest { [Required] public string SummaryResponsesJson { get; set; } = "{}"; [MaxLength(500)] public string? Reason { get; set; } public int ExpectedVersion { get; set; } }
public class FlagSurveillanceSuspectedRequest { public DateOnly OnsetDate { get; set; } [Required, MaxLength(1000)] public string Note { get; set; } = string.Empty; }
public class SurveillanceListItem
{
    public Guid Id { get; set; }
    public Guid OprCaseId { get; set; }
    public Guid EpisodeId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public DateOnly DayOneDate { get; set; }
    public CliSurveillanceStatus Status { get; set; }
    public Guid? NosocomialInfectionId { get; set; }
    public DateTime? StoppedAt { get; set; }
    public int? StoppedOnDayNumber { get; set; }
    public int Version { get; set; }
}
public class SurveillanceDetailResponse : SurveillanceListItem
{
    public Guid InstrumentVersionId { get; set; }
    public string DefinitionJson { get; set; } = "{}";
    public string SummaryResponsesJson { get; set; } = "{}";
    public List<SurveillanceEntryResponse> Entries { get; set; } = new();
    public List<SurveillanceDayTemperature> Temperatures { get; set; } = new();
}
public class SurveillanceDayTemperature { public int DayNumber { get; set; } public DateOnly Date { get; set; } public decimal? TemperatureMaxCelsius { get; set; } public bool? FeverIndicator { get; set; } }
public class SurveillanceEntryResponse
{
    public Guid Id { get; set; }
    public int DayNumber { get; set; }
    public DateOnly EntryDate { get; set; }
    public string ResponsesJson { get; set; } = "{}";
    public decimal? TemperatureMaxCelsiusSnapshot { get; set; }
    public bool? FeverIndicatorFromVitals { get; set; }
    public int RevisionNumber { get; set; }
}
public class SurveillanceFormReadiness { public bool HasApprovedVersion { get; set; } public string? Warning { get; set; } }
