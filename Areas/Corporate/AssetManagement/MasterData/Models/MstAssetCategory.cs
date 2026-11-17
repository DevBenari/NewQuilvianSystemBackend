using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Kategori besar aset — Alat Medis, Kendaraan, Gedung, Tanah, Mebel (AST-BP-001 register,
/// data-dictionary.md bagian 2.1, BE-AST-002). Tidak dihapus bila sudah dipakai; cukup dinonaktifkan.
/// </summary>
[Table("MstAssetCategory", Schema = "public")]
public sealed class MstAssetCategory : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kode kategori, contoh <c>ALAT_MEDIS</c>. Unik di antara baris yang belum dihapus.</summary>
    [Required, MaxLength(30)] public string CategoryCode { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string CategoryName { get; set; } = string.Empty;

    /// <summary><c>false</c> untuk Tanah (AST-RCG-G27): kategori ini tidak disusutkan.</summary>
    public bool IsDepreciable { get; set; } = true;

    [MaxLength(500)] public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
