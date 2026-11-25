using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

public sealed class AssetConditionReportCreateRequest
{
    public Guid? AssetId { get; set; }
    public AssetCondition? ReportedCondition { get; set; }
    public ConditionReportSource? Source { get; set; }
    public bool IsFoundAgain { get; set; }
    public string? Notes { get; set; }
    public IFormFile? Photo { get; set; }
    public IFormFile? LossReport { get; set; }
}

public sealed class AssetConditionDecideRequest
{
    public bool Accept { get; set; } = true;
    public AssetCondition? DecidedCondition { get; set; }
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetConditionApproveLossRequest
{
    public AssetCondition? DecidedCondition { get; set; }
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
    public IFormFile? LossReport { get; set; }
}

public sealed class AssetConditionPagedQuery
{
    public string? Search { get; set; }
    public ConditionReportStatus? Status { get; set; }
    public AssetCondition? ReportedCondition { get; set; }
    public ConditionReportSource? Source { get; set; }
    public bool? IsFoundAgain { get; set; }
    public bool? AwaitingLossApproval { get; set; }
    public Guid? AssetId { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed record AssetConditionOpenRequest(
    Guid AssetId,
    ConditionReportSource Source,
    AssetCondition ReportedCondition,
    Guid? SourceDocumentId,
    string? Notes,
    Guid ActorUserId);

public sealed class AssetConditionActorResponse
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? PositionName { get; set; }
    public DateTime At { get; set; }
}

public sealed class AssetConditionAttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

public sealed class AssetConditionReportResponse
{
    public Guid Id { get; set; }
    public string ReportNumber { get; set; } = string.Empty;
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public string? LocationName { get; set; }
    public Guid? CustodianEmployeeId { get; set; }
    public string? CustodianEmployeeName { get; set; }
    public AssetCondition CurrentCondition { get; set; }
    public string CurrentConditionName { get; set; } = string.Empty;
    public ConditionReportSource Source { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public Guid? SourceDocumentId { get; set; }
    public AssetCondition PreviousCondition { get; set; }
    public string PreviousConditionName { get; set; } = string.Empty;
    public AssetCondition ReportedCondition { get; set; }
    public string ReportedConditionName { get; set; } = string.Empty;
    public bool IsFoundAgain { get; set; }
    public string? Notes { get; set; }
    public AssetConditionAttachmentResponse? Photo { get; set; }
    public AssetConditionAttachmentResponse? LossReport { get; set; }
    public ConditionReportStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public AssetConditionActorResponse? Reported { get; set; }
    public AssetConditionActorResponse? Decided { get; set; }
    public AssetCondition? DecidedCondition { get; set; }
    public string? DecidedConditionName { get; set; }
    public string? DecisionReason { get; set; }
    public bool RequiresLossApproval { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public Guid RowVersion { get; set; }
}

public sealed class AssetConditionSummaryResponse
{
    public int Total { get; set; }
    public int Reported { get; set; }
    public int Decided { get; set; }
    public int Rejected { get; set; }
    public int AwaitingDecision { get; set; }
    public int AwaitingLossApproval { get; set; }
}
