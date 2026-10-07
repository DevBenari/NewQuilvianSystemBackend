using System.ComponentModel.DataAnnotations;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;

public sealed class OutboxMonitorQuery
{
    public string? Status { get; set; }
    public string? EventType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class OutboxMonitorResponse
{
    public Dictionary<string, int> CountsByStatus { get; set; } = [];
    public PagedResult<OutboxMonitorItem> Messages { get; set; } = new();
}

public sealed class OutboxMonitorItem
{
    public Guid Id { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string IdempotencyKey { get; set; } = string.Empty;
    public int RetryCount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ProcessingStartedAtUtc { get; set; }
    public DateTime? NextRetryAtUtc { get; set; }
    public Guid? AcknowledgedReceiptId { get; set; }
    public Guid? ReplayBatchId { get; set; }
    public bool HasError { get; set; }
}

public sealed class ReplayRequest
{
    public bool DryRun { get; set; } = true;
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class ReplayResultResponse
{
    public Guid ReplayBatchId { get; set; }
    public bool DryRun { get; set; }
    public int EpisodeCount { get; set; }
    public List<ReplayEpisodeItem> Items { get; set; } = [];
}

public sealed class ReplayEpisodeItem
{
    public Guid EpisodeId { get; set; }
    public string EpisodeNumber { get; set; } = string.Empty;
    public List<string> EventsQueued { get; set; } = [];
    public bool AlreadyPublishedRequeued { get; set; }
}
