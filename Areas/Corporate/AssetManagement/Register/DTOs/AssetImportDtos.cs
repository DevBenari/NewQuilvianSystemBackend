using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

public sealed class AssetImportUploadRequest
{
    public IFormFile? File { get; set; }
}

public sealed class AssetImportDecisionRequest
{
    public bool Accept { get; set; } = true;
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class AssetImportPagedQuery
{
    public string? Search { get; set; }
    public List<ImportBatchStatus> Status { get; set; } = [];
    public bool? AwaitingMyDecision { get; set; }
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetImportRowPagedQuery
{
    public bool? OnlyErrors { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public sealed class AssetImportActorResponse
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? PositionName { get; set; }
    public DateTime At { get; set; }
}

public sealed class AssetImportFileResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
}

public sealed class AssetImportBatchResponse
{
    public Guid Id { get; set; }
    public string ImportNumber { get; set; } = string.Empty;
    public DateOnly CutoverDate { get; set; }
    public ImportBatchStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int RowCount { get; set; }
    public int ErrorCount { get; set; }
    public int CreatedAssetCount { get; set; }
    public decimal? TotalAcquisitionCost { get; set; }
    public decimal? TotalAccumulatedDepreciation { get; set; }
    public decimal? TotalNetBookValue { get; set; }
    public bool AmountsVisible { get; set; }
    public AssetImportFileResponse? File { get; set; }
    public AssetImportActorResponse? Uploaded { get; set; }
    public AssetImportActorResponse? AssetHeadApproved { get; set; }
    public AssetImportActorResponse? AccountingApproved { get; set; }
    public AssetImportActorResponse? Rejected { get; set; }
    public string? DecisionReason { get; set; }
    public bool IsAwaitingMyDecision { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class AssetImportRowResponse
{
    public Guid Id { get; set; }
    public int RowNumber { get; set; }
    public string? GroupCode { get; set; }
    public string? AssetName { get; set; }
    public string? UnitCode { get; set; }
    public string? LocationCode { get; set; }
    public string? CustodianNumber { get; set; }
    public string? ReadyForUseDate { get; set; }
    public decimal? AcquisitionCost { get; set; }
    public decimal? AccumulatedDepreciation { get; set; }
    public List<string> Errors { get; set; } = [];
    public Guid? CreatedAssetId { get; set; }
    public string? InventoryNumber { get; set; }
}

public sealed class AssetImportSummaryResponse
{
    public int Total { get; set; }
    public int FailedValidation { get; set; }
    public int Validated { get; set; }
    public int AssetHeadApproved { get; set; }
    public int Applied { get; set; }
    public int Rejected { get; set; }
    public int Cancelled { get; set; }
    public int AwaitingMyDecision { get; set; }
    public DateOnly? CutoverDate { get; set; }
    public int MaxRows { get; set; }
}
