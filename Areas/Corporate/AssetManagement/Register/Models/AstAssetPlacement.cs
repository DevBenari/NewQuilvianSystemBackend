using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Riwayat penempatan aset (data-dictionary.md bagian 3.2, BE-AST-003). Baris dengan <see cref="EndAt"/>
/// kosong adalah penempatan berjalan; database menjamin paling banyak satu per aset.
/// </summary>
[Table("AstAssetPlacement", Schema = "public")]
public sealed class AstAssetPlacement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public Guid OrganizationUnitId { get; set; }
    public MstOrganizationUnit? OrganizationUnit { get; set; }

    public Guid LocationId { get; set; }
    public MstAssetLocation? Location { get; set; }

    public Guid CustodianEmployeeId { get; set; }
    public MstEmployee? CustodianEmployee { get; set; }

    /// <summary>Asal penempatan: <c>PENYERAHAN</c>, <c>MUTASI</c>, <c>IMPOR</c>, atau <c>PEMULIHAN</c>.</summary>
    [Required, MaxLength(30)] public string SourceType { get; set; } = string.Empty;

    /// <summary>Dokumen asal penempatan (penyerahan, mutasi, batch impor, pemulihan); tanpa FK keras.</summary>
    public Guid? SourceDocumentId { get; set; }

    /// <summary>Saat penerima mengonfirmasi (AST-DEC-044).</summary>
    public DateTime StartAt { get; set; }

    public DateTime? EndAt { get; set; }
}
