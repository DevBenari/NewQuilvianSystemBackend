using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Klasifikasi medis/non-medis aset (AST-DEC-026, data-dictionary.md bagian 2.2, BE-AST-002).
/// </summary>
[Table("MstAssetClassification", Schema = "public")]
public sealed class MstAssetClassification : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Kode klasifikasi, contoh <c>MEDIS</c>, <c>NON_MEDIS</c>.</summary>
    [Required, MaxLength(30)] public string ClassificationCode { get; set; } = string.Empty;

    [Required, MaxLength(150)] public string ClassificationName { get; set; } = string.Empty;

    /// <summary>Memicu atribut medis wajib saat aset diajukan (AST-RULE-006).</summary>
    public bool IsMedical { get; set; } = false;

    public bool IsActive { get; set; } = true;
}
