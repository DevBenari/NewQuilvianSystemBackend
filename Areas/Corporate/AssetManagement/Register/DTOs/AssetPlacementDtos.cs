using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

public sealed class AssetHandoverCreateRequest
{
    public HandoverPurpose? Purpose { get; set; }
    public Guid? TargetOrganizationUnitId { get; set; }
    public Guid? TargetLocationId { get; set; }
    public Guid? TargetCustodianEmployeeId { get; set; }
    public List<Guid> AssetIds { get; set; } = [];
    public string? Notes { get; set; }
}

public sealed class AssetHandoverDecisionRequest
{
    public string? Reason { get; set; }
}

public sealed class AssetPlacementPagedQuery
{
    public string? Search { get; set; }
    public HandoverStatus? Status { get; set; }
    public HandoverPurpose? Purpose { get; set; }
    public Guid? TargetOrganizationUnitId { get; set; }
    public bool? AwaitingMyConfirmation { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? CustodianEmployeeId { get; set; }
    public Guid? AssetId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetHandoverActorResponse
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? PositionName { get; set; }
    public DateTime At { get; set; }
}

public sealed class AssetHandoverItemResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public AssetCondition ConditionAtHandover { get; set; }
    public string ConditionAtHandoverName { get; set; } = string.Empty;
}

public sealed class AssetHandoverResponse
{
    public Guid Id { get; set; }
    public string HandoverNumber { get; set; } = string.Empty;
    public HandoverPurpose Purpose { get; set; }
    public string PurposeName { get; set; } = string.Empty;
    public HandoverStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public Guid TargetOrganizationUnitId { get; set; }
    public string? TargetOrganizationUnitName { get; set; }
    public Guid TargetLocationId { get; set; }
    public string? TargetLocationName { get; set; }
    public Guid TargetCustodianEmployeeId { get; set; }
    public string? TargetCustodianEmployeeName { get; set; }
    public string? Notes { get; set; }
    public AssetHandoverActorResponse? HandedOver { get; set; }
    public AssetHandoverActorResponse? Confirmed { get; set; }
    public AssetHandoverActorResponse? Rejected { get; set; }
    public AssetHandoverActorResponse? Cancelled { get; set; }
    public string? DecisionReason { get; set; }
    public int ItemCount { get; set; }
    public List<AssetHandoverItemResponse> Items { get; set; } = [];
    public bool IsAwaitingMyConfirmation { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public Guid RowVersion { get; set; }
}

public sealed class AssetHandoverSummaryResponse
{
    public int Total { get; set; }
    public int Submitted { get; set; }
    public int Confirmed { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int AwaitingMyConfirmation { get; set; }
}

public sealed class AssetPlacementListItemResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public AssetCondition Condition { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public Guid OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public Guid LocationId { get; set; }
    public string? LocationName { get; set; }
    public Guid CustodianEmployeeId { get; set; }
    public string? CustodianEmployeeName { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceDocumentId { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public bool IsCurrent { get; set; }
}

public sealed record AssetPlacementTarget(Guid OrganizationUnitId, Guid LocationId, Guid CustodianEmployeeId);

public sealed record AssetHandoverStockTakeRequest(
    Guid AssetId,
    Guid TargetLocationId,
    Guid TargetCustodianEmployeeId,
    Guid SourceDocumentId,
    string? Notes,
    Guid ActorUserId);
