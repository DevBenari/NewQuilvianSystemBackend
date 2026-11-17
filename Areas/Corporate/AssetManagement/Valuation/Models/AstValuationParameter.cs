using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstValuationParameter", Schema = "public")]
public sealed class AstValuationParameter : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LegalEntityId { get; set; }
    public MstLegalEntity? LegalEntity { get; set; }

    public DateOnly EffectiveFromPeriod { get; set; }

    public decimal CapitalizationThreshold { get; set; }

    public decimal DisposalDirectorThreshold { get; set; }

    public decimal UtilizationDirectorThreshold { get; set; }

    [MaxLength(500)] public string? Note { get; set; }

    public Guid SetBy { get; set; }

    public DateTime SetAt { get; set; }
}
