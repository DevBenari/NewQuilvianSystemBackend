using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Satu berkas impor data awal aset (AST-DEC-037, 050, data-dictionary.md bagian 3.9, BE-AST-003). Diterapkan
/// semua-atau-tidak sama sekali, setelah dua persetujuan: Kepala Bagian Aset lalu Akuntansi.
/// </summary>
[Table("AstImportBatch", Schema = "public")]
public sealed class AstImportBatch : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor <c>AST-IMP-######</c> dari deret <c>AST_IMPORT</c>.</summary>
    [Required, MaxLength(30)] public string ImportNumber { get; set; } = string.Empty;

    /// <summary>Tanggal awal register, disalin dari <c>MstAssetSetting</c> saat unggah.</summary>
    public DateOnly CutoverDate { get; set; }

    /// <summary>Berkas Excel asli.</summary>
    public Guid FileAttachmentId { get; set; }
    public AstAttachment? FileAttachment { get; set; }

    public ImportBatchStatus Status { get; set; } = ImportBatchStatus.Diunggah;

    public int RowCount { get; set; } = 0;

    public int ErrorCount { get; set; } = 0;

    /// <summary>Ringkasan untuk Akuntansi. Sensitif.</summary>
    public decimal TotalAcquisitionCost { get; set; } = 0m;

    /// <summary>Ringkasan untuk Akuntansi. Sensitif.</summary>
    public decimal TotalAccumulatedDepreciation { get; set; } = 0m;

    public Guid? UploadedBy { get; set; }
    public DateTime? UploadedAt { get; set; }

    public Guid? AssetHeadApprovedBy { get; set; }
    public DateTime? AssetHeadApprovedAt { get; set; }

    public Guid? AccountingApprovedBy { get; set; }
    public DateTime? AccountingApprovedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<AstImportBatchRow> Rows { get; set; } = new List<AstImportBatchRow>();
}
