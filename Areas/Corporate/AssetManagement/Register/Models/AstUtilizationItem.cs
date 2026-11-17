using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>Aset dalam satu pemanfaatan (data-dictionary.md bagian 3.8, BE-AST-003).</summary>
[Table("AstUtilizationItem", Schema = "public")]
public sealed class AstUtilizationItem : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UtilizationId { get; set; }
    public AstUtilization? Utilization { get; set; }

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }
}
