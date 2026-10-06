using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuilvianSystemBackend.Models;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Enums;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Models;

[Table("CliSurgicalSiteSurveillanceEntryRevision", Schema = "public")]
public class CliSurgicalSiteSurveillanceEntryRevision : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid EntryId { get; set; }

    public int RevisionNumber { get; set; }

    public string PreviousResponsesJson { get; set; } = string.Empty;

    [MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    public Guid RevisedByUserId { get; set; }

    public DateTime RevisedAt { get; set; }
}
