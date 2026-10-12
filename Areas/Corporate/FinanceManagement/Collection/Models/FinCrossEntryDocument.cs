using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Models;

/// <summary>
/// Dokumen lampiran pendukung Ayat Silang (bukti transfer/rekening koran/surat penjamin).
/// Berkas fisik tersimpan di bawah folder uploads/finance/cross-entry-documents/
/// </summary>
[Table("FinCrossEntryDocument", Schema = "public")]
public sealed class FinCrossEntryDocument : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CrossEntryId { get; set; }
    public FinCrossEntry? CrossEntry { get; set; }

    [Required, MaxLength(200)]
    public string DocumentName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string OriginalFileName { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string StoredFileName { get; set; } = string.Empty;

    [Required, MaxLength(500)]
    public string RelativePath { get; set; } = string.Empty;

    [Required, MaxLength(150)]
    public string ContentType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }
}
