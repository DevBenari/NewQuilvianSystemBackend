using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliTransfusionMonitoring", Schema = "public")]
public class CliTransfusionMonitoring : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InpEpisodeId { get; set; }

    public Guid EncounterId { get; set; }

    public Guid PatientId { get; set; }

    public Guid BloodUnitId { get; set; }

    public DateTime ReceivedAtWardAt { get; set; }

    public DateTime TransfusionStartedAt { get; set; }

    public CliTransfusionMonitoringStatus Status { get; set; } = CliTransfusionMonitoringStatus.InProgress;

    public DateTime? StoppedAt { get; set; }

    [MaxLength(500)]
    public string? StopReason { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid PerformedByUserId { get; set; }

    [MaxLength(500)]
    public string? CancelReason { get; set; }

    public int Version { get; set; } = 1;
}
