using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

public sealed class AssetUtilizationUpsertRequest
{
    public UtilizationType? UtilizationType { get; set; }
    public string? PartnerName { get; set; }
    public string? AgreementNumber { get; set; }
    public decimal? AgreementValue { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Purpose { get; set; }
    public List<Guid> AssetIds { get; set; } = [];
    public Guid? RowVersion { get; set; }
}

public sealed class AssetUtilizationDecisionRequest
{
    public bool Accept { get; set; } = true;
    public bool Forward { get; set; }
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetUtilizationAgreementRequest
{
    public IFormFile? File { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetUtilizationPagedQuery
{
    public string? Search { get; set; }
    public List<UtilizationStatus> Status { get; set; } = [];
    public UtilizationType? UtilizationType { get; set; }
    public UtilizationApprovalLevel? RequiredApprovalLevel { get; set; }
    public Guid? AssetId { get; set; }
    public bool? AwaitingMyDecision { get; set; }
    public DateOnly? ActiveOn { get; set; }
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetUtilizationActorResponse
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? PositionName { get; set; }
    public DateTime At { get; set; }
}

public sealed class AssetUtilizationItemResponse
{
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public string? OwnershipTypeName { get; set; }
    public bool IsOwnedByHospital { get; set; }
    public AssetRecordStatus RecordStatus { get; set; }
    public string RecordStatusName { get; set; } = string.Empty;
    public AssetCondition Condition { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public AssetActiveProcessKind ActiveProcessKind { get; set; }
    public Guid? ActiveProcessId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public string? LocationName { get; set; }
}

public sealed class AssetUtilizationAttachmentResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

public sealed class AssetUtilizationResponse
{
    public Guid Id { get; set; }
    public string UtilizationNumber { get; set; } = string.Empty;
    public UtilizationType UtilizationType { get; set; }
    public string UtilizationTypeName { get; set; } = string.Empty;
    public string PartnerName { get; set; } = string.Empty;
    public string AgreementNumber { get; set; } = string.Empty;
    public decimal AgreementValue { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int DurationMonths { get; set; }
    public string? Purpose { get; set; }
    public UtilizationStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public UtilizationApprovalLevel? RequiredApprovalLevel { get; set; }
    public string? RequiredApprovalLevelName { get; set; }
    public decimal? ThresholdUsed { get; set; }
    public AssetUtilizationAttachmentResponse? Agreement { get; set; }
    public AssetUtilizationActorResponse? Created { get; set; }
    public AssetUtilizationActorResponse? Submitted { get; set; }
    public AssetUtilizationActorResponse? AssetHeadDecided { get; set; }
    public AssetUtilizationActorResponse? DirectorDecided { get; set; }
    public AssetUtilizationActorResponse? Ended { get; set; }
    public string? DecisionReason { get; set; }
    public int AssetCount { get; set; }
    public List<AssetUtilizationItemResponse> Items { get; set; } = [];
    public bool IsAwaitingMyDecision { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class AssetUtilizationSummaryResponse
{
    public int Total { get; set; }
    public int Draft { get; set; }
    public int Submitted { get; set; }
    public int AwaitingDirector { get; set; }
    public int Approved { get; set; }
    public int Running { get; set; }
    public int Ended { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int AwaitingMyDecision { get; set; }
}
