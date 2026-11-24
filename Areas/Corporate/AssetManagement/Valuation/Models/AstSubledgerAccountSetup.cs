using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstSubledgerAccountSetup", Schema = "public")]
public sealed class AstSubledgerAccountSetup : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetGroupId { get; set; }
    public MstAssetGroup? AssetGroup { get; set; }

    [MaxLength(50)] public string CostControlAccountCode { get; set; } = string.Empty;

    [MaxLength(50)] public string AccumulatedDepreciationControlAccountCode { get; set; } = string.Empty;

    [MaxLength(50)] public string? AccumulatedImpairmentControlAccountCode { get; set; }
}
