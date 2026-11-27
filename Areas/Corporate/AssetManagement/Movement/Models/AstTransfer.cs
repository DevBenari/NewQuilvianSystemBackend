using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Workforce.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

[Table("AstTransfer", Schema = "public")]
public sealed class AstTransfer : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)] public string TransferNumber { get; set; } = string.Empty;

    public Guid SourceOrganizationUnitId { get; set; }
    public MstOrganizationUnit? SourceOrganizationUnit { get; set; }

    public Guid TargetOrganizationUnitId { get; set; }
    public MstOrganizationUnit? TargetOrganizationUnit { get; set; }

    public Guid? TargetLocationId { get; set; }
    public MstAssetLocation? TargetLocation { get; set; }

    public Guid? TargetCustodianEmployeeId { get; set; }
    public MstEmployee? TargetCustodianEmployee { get; set; }

    public TransferReason Reason { get; set; }

    [Required, MaxLength(1000)] public string ReasonNote { get; set; } = string.Empty;

    public Guid? CorrectsTransferId { get; set; }
    public AstTransfer? CorrectsTransfer { get; set; }

    public Guid? SourceStockTakeResultId { get; set; }

    public TransferStatus Status { get; set; } = TransferStatus.Diajukan;

    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }

    public Guid? UnitApprovedBy { get; set; }
    public DateTime? UnitApprovedAt { get; set; }

    public bool IsUnitApprovalByRequester { get; set; }

    public Guid? AssetHeadApprovedBy { get; set; }
    public DateTime? AssetHeadApprovedAt { get; set; }

    public Guid? ReceivedBy { get; set; }
    public DateTime? ReceivedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }

    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<AstTransferItem> Items { get; set; } = new List<AstTransferItem>();
}
