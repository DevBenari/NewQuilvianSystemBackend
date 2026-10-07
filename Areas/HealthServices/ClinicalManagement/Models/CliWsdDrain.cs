using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliWsdDrain", Schema = "public")]
public class CliWsdDrain : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid InpEpisodeId { get; set; }

    public Guid EncounterId { get; set; }

    public Guid PatientId { get; set; }

    [MaxLength(50)]
    public string DrainLabel { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? InsertionSite { get; set; }

    public DateTime InsertedAt { get; set; }

    public decimal InitialResidualMl { get; set; } = 0;

    public DateTime? RemovedAt { get; set; }

    public Guid? RemovedByUserId { get; set; }

    public CliWsdDrainStatus Status { get; set; } = CliWsdDrainStatus.Active;

    public Guid RegisteredByUserId { get; set; }

    [MaxLength(500)]
    public string? CorrectionReason { get; set; }

    public int Version { get; set; } = 1;
}
