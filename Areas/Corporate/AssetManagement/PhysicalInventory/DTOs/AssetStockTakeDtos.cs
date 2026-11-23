using Microsoft.AspNetCore.Http;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.Enums;
using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.PhysicalInventory.DTOs;

public sealed class StockTakePagedQuery
{
    public string? Search { get; set; }
    public List<StockTakeStatus>? Status { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public int? Year { get; set; }
    public bool? Mine { get; set; }
    public string SortDirection { get; set; } = "desc";
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class StockTakeScheduleRequest
{
    public Guid? OrganizationUnitId { get; set; }
    public Guid? LocationId { get; set; }
    public DateOnly? PlannedStartDate { get; set; }
    public DateOnly? PlannedEndDate { get; set; }
    public string? Notes { get; set; }
    public List<StockTakeTeamMemberRequest>? TeamMembers { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class StockTakeTeamMemberRequest
{
    public Guid EmployeeId { get; set; }
    public bool IsLeader { get; set; }
}

public sealed class StockTakeVersionRequest
{
    public Guid? RowVersion { get; set; }
}

public sealed class StockTakeDecisionRequest
{
    public string? Reason { get; set; }
    public Guid? RowVersion { get; set; }
}

public sealed class StockTakeActorResponse
{
    public Guid UserId { get; set; }
    public string? Name { get; set; }
    public string? PositionName { get; set; }
    public DateTime At { get; set; }
}

public sealed class StockTakeTeamMemberResponse
{
    public Guid Id { get; set; }
    public Guid EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? EmployeeNumber { get; set; }
    public bool IsLeader { get; set; }
}

public sealed class StockTakeStartBlockersResponse
{
    public bool CanStartToday { get; set; }
    public DateOnly AvailableFrom { get; set; }
    public string? ActiveSessionNumber { get; set; }
    public List<string> TransferNumbers { get; set; } = [];
    public List<string> LoanNumbers { get; set; } = [];
    public List<string> HandoverNumbers { get; set; } = [];
    public int ProcessCount { get; set; }
}

public sealed class StockTakeSessionResponse
{
    public Guid Id { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public Guid OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public DateOnly PlannedStartDate { get; set; }
    public DateOnly PlannedEndDate { get; set; }
    public StockTakeStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public bool IsPastPlannedEnd { get; set; }
    public int ExpectedCount { get; set; }
    public int ScannedCount { get; set; }
    public string? Notes { get; set; }
    public string? DecisionReason { get; set; }
    public Guid? LeaderEmployeeId { get; set; }
    public string? LeaderName { get; set; }
    public int TeamSize { get; set; }
    public bool IsTeamMember { get; set; }
    public bool IsLeader { get; set; }
    public StockTakeActorResponse? Created { get; set; }
    public StockTakeActorResponse? Started { get; set; }
    public StockTakeActorResponse? CountingClosed { get; set; }
    public StockTakeActorResponse? Submitted { get; set; }
    public StockTakeActorResponse? Verified { get; set; }
    public StockTakeActorResponse? Cancelled { get; set; }
    public List<string> AvailableActions { get; set; } = [];
    public List<StockTakeTeamMemberResponse> TeamMembers { get; set; } = [];
    public StockTakeStartBlockersResponse? StartBlockers { get; set; }
    public int FindingCount { get; set; }
    public StockTakeResultCountsResponse? ResultCounts { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class StockTakeScanRequest
{
    public string? Code { get; set; }
    public Guid? FoundLocationId { get; set; }
    public AssetCondition? ObservedCondition { get; set; }
    public bool IsManualEntry { get; set; }
    public string? Notes { get; set; }
    public IFormFile? Photo { get; set; }
}

public sealed class StockTakeUnregisteredFindingRequest
{
    public string? Code { get; set; }
    public string? Description { get; set; }
    public Guid? FoundLocationId { get; set; }
    public AssetCondition? ObservedCondition { get; set; }
    public string? Notes { get; set; }
    public IFormFile? Photo { get; set; }
}

public sealed class StockTakeItemQuery
{
    public string? Search { get; set; }
    public bool? Scanned { get; set; }
    public List<StockTakeResultCategory>? Category { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class StockTakeLookupResponse
{
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public AssetCondition CurrentCondition { get; set; }
    public string CurrentConditionName { get; set; } = string.Empty;
    public Guid? CurrentOrganizationUnitId { get; set; }
    public string? CurrentOrganizationUnitName { get; set; }
    public bool IsExpected { get; set; }
    public Guid? ExpectedLocationId { get; set; }
    public string? ExpectedLocationName { get; set; }
    public AssetCondition? ExpectedCondition { get; set; }
    public string? ExpectedConditionName { get; set; }
    public Guid? LoanBorrowerOrganizationUnitId { get; set; }
    public string? LoanBorrowerOrganizationUnitName { get; set; }
    public StockTakeScanResponse? LastScan { get; set; }
}

public sealed class StockTakeScanResponse
{
    public Guid Id { get; set; }
    public Guid? AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string? AssetName { get; set; }
    public string? ScannedCode { get; set; }
    public bool IsManualEntry { get; set; }
    public bool IsUnregisteredFinding { get; set; }
    public string? FindingDescription { get; set; }
    public Guid FoundLocationId { get; set; }
    public string? FoundLocationName { get; set; }
    public AssetCondition ObservedCondition { get; set; }
    public string ObservedConditionName { get; set; } = string.Empty;
    public Guid? PhotoAttachmentId { get; set; }
    public string? Notes { get; set; }
    public Guid ScannedByEmployeeId { get; set; }
    public string? ScannedByName { get; set; }
    public DateTime ScannedAt { get; set; }
    public bool IsLatest { get; set; }
    public bool IsExpected { get; set; }
    public int ScannedCount { get; set; }
    public int ExpectedCount { get; set; }
}

public sealed class StockTakeExpectedItemResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? GroupName { get; set; }
    public Guid ExpectedLocationId { get; set; }
    public string? ExpectedLocationName { get; set; }
    public string? ExpectedCustodianName { get; set; }
    public AssetCondition ExpectedCondition { get; set; }
    public string ExpectedConditionName { get; set; } = string.Empty;
    public Guid? LoanBorrowerOrganizationUnitId { get; set; }
    public string? LoanBorrowerOrganizationUnitName { get; set; }
    public bool IsScanned { get; set; }
    public string? FoundLocationName { get; set; }
    public AssetCondition? ObservedCondition { get; set; }
    public string? ObservedConditionName { get; set; }
    public DateTime? ScannedAt { get; set; }
}

public sealed class StockTakeResultResponse
{
    public Guid Id { get; set; }
    public StockTakeResultCategory Category { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public Guid? AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string? AssetName { get; set; }
    public string? GroupName { get; set; }
    public bool IsLocationMismatch { get; set; }
    public bool IsConditionMismatch { get; set; }
    public bool IsForeignAsset { get; set; }
    public string? OwnerOrganizationUnitName { get; set; }
    public Guid? ExpectedLocationId { get; set; }
    public string? ExpectedLocationName { get; set; }
    public Guid? FoundLocationId { get; set; }
    public string? FoundLocationName { get; set; }
    public AssetCondition? ExpectedCondition { get; set; }
    public string? ExpectedConditionName { get; set; }
    public AssetCondition? ObservedCondition { get; set; }
    public string? ObservedConditionName { get; set; }
    public Guid? ScanId { get; set; }
    public string? ScannedCode { get; set; }
    public string? FindingDescription { get; set; }
    public Guid? PhotoAttachmentId { get; set; }
    public string? ScannedByName { get; set; }
    public DateTime? ScannedAt { get; set; }
    public string? TeamNote { get; set; }
}

public sealed class StockTakeResultCountsResponse
{
    public int Total { get; set; }
    public int Sesuai { get; set; }
    public int SesuaiSedangDipinjam { get; set; }
    public int TidakDitemukan { get; set; }
    public int BedaLokasi { get; set; }
    public int BedaKondisi { get; set; }
    public int TidakTerdaftar { get; set; }
    public int Discrepancies { get; set; }
}

public sealed class StockTakeSessionSummaryResponse
{
    public int Total { get; set; }
    public int Scheduled { get; set; }
    public int Running { get; set; }
    public int Matching { get; set; }
    public int AwaitingVerification { get; set; }
    public int Verified { get; set; }
    public int Cancelled { get; set; }
    public int Mine { get; set; }
}
