using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

public sealed class AssetPagedQuery
{
    public string? Search { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? GroupId { get; set; }
    public AssetRecordStatus? RecordStatus { get; set; }
    public AssetCondition? Condition { get; set; }
    public AssetPlacementState? PlacementState { get; set; }
    public AssetAcquisitionType? AcquisitionType { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetHistoryPagedQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public abstract class AssetDraftFieldsRequest
{
    public string? AssetName { get; set; }
    public string? Description { get; set; }
    public Guid? AssetGroupId { get; set; }
    public Guid? OwnershipTypeId { get; set; }
    public string? Brand { get; set; }
    public string? ModelType { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? DistributionPermitNumber { get; set; }
    public int? ManufactureYear { get; set; }
    public string? DistributorName { get; set; }
    public AssetAcquisitionType? AcquisitionType { get; set; }
    public DateOnly? AcquisitionDate { get; set; }
    public DateOnly? ReadyForUseDate { get; set; }
    public decimal RegisteredAcquisitionCost { get; set; }
    public string? SupplierName { get; set; }
    public Guid? GoodsReceiptId { get; set; }
    public string? DonationReference { get; set; }
}

public sealed class AssetDraftCreateRequest : AssetDraftFieldsRequest
{
    public int Quantity { get; set; } = 1;
}

public sealed class AssetDraftUpdateRequest : AssetDraftFieldsRequest
{
    public Guid RowVersion { get; set; }
}

public sealed class AssetDecisionRequest
{
    public string? Reason { get; set; }
}

public sealed class AssetBatchApproveRequest
{
    public List<Guid> AssetIds { get; set; } = [];
}

public sealed class AssetDecisionResultResponse
{
    public Guid AssetId { get; set; }
    public bool IsSuccess { get; set; }
    public string? InventoryNumber { get; set; }
    public int StatusCode { get; set; }
    public string? Code { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class AssetListItemResponse
{
    public Guid Id { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid AssetGroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public bool IsMedical { get; set; }
    public string OwnershipTypeName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? SerialNumber { get; set; }
    public AssetAcquisitionType AcquisitionType { get; set; }
    public string AcquisitionTypeName { get; set; } = string.Empty;
    public DateOnly AcquisitionDate { get; set; }
    public AssetRecordStatus RecordStatus { get; set; }
    public string RecordStatusName { get; set; } = string.Empty;
    public AssetCondition Condition { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public AssetPlacementState PlacementState { get; set; }
    public string PlacementStateName { get; set; } = string.Empty;
    public Guid? CurrentOrganizationUnitId { get; set; }
    public string? CurrentOrganizationUnitName { get; set; }
    public Guid? CurrentLocationId { get; set; }
    public string? CurrentLocationName { get; set; }
    public Guid? CurrentCustodianEmployeeId { get; set; }
    public string? CurrentCustodianEmployeeName { get; set; }
    public AssetActiveProcessKind ActiveProcessKind { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class AssetDetailResponse
{
    public Guid Id { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid AssetGroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ClassificationName { get; set; } = string.Empty;
    public bool IsMedical { get; set; }
    public Guid OwnershipTypeId { get; set; }
    public string OwnershipTypeName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? ModelType { get; set; }
    public string? SerialNumber { get; set; }
    public string? Manufacturer { get; set; }
    public string? DistributionPermitNumber { get; set; }
    public int? ManufactureYear { get; set; }
    public string? DistributorName { get; set; }
    public AssetAcquisitionType AcquisitionType { get; set; }
    public string AcquisitionTypeName { get; set; } = string.Empty;
    public DateOnly AcquisitionDate { get; set; }
    public DateOnly ReadyForUseDate { get; set; }
    public string? SupplierName { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public Guid? GoodsReceiptId { get; set; }
    public string? GoodsReceiptNumber { get; set; }
    public string? DonationReference { get; set; }
    public AssetRecordStatus RecordStatus { get; set; }
    public string RecordStatusName { get; set; } = string.Empty;
    public AssetCondition Condition { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public AssetPlacementState PlacementState { get; set; }
    public string PlacementStateName { get; set; } = string.Empty;
    public Guid? CurrentOrganizationUnitId { get; set; }
    public string? CurrentOrganizationUnitName { get; set; }
    public Guid? CurrentLocationId { get; set; }
    public string? CurrentLocationName { get; set; }
    public Guid? CurrentCustodianEmployeeId { get; set; }
    public string? CurrentCustodianEmployeeName { get; set; }
    public AssetActiveProcessKind ActiveProcessKind { get; set; }
    public Guid? SubmittedBy { get; set; }
    public string? SubmittedByName { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? RejectedBy { get; set; }
    public string? RejectedByName { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string? DecisionReason { get; set; }
    public Guid CreateBy { get; set; }
    public DateTime CreateDateTime { get; set; }
    public Guid RowVersion { get; set; }
    public List<string> AvailableActions { get; set; } = [];
}

public sealed class AssetAmountResponse
{
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public decimal RegisteredAcquisitionCost { get; set; }
    public AssetBookSummaryResponse? Book { get; set; }
}

public sealed class AssetBookSummaryResponse
{
    public bool IsCapitalized { get; set; }
    public bool IsDepreciable { get; set; }
    public decimal CapitalizedCost { get; set; }
    public decimal ResidualValue { get; set; }
    public decimal AccumulatedDepreciation { get; set; }
    public decimal AccumulatedImpairment { get; set; }
    public decimal NetBookValue { get; set; }
    public int UsefulLifeMonths { get; set; }
    public int DepreciatedMonths { get; set; }
    public DateOnly DepreciationStartPeriod { get; set; }
    public DateOnly? LastDepreciatedPeriod { get; set; }
    public string BookStatusName { get; set; } = string.Empty;
}

public sealed class AssetHistoryItemResponse
{
    public Guid Id { get; set; }
    public AssetHistoryEventType EventType { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public string SubModule { get; set; } = string.Empty;
    public string? FromValue { get; set; }
    public string? ToValue { get; set; }
    public string? SourceDocumentType { get; set; }
    public Guid? SourceDocumentId { get; set; }
    public string? SourceDocumentNumber { get; set; }
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public string? Reason { get; set; }
    public DateTime OccurredAt { get; set; }
}

public sealed class AssetRegisterSummaryResponse
{
    public int Total { get; set; }
    public int Draft { get; set; }
    public int Submitted { get; set; }
    public int Registered { get; set; }
    public int Cancelled { get; set; }
    public int Disposed { get; set; }
    public int RegisteredNotPlaced { get; set; }
    public int RegisteredDamaged { get; set; }
    public int RegisteredLost { get; set; }
}
