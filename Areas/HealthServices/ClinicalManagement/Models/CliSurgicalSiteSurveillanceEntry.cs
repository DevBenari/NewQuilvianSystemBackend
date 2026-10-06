using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliSurgicalSiteSurveillanceEntry", Schema = "public")]
public class CliSurgicalSiteSurveillanceEntry : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SurveillanceId { get; set; }

    public int DayNumber { get; set; }

    public DateOnly EntryDate { get; set; }

    public string ResponsesJson { get; set; } = string.Empty;

    public decimal? TemperatureMaxCelsiusSnapshot { get; set; }

    public bool? FeverIndicatorFromVitals { get; set; }

    public Guid RecordedByUserId { get; set; }

    public int RevisionNumber { get; set; } = 1;
}
