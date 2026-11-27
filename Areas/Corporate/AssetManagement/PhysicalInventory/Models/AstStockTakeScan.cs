using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

[Table("AstStockTakeScan", Schema = "public")]
public sealed class AstStockTakeScan : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid SessionId { get; set; }
    public AstStockTakeSession? Session { get; set; }

    public Guid? AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    [MaxLength(60)] public string? ScannedCode { get; set; }

    public bool IsManualEntry { get; set; }

    public bool IsUnregisteredFinding { get; set; }

    [MaxLength(500)] public string? FindingDescription { get; set; }

    public Guid FoundLocationId { get; set; }
    public MstAssetLocation? FoundLocation { get; set; }

    public AssetCondition ObservedCondition { get; set; }

    public Guid? PhotoAttachmentId { get; set; }
    public AstAttachment? PhotoAttachment { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }

    public Guid ScannedByEmployeeId { get; set; }
    public MstEmployee? ScannedByEmployee { get; set; }

    public DateTime ScannedAt { get; set; }

    public bool IsLatest { get; set; } = true;
}
