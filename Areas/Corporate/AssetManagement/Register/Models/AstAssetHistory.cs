using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Linimasa aset lintas sub-modul (AST-DES-004, data-dictionary.md bagian 3.5, BE-AST-003). Ditulis hanya lewat
/// <c>AssetHistoryService</c> dan tidak pernah diubah atau dihapus. Nilai rupiah tidak boleh masuk ke
/// <see cref="FromValue"/>/<see cref="ToValue"/>.
/// </summary>
[Table("AstAssetHistory", Schema = "public")]
public sealed class AstAssetHistory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public AssetHistoryEventType EventType { get; set; }

    /// <summary>Sub-modul penulis: <c>register</c>, <c>pergerakan</c>, <c>inventarisasi</c>, <c>nilai</c>, <c>penghapusan</c>.</summary>
    [Required, MaxLength(20)] public string SubModule { get; set; } = string.Empty;

    /// <summary>Teks terbaca keadaan sebelum, tanpa rupiah.</summary>
    [MaxLength(250)] public string? FromValue { get; set; }

    /// <summary>Teks terbaca keadaan sesudah, tanpa rupiah.</summary>
    [MaxLength(250)] public string? ToValue { get; set; }

    [MaxLength(40)] public string? SourceDocumentType { get; set; }

    public Guid? SourceDocumentId { get; set; }

    [MaxLength(30)] public string? SourceDocumentNumber { get; set; }

    public Guid ActorUserId { get; set; }

    [MaxLength(500)] public string? Reason { get; set; }

    public DateTime OccurredAt { get; set; }
}
