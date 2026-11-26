using QuilvianSystemBackend.Areas.Corporate.AssetManagement.MasterData.Models;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Models;

[Table("AstStockTakeSession", Schema = "public")]
public sealed class AstStockTakeSession : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)] public string SessionNumber { get; set; } = string.Empty;

    public Guid OrganizationUnitId { get; set; }
    public MstOrganizationUnit? OrganizationUnit { get; set; }

    public Guid? LocationId { get; set; }
    public MstAssetLocation? Location { get; set; }

    public DateOnly PlannedStartDate { get; set; }

    public DateOnly PlannedEndDate { get; set; }

    public StockTakeStatus Status { get; set; } = StockTakeStatus.Dijadwalkan;

    public int ExpectedCount { get; set; }

    public int ScannedCount { get; set; }

    public Guid? StartedBy { get; set; }
    public DateTime? StartedAt { get; set; }

    public Guid? CountingClosedBy { get; set; }
    public DateTime? CountingClosedAt { get; set; }

    public Guid? SubmittedBy { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }

    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    [MaxLength(1000)] public string? Notes { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<AstStockTakeTeamMember> TeamMembers { get; set; } = new List<AstStockTakeTeamMember>();

    public ICollection<AstStockTakeExpectedItem> ExpectedItems { get; set; } = new List<AstStockTakeExpectedItem>();

    public ICollection<AstStockTakeScan> Scans { get; set; } = new List<AstStockTakeScan>();

    public ICollection<AstStockTakeResult> Results { get; set; } = new List<AstStockTakeResult>();

    public ICollection<AstStockTakeFollowUp> FollowUps { get; set; } = new List<AstStockTakeFollowUp>();
}
