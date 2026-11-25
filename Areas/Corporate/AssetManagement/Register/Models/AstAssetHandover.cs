using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;

/// <summary>
/// Penyerahan aset beserta Berita Acara Penyerahan (AST-DEC-044, AST-DES-006, data-dictionary.md bagian 3.3,
/// BE-AST-003). Lokasi dan penanggung jawab aset baru berpindah saat penerima yang dituju mengonfirmasi.
/// </summary>
[Table("AstAssetHandover", Schema = "public")]
public sealed class AstAssetHandover : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Nomor <c>AST-PNY-######</c> dari deret <c>AST_HANDOVER</c>.</summary>
    [Required, MaxLength(30)] public string HandoverNumber { get; set; } = string.Empty;

    public HandoverPurpose Purpose { get; set; }

    public HandoverStatus Status { get; set; } = HandoverStatus.Diserahkan;

    public Guid TargetOrganizationUnitId { get; set; }
    public MstOrganizationUnit? TargetOrganizationUnit { get; set; }

    public Guid TargetLocationId { get; set; }
    public MstAssetLocation? TargetLocation { get; set; }

    public Guid TargetCustodianEmployeeId { get; set; }
    public MstEmployee? TargetCustodianEmployee { get; set; }

    /// <summary>Hasil opname atau pemulihan yang memicu penyerahan ini; tanpa FK keras.</summary>
    public Guid? SourceDocumentId { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    public Guid HandedOverBy { get; set; }
    public DateTime HandedOverAt { get; set; }

    public Guid? ConfirmedBy { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }

    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<AstAssetHandoverItem> Items { get; set; } = new List<AstAssetHandoverItem>();
}
