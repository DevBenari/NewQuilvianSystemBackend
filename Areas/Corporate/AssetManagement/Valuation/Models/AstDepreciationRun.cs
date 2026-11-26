using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstDepreciationRun", Schema = "public")]
public sealed class AstDepreciationRun : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid LegalEntityId { get; set; }
    public MstLegalEntity? LegalEntity { get; set; }

    public DateOnly Period { get; set; }

    public int AssetCount { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal AdjustmentAmount { get; set; }

    public Guid ProcessedBy { get; set; }

    public DateTime ProcessedAt { get; set; }

    public ICollection<AstDepreciationRunLine> Lines { get; set; } = new List<AstDepreciationRunLine>();
}
