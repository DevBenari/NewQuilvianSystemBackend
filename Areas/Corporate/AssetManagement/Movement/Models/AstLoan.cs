using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.HumanResource.MasterData.Organization.Models;
using QuilvianSystemBackend.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Models;

[Table("AstLoan", Schema = "public")]
public sealed class AstLoan : IdentityModel
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(30)] public string LoanNumber { get; set; } = string.Empty;

    public Guid OwnerOrganizationUnitId { get; set; }
    public MstOrganizationUnit? OwnerOrganizationUnit { get; set; }

    public Guid BorrowerOrganizationUnitId { get; set; }
    public MstOrganizationUnit? BorrowerOrganizationUnit { get; set; }

    [Required, MaxLength(1000)] public string Purpose { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly PlannedReturnDate { get; set; }

    public DateOnly OriginalPlannedReturnDate { get; set; }

    public LoanStatus Status { get; set; } = LoanStatus.Diajukan;

    public Guid RequestedBy { get; set; }
    public DateTime RequestedAt { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public bool IsApprovalByRequester { get; set; }

    public Guid? PickedUpBy { get; set; }
    public DateTime? PickedUpAt { get; set; }

    public Guid? ReturnedBy { get; set; }
    public DateTime? ReturnedAt { get; set; }

    public Guid? ReturnConfirmedBy { get; set; }
    public DateTime? ReturnConfirmedAt { get; set; }

    public Guid? RejectedBy { get; set; }
    public DateTime? RejectedAt { get; set; }

    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    [MaxLength(500)] public string? DecisionReason { get; set; }

    public Guid RowVersion { get; set; } = Guid.NewGuid();

    public ICollection<AstLoanItem> Items { get; set; } = new List<AstLoanItem>();

    public ICollection<AstLoanExtension> Extensions { get; set; } = new List<AstLoanExtension>();
}
