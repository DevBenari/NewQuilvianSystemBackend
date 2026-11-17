using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstDepreciationPolicy", Schema = "public")]
public sealed class AstDepreciationPolicy : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetGroupId { get; set; }
    public MstAssetGroup? AssetGroup { get; set; }

    public DateOnly EffectiveFromPeriod { get; set; }

    public int UsefulLifeMonths { get; set; }

    public decimal ResidualValuePercent { get; set; } = 0m;

    public Guid SetBy { get; set; }

    public DateTime SetAt { get; set; }
}
