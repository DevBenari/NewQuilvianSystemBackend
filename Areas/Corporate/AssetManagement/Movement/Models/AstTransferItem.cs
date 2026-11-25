using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

[Table("AstTransferItem", Schema = "public")]
public sealed class AstTransferItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid TransferId { get; set; }
    public AstTransfer? Transfer { get; set; }

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public AssetCondition ConditionAtSubmit { get; set; }

    public AssetCondition? ConditionAtReceipt { get; set; }
}
