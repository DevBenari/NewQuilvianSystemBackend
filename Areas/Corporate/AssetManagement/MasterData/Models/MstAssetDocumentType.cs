using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Jenis dokumen kepemilikan dan pengamanan aset — STNK, BPKB, sertifikat, polis (data-dictionary.md
/// bagian 2.5, BE-AST-002).
/// </summary>
[Table("MstAssetDocumentType", Schema = "public")]
public sealed class MstAssetDocumentType : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kode jenis dokumen, contoh <c>STNK</c>, <c>BPKB</c>.</summary>
    [Required, MaxLength(30)] public string DocumentTypeCode { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string DocumentTypeName { get; set; } = string.Empty;

    /// <summary>Bila <c>true</c>, dokumen jenis ini wajib punya tanggal berakhir.</summary>
    public bool HasExpiry { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
