using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliEquipmentUsageRevision", Schema = "public")]
public class CliEquipmentUsageRevision : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EquipmentUsageId { get; set; }

    public int RevisionNumber { get; set; }

    public DateTime PreviousStartedAt { get; set; }

    public DateTime? PreviousEndedAt { get; set; }

    public decimal? PreviousQuantity { get; set; }

    public decimal? PreviousBilledUnits { get; set; }

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public Guid RevisedByUserId { get; set; }

    public DateTime RevisedAt { get; set; }
}
