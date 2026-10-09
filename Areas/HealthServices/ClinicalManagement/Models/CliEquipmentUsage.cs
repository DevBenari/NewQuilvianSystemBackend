using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliEquipmentUsage", Schema = "public")]
public class CliEquipmentUsage : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InpEpisodeId { get; set; }

    public Guid EncounterId { get; set; }

    public Guid PatientId { get; set; }

    public Guid MedicalEquipmentId { get; set; }

    public Guid ResponsibleDoctorId { get; set; }

    public Guid PerformedByUserId { get; set; }

    public DateTime StartedAt { get; set; }

    public DateTime? EndedAt { get; set; }

    public decimal? Quantity { get; set; }

    public MstEquipmentChargeUnit ChargeUnitSnapshot { get; set; }

    public MstEquipmentRoundingRule RoundingRuleSnapshot { get; set; }

    public decimal? BilledUnits { get; set; }

    public CliEquipmentUsageStatus Status { get; set; } = CliEquipmentUsageStatus.Running;

    public bool RequiresNurseReview { get; set; } = false;

    public DateTime? AutoClosedAt { get; set; }

    [MaxLength(500)]
    public string? CancelReason { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public int RevisionNumber { get; set; } = 1;

    public int Version { get; set; } = 1;
}
