using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Movement.DTOs;

public sealed class AssetTransferPagedQuery
{
    public string? Search { get; set; }
    public TransferStatus? Status { get; set; }
    public TransferReason? Reason { get; set; }
    public Guid? SourceOrganizationUnitId { get; set; }
    public Guid? TargetOrganizationUnitId { get; set; }
    public bool? AwaitingMe { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetTransferCreateRequest
{
    public Guid? TargetOrganizationUnitId { get; set; }
    public TransferReason? Reason { get; set; }
    public string? ReasonNote { get; set; }
    public Guid? CorrectsTransferId { get; set; }
    public Guid? TargetLocationId { get; set; }
    public Guid? TargetCustodianEmployeeId { get; set; }
    public List<Guid>? AssetIds { get; set; }
}

public sealed class AssetTransferDecisionRequest
{
    public bool Approve { get; set; }
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetTransferReasonRequest
{
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetTransferReceiveRequest
{
    public Guid? TargetLocationId { get; set; }
    public Guid? TargetCustodianEmployeeId { get; set; }
    public List<AssetTransferReceiveItemRequest>? Items { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetTransferReceiveItemRequest
{
    public Guid AssetId { get; set; }
    public AssetCondition? ConditionAtReceipt { get; set; }
}

public sealed record AssetTransferStockTakeRequest(
    Guid AssetId,
    Guid TargetOrganizationUnitId,
    Guid? TargetLocationId,
    Guid? TargetCustodianEmployeeId,
    Guid SourceStockTakeResultId,
    string ReasonNote,
    Guid ActorUserId);

public sealed class AssetMovementActorResponse
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? PositionName { get; set; }
    public DateTime At { get; set; }
}

public sealed class AssetTransferResponse
{
    public Guid Id { get; set; }
    public string TransferNumber { get; set; } = string.Empty;
    public Guid SourceOrganizationUnitId { get; set; }
    public string? SourceOrganizationUnitName { get; set; }
    public Guid TargetOrganizationUnitId { get; set; }
    public string? TargetOrganizationUnitName { get; set; }
    public Guid? TargetLocationId { get; set; }
    public string? TargetLocationName { get; set; }
    public Guid? TargetCustodianEmployeeId { get; set; }
    public string? TargetCustodianEmployeeName { get; set; }
    public TransferReason Reason { get; set; }
    public string ReasonName { get; set; } = string.Empty;
    public string ReasonNote { get; set; } = string.Empty;
    public Guid? CorrectsTransferId { get; set; }
    public string? CorrectsTransferNumber { get; set; }
    public Guid? SourceStockTakeResultId { get; set; }
    public TransferStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public AssetMovementActorResponse? Requested { get; set; }
    public AssetMovementActorResponse? UnitApproved { get; set; }
    public bool IsUnitApprovalByRequester { get; set; }
    public AssetMovementActorResponse? AssetHeadApproved { get; set; }
    public AssetMovementActorResponse? Received { get; set; }
    public AssetMovementActorResponse? Rejected { get; set; }
    public AssetMovementActorResponse? Cancelled { get; set; }
    public string? DecisionReason { get; set; }
    public int ItemCount { get; set; }
    public string? AwaitingParty { get; set; }
    public bool IsAwaitingMe { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public List<AssetTransferItemResponse> Items { get; set; } = [];
    public Guid RowVersion { get; set; }
}

public sealed class AssetTransferItemResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public AssetCondition ConditionAtSubmit { get; set; }
    public string ConditionAtSubmitName { get; set; } = string.Empty;
    public AssetCondition? ConditionAtReceipt { get; set; }
    public string? ConditionAtReceiptName { get; set; }
}

public sealed class AssetTransferSummaryResponse
{
    public int Total { get; set; }
    public int Submitted { get; set; }
    public int UnitApproved { get; set; }
    public int AssetHeadApproved { get; set; }
    public int Received { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int AwaitingMe { get; set; }
}
