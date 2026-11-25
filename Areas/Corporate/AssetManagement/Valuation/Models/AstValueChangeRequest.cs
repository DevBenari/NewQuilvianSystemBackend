using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Models;

[Table("AstValueChangeRequest", Schema = "public")]
public sealed class AstValueChangeRequest : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [MaxLength(30)] public string RequestNumber { get; set; } = string.Empty;

    public Guid AssetId { get; set; }
    public AstAsset? Asset { get; set; }

    public ValueChangeType ChangeType { get; set; }

    public decimal? Amount { get; set; }

    public int? AdditionalLifeMonths { get; set; }

    public int? NewUsefulLifeMonths { get; set; }

    public decimal? NewResidualValue { get; set; }

    [MaxLength(60)] public string? InvoiceNumber { get; set; }

    public Guid EvidenceAttachmentId { get; set; }
    public AstAttachment? EvidenceAttachment { get; set; }

    [MaxLength(1000)] public string Reason { get; set; } = string.Empty;

    public ValueChangeStatus Status { get; set; } = ValueChangeStatus.Diajukan;

    public Guid? RequestedBy { get; set; }

    public DateTime? RequestedAt { get; set; }

    public Guid? DecidedBy { get; set; }

    public DateTime? DecidedAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    public Guid? AppliedEntryId { get; set; }
    public AstAssetValueEntry? AppliedEntry { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();
}
