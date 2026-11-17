using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>Satu baris berkas impor beserta galatnya (data-dictionary.md bagian 3.9, BE-AST-003).</summary>
[Table("AstImportBatchRow", Schema = "public")]
public sealed class AstImportBatchRow : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid ImportBatchId { get; set; }
    public AstImportBatch? ImportBatch { get; set; }

    /// <summary>Nomor baris pada berkas Excel.</summary>
    public int RowNumber { get; set; }

    /// <summary>Isi baris apa adanya, disimpan untuk audit. Sensitif: memuat angka rupiah.</summary>
    [Required] public string RawJson { get; set; } = string.Empty;

    /// <summary>Gabungan pesan galat <c>AST-VAL-REG-062..066</c>.</summary>
    [MaxLength(2000)] public string? ErrorMessages { get; set; }

    /// <summary>Aset yang terbentuk dari baris ini; terisi saat batch diterapkan.</summary>
    public Guid? CreatedAssetId { get; set; }
    public AstAsset? CreatedAsset { get; set; }
}
