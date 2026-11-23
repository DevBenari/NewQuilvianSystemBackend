using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

[Table("AstStockTakeResult", Schema = "public")]
public sealed class AstStockTakeResult : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }
    public AstStockTakeSession? Session { get; set; }

    public Guid? AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public Guid? ScanId { get; set; }
    public AstStockTakeScan? Scan { get; set; }

    public StockTakeResultCategory Category { get; set; }

    public bool IsLocationMismatch { get; set; }

    public bool IsConditionMismatch { get; set; }

    public bool IsForeignAsset { get; set; }

    public Guid? ExpectedLocationId { get; set; }
    public MstAssetLocation? ExpectedLocation { get; set; }

    public Guid? FoundLocationId { get; set; }
    public MstAssetLocation? FoundLocation { get; set; }

    public AssetCondition? ExpectedCondition { get; set; }

    public AssetCondition? ObservedCondition { get; set; }

    [MaxLength(1000)] public string? TeamNote { get; set; }

    public ICollection<AstStockTakeFollowUp> FollowUps { get; set; } = new List<AstStockTakeFollowUp>();
}
