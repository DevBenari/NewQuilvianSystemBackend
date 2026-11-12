using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;

/// <summary>
/// Jenis dokumen yang wajib dimiliki aset pada satu kategori (AST-RCG-G14, data-dictionary.md bagian 2.6,
/// BE-AST-002). Satu pasangan kategori–jenis dokumen hanya boleh ada sekali di antara baris yang belum dihapus.
/// </summary>
[Table("MstAssetCategoryDocumentRequirement", Schema = "public")]
public sealed class MstAssetCategoryDocumentRequirement : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CategoryId { get; set; }
    public MstAssetCategory? Category { get; set; }

    public Guid DocumentTypeId { get; set; }
    public MstAssetDocumentType? DocumentType { get; set; }
}
