using QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.Enums;

namespace QuilvianSystemBackend.Areas.Corporate.AssetManagement.Register.DTOs;

public sealed class AssetDashboardQuery
{
    public Guid? OrganizationUnitId { get; set; }
}

public sealed class AssetDashboardSummaryResponse
{
    public Guid? OrganizationUnitId { get; set; }
    public string? OrganizationUnitName { get; set; }
    public bool CanReadAllUnits { get; set; }
    public List<AssetDashboardCardResponse> Cards { get; set; } = [];
    public List<AssetDashboardCardResponse> Conditions { get; set; } = [];
    public int DamagedTotal { get; set; }
    public List<AssetDamagedItemResponse> RecentDamaged { get; set; } = [];
    public DateTime GeneratedAt { get; set; }
}

public sealed class AssetDashboardCardResponse
{
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public Dictionary<string, string> Filter { get; set; } = new(StringComparer.Ordinal);
}

public sealed class AssetDamagedItemResponse
{
    public Guid Id { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? CurrentOrganizationUnitName { get; set; }
    public string? CurrentLocationName { get; set; }
    public AssetCondition Condition { get; set; }
    public string ConditionName { get; set; } = string.Empty;
    public DateTime ConditionSince { get; set; }
}

public sealed class AssetUnitSummaryResponse
{
    public Guid OrganizationUnitId { get; set; }
    public string OrganizationUnitName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Active { get; set; }
    public int LightlyDamaged { get; set; }
    public int HeavilyDamaged { get; set; }
    public int Unused { get; set; }
    public int Lost { get; set; }
    public Dictionary<string, string> Filter { get; set; } = new(StringComparer.Ordinal);
}

public sealed class AssetReportQuery
{
    public string? Search { get; set; }
    public Guid? OrganizationUnitId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid? GroupId { get; set; }
    public AssetCondition? Condition { get; set; }
    public AssetPlacementState? PlacementState { get; set; }
    public Guid? AssetId { get; set; }
    public AssetHistoryEventType? EventType { get; set; }
    public string? SubModule { get; set; }
    public Guid? ActorUserId { get; set; }
    public DateOnly? OccurredFrom { get; set; }
    public DateOnly? OccurredTo { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

public sealed class AssetHistoryReportItemResponse
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string? InventoryNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public string? CurrentOrganizationUnitName { get; set; }
    public AssetHistoryEventType EventType { get; set; }
    public string EventTypeName { get; set; } = string.Empty;
    public string SubModule { get; set; } = string.Empty;
    public string SubModuleName { get; set; } = string.Empty;
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

public sealed class AssetReportDefinitionResponse
{
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public List<string> QueryParameters { get; set; } = [];
    public List<AssetReportOptionResponse> SortOptions { get; set; } = [];
}

public sealed class AssetReportMetadataResponse
{
    public List<AssetReportDefinitionResponse> Reports { get; set; } = [];
    public List<AssetReportOptionResponse> EventTypes { get; set; } = [];
    public List<AssetReportOptionResponse> SubModules { get; set; } = [];
    public List<AssetReportOptionResponse> Conditions { get; set; } = [];
    public List<AssetReportOptionResponse> PlacementStates { get; set; } = [];
    public int MaxExportRows { get; set; }
}

public sealed class AssetReportOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}
