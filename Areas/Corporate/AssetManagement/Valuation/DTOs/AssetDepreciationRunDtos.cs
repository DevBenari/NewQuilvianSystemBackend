using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Valuation.DTOs;

public sealed class DepreciationPreviewQuery
{
    public string? Period { get; set; }
}

public sealed class DepreciationRunCreateRequest
{
    public string? Period { get; set; }
}

public sealed class DepreciationRunPagedQuery
{
    public int? Year { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class DepreciationRunLineQuery
{
    public Guid? AssetGroupId { get; set; }
    public Guid? CostCenterId { get; set; }
    public string? Search { get; set; }
    public bool? AdjustmentOnly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class DepreciationIssueResponse
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? AssetGroupId { get; set; }
}

public sealed class DepreciationGroupTotalResponse
{
    public Guid AssetGroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public Guid CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public int AssetCount { get; set; }
    public decimal Amount { get; set; }
    public decimal AdjustmentAmount { get; set; }
}

public sealed class DepreciationPreviewResponse
{
    public DateOnly Period { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public Guid LegalEntityId { get; set; }
    public string LegalEntityName { get; set; } = string.Empty;
    public DateOnly? NextAllowedPeriod { get; set; }
    public string? NextAllowedPeriodLabel { get; set; }
    public bool CanProcess { get; set; }
    public DepreciationIssueResponse? Blocker { get; set; }
    public Guid? ProcessedRunId { get; set; }
    public int AssetCount { get; set; }
    public int LineCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public List<DepreciationGroupTotalResponse> Groups { get; set; } = [];
    public List<DepreciationIssueResponse> Errors { get; set; } = [];
    public List<DepreciationIssueResponse> Warnings { get; set; } = [];
}

public sealed class DepreciationOutboxSummaryResponse
{
    public int Total { get; set; }
    public int Held { get; set; }
    public int Pending { get; set; }
    public int Acknowledged { get; set; }
    public int Failed { get; set; }
}

public sealed class DepreciationRunResponse
{
    public Guid Id { get; set; }
    public Guid LegalEntityId { get; set; }
    public DateOnly Period { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public Guid ProcessedBy { get; set; }
    public string? ProcessedByName { get; set; }
    public DateTime ProcessedAt { get; set; }
    public List<DepreciationGroupTotalResponse> Groups { get; set; } = [];
    public DepreciationOutboxSummaryResponse Outbox { get; set; } = new();
}

public sealed class DepreciationRunLineResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public Guid AssetGroupId { get; set; }
    public string InventoryCode { get; set; } = string.Empty;
    public Guid OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public Guid CostCenterId { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public decimal Amount { get; set; }
    public DepreciationAdjustmentReason AdjustmentReason { get; set; }
    public string AdjustmentReasonName { get; set; } = string.Empty;
    public DateOnly? AdjustmentPeriod { get; set; }
}
