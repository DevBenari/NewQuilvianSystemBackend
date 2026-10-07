using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliSurgicalSiteSurveillance", Schema = "public")]
public class CliSurgicalSiteSurveillance : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OprCaseId { get; set; }

    public Guid InpEpisodeId { get; set; }

    public Guid EncounterId { get; set; }

    public Guid PatientId { get; set; }

    public Guid InstrumentVersionId { get; set; }

    public DateTime SurgeryCompletedAt { get; set; }

    public DateOnly DayOneDate { get; set; }

    public CliSurveillanceStatus Status { get; set; } = CliSurveillanceStatus.Active;

    public DateTime? StoppedAt { get; set; }

    public int? StoppedOnDayNumber { get; set; }

    public string SummaryResponsesJson { get; set; } = "{}";

    public Guid? NosocomialInfectionId { get; set; }

    public DateTime? SuspectedFlaggedAt { get; set; }

    public Guid? SuspectedFlaggedByUserId { get; set; }

    [MaxLength(1000)]
    public string? ReviewNote { get; set; }

    [MaxLength(500)]
    public string? CancelReason { get; set; }

    public int Version { get; set; } = 1;
}
