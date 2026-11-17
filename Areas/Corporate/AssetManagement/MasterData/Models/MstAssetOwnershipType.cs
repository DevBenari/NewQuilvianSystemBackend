using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Jenis kepemilikan aset — milik RS, sewa, KSO, pinjam pakai (data-dictionary.md bagian 2.4, BE-AST-002).
/// </summary>
[Table("MstAssetOwnershipType", Schema = "public")]
public sealed class MstAssetOwnershipType : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kode kepemilikan, contoh <c>MILIK</c>, <c>SEWA</c>, <c>KSO</c>, <c>PINJAM_PAKAI</c>.</summary>
    [Required, MaxLength(30)] public string OwnershipCode { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string OwnershipName { get; set; } = string.Empty;

    /// <summary>Aset dengan jenis kepemilikan ini dapat dikapitalisasi (AST-RCG-G25).</summary>
    public bool IsCapitalizable { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
