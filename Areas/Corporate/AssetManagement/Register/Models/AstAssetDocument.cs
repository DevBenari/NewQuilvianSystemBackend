using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Dokumen kepemilikan dan pengamanan aset — sertifikat, BPKB, STNK, polis (data-dictionary.md bagian 3.6,
/// BE-AST-003).
/// </summary>
[Table("AstAssetDocument", Schema = "public")]
public sealed class AstAssetDocument : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public Guid DocumentTypeId { get; set; }
    public MstAssetDocumentType? DocumentType { get; set; }

    /// <summary>Nomor sertifikat/BPKB. Sensitif: tidak boleh masuk log.</summary>
    [Required, MaxLength(100)] public string DocumentNumber { get; set; } = string.Empty;

    public DateOnly? IssuedDate { get; set; }

    /// <summary>Wajib bila jenis dokumen <c>HasExpiry</c> (ditegakkan service).</summary>
    public DateOnly? ExpiryDate { get; set; }

    /// <summary>Nama pemegang dokumen ("atas nama").</summary>
    [MaxLength(200)] public string? HolderName { get; set; }

    public Guid? AttachmentId { get; set; }
    public AstAttachment? Attachment { get; set; }

    [MaxLength(500)] public string? Notes { get; set; }
}
