using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Kelompok aset beserta kode inventarisnya (data-dictionary.md bagian 2.3, BE-AST-002). Menu
/// "Kode Inventaris" menampilkan kolom <see cref="InventoryCode"/>; tidak ada tabel kode inventaris
/// terpisah. Umur manfaat dan nilai residu bukan milik tabel ini, melainkan <c>AstDepreciationPolicy</c>
/// pada sub-modul nilai.
/// </summary>
[Table("MstAssetGroup", Schema = "public")]
public sealed class MstAssetGroup : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CategoryId { get; set; }
    public MstAssetCategory? Category { get; set; }

    public Guid ClassificationId { get; set; }
    public MstAssetClassification? Classification { get; set; }

    /// <summary>
    /// Kode yang masuk ke nomor inventaris <c>AST-&lt;KODE&gt;-######</c>: 2–11 huruf besar atau angka
    /// (AST-VAL-REG-051), bukan kode dokumen terlarang (AST-DES-014), dan terkunci setelah satu aset
    /// di kelompok ini bernomor (AST-DES-015). Ketiga aturan itu ditegakkan service master, bukan kolom ini.
    /// </summary>
    [Required, MaxLength(11)] public string InventoryCode { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string GroupName { get; set; } = string.Empty;

    [MaxLength(500)] public string? Description { get; set; }

    public bool IsActive { get; set; } = true;
}
