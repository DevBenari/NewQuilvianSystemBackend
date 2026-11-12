using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>Butir aset dalam satu penyerahan (data-dictionary.md bagian 3.3, BE-AST-003).</summary>
[Table("AstAssetHandoverItem", Schema = "public")]
public sealed class AstAssetHandoverItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid HandoverId { get; set; }
    public AstAssetHandover? Handover { get; set; }

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    /// <summary>Salinan kondisi aset saat diserahkan, untuk berita acara.</summary>
    public AssetCondition ConditionAtHandover { get; set; }
}
