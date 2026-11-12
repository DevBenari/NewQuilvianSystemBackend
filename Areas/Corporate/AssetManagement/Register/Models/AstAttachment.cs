using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Metadata berkas lampiran untuk seluruh sub-modul aset (AST-DES-020, data-dictionary.md bagian 3.7,
/// BE-AST-003). Pemiliknya dirujuk lewat <see cref="OwnerType"/> + <see cref="OwnerId"/> tanpa FK keras;
/// kecocokannya diperiksa <c>AssetAttachmentService</c>.
/// </summary>
[Table("AstAttachment", Schema = "public")]
public sealed class AstAttachment : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Jenis pemilik, contoh <c>ConditionReport</c>, <c>StockTakeScan</c>.</summary>
    [Required, MaxLength(40)] public string OwnerType { get; set; } = string.Empty;

    public Guid OwnerId { get; set; }

    [Required, MaxLength(255)] public string OriginalFileName { get; set; } = string.Empty;

    [Required, MaxLength(100)] public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    /// <summary>Hash SHA-256 isi berkas dalam heksadesimal (64 karakter).</summary>
    [Required, MaxLength(64)] public string Sha256 { get; set; } = string.Empty;

    /// <summary>Jalur simpan di server. Sensitif: tidak pernah dikirim ke frontend.</summary>
    [Required, MaxLength(500)] public string StoredPath { get; set; } = string.Empty;
}
