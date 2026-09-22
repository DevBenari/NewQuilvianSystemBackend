using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;

public sealed class BillingIntakeQuery
{
    public string? Status { get; set; }
    public string? HandoffType { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class BillingIntakeResponse
{
    public Guid Id { get; set; }
    public string HandoffType { get; set; } = string.Empty;
    public Guid SourceHandoffId { get; set; }
    public Guid SourceHandoffKey { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? TargetEntityId { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
    public DateTimeOffset? AcknowledgedAt { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
}

public sealed class BillingIntakeSummaryResponse
{
    public int TotalIntake { get; set; }
    public int NewCount { get; set; }
    public int ErrorCount { get; set; }
    public int ConsumedCount { get; set; }
    public int AcknowledgedCount { get; set; }
}

public sealed class BillingIntakeFilterMetadataResponse
{
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
}

public sealed class SyncBillingIntakeResponse
{
    public int DiscoveredCount { get; set; }
}
