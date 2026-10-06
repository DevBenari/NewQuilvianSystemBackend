using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliWsdReading", Schema = "public")]
public class CliWsdReading : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid WsdDrainId { get; set; }

    public Guid InpEpisodeId { get; set; }

    public Guid? ShiftId { get; set; }

    public DateTime PeriodStartAt { get; set; }

    public DateTime PeriodEndAt { get; set; }

    public decimal PreviousResidualMl { get; set; }

    public decimal CurrentResidualMl { get; set; }

    public decimal DiscardedVolumeMl { get; set; } = 0;

    public decimal IncreaseMl { get; set; }

    public Guid FluidBalanceEntryId { get; set; }

    public ClinicalMeasurementStatus Status { get; set; } = ClinicalMeasurementStatus.Active;

    public int RevisionNumber { get; set; } = 1;

    public Guid RecordedByUserId { get; set; }

    [MaxLength(500)]
    public string? CancelReason { get; set; }
}
