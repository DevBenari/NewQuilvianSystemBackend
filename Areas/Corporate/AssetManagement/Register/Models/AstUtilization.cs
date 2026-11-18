using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Permohonan dan perjanjian pemanfaatan aset oleh pihak lain — sewa, KSO, pinjam pakai (AST-DEC-021, 029,
/// data-dictionary.md bagian 3.8, BE-AST-003). Penyetuju terakhir dan batas yang dipakai dikunci saat diajukan.
/// </summary>
[Table("AstUtilization", Schema = "public")]
public sealed class AstUtilization : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor <c>AST-PMF-######</c> dari deret <c>AST_UTILIZATION</c>.</summary>
    [Required, MaxLength(30)] public string UtilizationNumber { get; set; } = string.Empty;

    public UtilizationType UtilizationType { get; set; }

    /// <summary>Nama mitra. Sensitif.</summary>
    [Required, MaxLength(200)] public string PartnerName { get; set; } = string.Empty;

    [Required, MaxLength(100)] public string AgreementNumber { get; set; } = string.Empty;

    /// <summary>Total nilai perjanjian. Sensitif.</summary>
    public decimal AgreementValue { get; set; } = 0m;

    public DateOnly StartDate { get; set; }

    public DateOnly EndDate { get; set; }

    [MaxLength(1000)] public string? Purpose { get; set; }

    public UtilizationStatus Status { get; set; } = UtilizationStatus.Draft;

    /// <summary>Penyetuju terakhir yang dibutuhkan, dikunci saat diajukan.</summary>
    public UtilizationApprovalLevel? RequiredApprovalLevel { get; set; }

    /// <summary>Batas rupiah dari sub-modul nilai yang dipakai saat diajukan.</summary>
    public decimal? ThresholdUsed { get; set; }

    public Guid? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public Guid? AssetHeadDecidedBy { get; set; }
    public DateTime? AssetHeadDecidedAt { get; set; }

    public Guid? DirectorDecidedBy { get; set; }
    public DateTime? DirectorDecidedAt { get; set; }

    public Guid? EndedBy { get; set; }
    public DateTime? EndedAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    public Guid? AgreementAttachmentId { get; set; }
    public AstAttachment? AgreementAttachment { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<AstUtilizationItem> Items { get; set; } = new List<AstUtilizationItem>();
}
