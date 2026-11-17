using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Laporan dan penetapan kondisi aset (AST-DEC-017, data-dictionary.md bagian 3.4, BE-AST-003). Kondisi aset
/// baru berubah setelah laporan ditetapkan. Database menjamin paling banyak satu laporan terbuka
/// (<c>Dilaporkan</c>) per aset.
/// </summary>
[Table("AstConditionReport", Schema = "public")]
public sealed class AstConditionReport : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor <c>AST-KND-######</c> dari deret <c>AST_CONDITION</c>.</summary>
    [Required, MaxLength(30)] public string ReportNumber { get; set; } = string.Empty;

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public ConditionReportSource Source { get; set; }

    /// <summary>Pinjaman atau hasil opname yang memicu laporan ini; tanpa FK keras.</summary>
    public Guid? SourceDocumentId { get; set; }

    public AssetCondition PreviousCondition { get; set; }

    public AssetCondition ReportedCondition { get; set; }

    /// <summary>Aset yang tercatat hilang ternyata ditemukan kembali.</summary>
    public bool IsFoundAgain { get; set; } = false;

    [MaxLength(1000)] public string? Notes { get; set; }

    /// <summary>Foto kondisi; wajib bila laporan berasal dari unit (ditegakkan service).</summary>
    public Guid? PhotoAttachmentId { get; set; }
    public AstAttachment? PhotoAttachment { get; set; }

    /// <summary>Berita acara kehilangan; wajib untuk kondisi <c>Hilang</c>.</summary>
    public Guid? LossReportAttachmentId { get; set; }
    public AstAttachment? LossReportAttachment { get; set; }

    public ConditionReportStatus Status { get; set; } = ConditionReportStatus.Dilaporkan;

    public Guid ReportedBy { get; set; }
    public DateTime ReportedAt { get; set; }

    public AssetCondition? DecidedCondition { get; set; }

    public Guid? DecidedBy { get; set; }
    public DateTime? DecidedAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
