using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliTransfusionMonitoringPoint", Schema = "public")]
public class CliTransfusionMonitoringPoint : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid MonitoringId { get; set; }

    public CliTransfusionPointType PointType { get; set; }

    public DateTime DueAt { get; set; }

    public DateTime? MeasuredAt { get; set; }

    public int? SystolicBp { get; set; }

    public int? DiastolicBp { get; set; }

    public decimal? TemperatureCelsius { get; set; }

    public int? PulseRate { get; set; }

    public bool IsLate { get; set; } = false;

    public bool IsStopped { get; set; } = false;

    [MaxLength(500)]
    public string? LateNote { get; set; }

    public Guid? RecordedByUserId { get; set; }

    public int RevisionNumber { get; set; } = 0;

    [MaxLength(500)]
    public string? CorrectionReason { get; set; }
}
