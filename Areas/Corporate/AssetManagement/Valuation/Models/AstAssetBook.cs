using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstAssetBook", Schema = "public")]
public sealed class AstAssetBook : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public bool IsCapitalized { get; set; }

    public bool IsDepreciable { get; set; }

    public decimal AcquisitionCost { get; set; }

    public decimal CapitalizedCost { get; set; }

    public decimal ResidualValue { get; set; } = 0m;

    public int UsefulLifeMonths { get; set; }

    public int DepreciatedMonths { get; set; } = 0;

    public DateOnly DepreciationStartPeriod { get; set; }

    public DateOnly? LastDepreciatedPeriod { get; set; }

    public decimal AccumulatedDepreciation { get; set; } = 0m;

    public decimal AccumulatedImpairment { get; set; } = 0m;

    public bool HasEstimateOverride { get; set; } = false;

    public Guid? DepreciationPolicyId { get; set; }
    public AstDepreciationPolicy? DepreciationPolicy { get; set; }

    public AssetBookStatus BookStatus { get; set; } = AssetBookStatus.Aktif;

    public DateTime? StoppedAt { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
