using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;

public sealed class AssetLoanPagedQuery
{
    public string? Search { get; set; }
    public List<LoanStatus>? Status { get; set; }
    public Guid? OwnerOrganizationUnitId { get; set; }
    public Guid? BorrowerOrganizationUnitId { get; set; }
    public bool? IsOverdue { get; set; }
    public bool? AwaitingMe { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string SortBy { get; set; } = "requestedAt";
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetLoanableAssetQuery
{
    public string? Search { get; set; }
    public Guid? OwnerOrganizationUnitId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetLoanCreateRequest
{
    public Guid? BorrowerOrganizationUnitId { get; set; }
    public string? Purpose { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? PlannedReturnDate { get; set; }
    public List<Guid>? AssetIds { get; set; }
}

public sealed class AssetLoanDecisionRequest
{
    public bool Approve { get; set; }
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetLoanReasonRequest
{
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetLoanStepRequest
{
    public Guid? RowVersion { get; set; }
}

public sealed class AssetLoanReturnRequest
{
    public List<AssetLoanReturnItemRequest>? Items { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetLoanReturnItemRequest
{
    public Guid AssetId { get; set; }
    public AssetCondition? ReturnCondition { get; set; }
}

public sealed class AssetLoanExtensionRequest
{
    public DateOnly? RequestedReturnDate { get; set; }
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetLoanResponse
{
    public Guid Id { get; set; }
    public string LoanNumber { get; set; } = string.Empty;
    public Guid OwnerOrganizationUnitId { get; set; }
    public string? OwnerOrganizationUnitName { get; set; }
    public Guid BorrowerOrganizationUnitId { get; set; }
    public string? BorrowerOrganizationUnitName { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly PlannedReturnDate { get; set; }
    public DateOnly OriginalPlannedReturnDate { get; set; }
    public LoanStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public int OverdueDays { get; set; }
    public AssetMovementActorResponse? Requested { get; set; }
    public AssetMovementActorResponse? Approved { get; set; }
    public bool IsApprovalByRequester { get; set; }
    public AssetMovementActorResponse? PickedUp { get; set; }
    public AssetMovementActorResponse? Returned { get; set; }
    public AssetMovementActorResponse? ReturnConfirmed { get; set; }
    public AssetMovementActorResponse? Rejected { get; set; }
    public AssetMovementActorResponse? Cancelled { get; set; }
    public string? DecisionReason { get; set; }
    public int ItemCount { get; set; }
    public AssetLoanExtensionResponse? OpenExtension { get; set; }
    public string? AwaitingParty { get; set; }
    public bool IsAwaitingMe { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public List<AssetLoanItemResponse> Items { get; set; } = [];
    public List<AssetLoanExtensionResponse> Extensions { get; set; } = [];
    public Guid RowVersion { get; set; }
}

public sealed class AssetLoanItemResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public AssetCondition ConditionAtPickup { get; set; }
    public string ConditionAtPickupName { get; set; } = string.Empty;
    public AssetCondition? ReturnCondition { get; set; }
    public string? ReturnConditionName { get; set; }
}

public sealed class AssetLoanExtensionResponse
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public DateOnly PreviousReturnDate { get; set; }
    public DateOnly RequestedReturnDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LoanExtensionStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public AssetMovementActorResponse? Requested { get; set; }
    public AssetMovementActorResponse? Decided { get; set; }
    public string? DecisionReason { get; set; }
}

public sealed class AssetLoanableAssetResponse
{
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public AssetCondition Condition { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public Guid OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public string? LocationName { get; set; }
}

public sealed class AssetLoanSummaryResponse
{
    public int Total { get; set; }
    public int Submitted { get; set; }
    public int Approved { get; set; }
    public int Borrowed { get; set; }
    public int AwaitingReturnConfirmation { get; set; }
    public int Returned { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int Overdue { get; set; }
    public int AwaitingMe { get; set; }
}
