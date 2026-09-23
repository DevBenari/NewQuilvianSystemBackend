using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;

public sealed class AccountingEventQuery
{
    public string? DeliveryStatus { get; set; }
    public string? EventTypeCode { get; set; }

    /// <summary>Dicocokkan ke EventNumber atau SourceTransactionId (contains, case-insensitive).</summary>
    public string? Search { get; set; }

    public string SortBy { get; set; } = "eventOccurredAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

/// <summary>Baris daftar/antrean — sengaja TANPA PayloadJson/ComponentsJson (lihat detail).</summary>
public class AccountingEventResponse
{
    public Guid Id { get; set; }
    public string EventNumber { get; set; } = string.Empty;
    public string EventTypeCode { get; set; } = string.Empty;
    public string SourceModule { get; set; } = string.Empty;
    public string SourceTransactionId { get; set; } = string.Empty;
    public string SourceVersion { get; set; } = string.Empty;
    public DateTimeOffset EventOccurredAt { get; set; }
    public DateOnly AccountingDate { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public string DeliveryStatus { get; set; } = string.Empty;
    public string? HoldReason { get; set; }
    public int AttemptCount { get; set; }
    public DateTimeOffset? LastAttemptAt { get; set; }
    public int? LastResponseCode { get; set; }
    public string? AccountingReceiptNumber { get; set; }
    public string? AccountingJournalNumber { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid RowVersion { get; set; }
    public DateTime CreateDateTime { get; set; }
}

/// <summary>Detail satu kejadian, termasuk muatan yang sudah dijamin bebas data pasien
/// (BE-FIN-011 bagian 1.2) dan riwayat percobaan kirim.</summary>
public sealed class AccountingEventDetailResponse : AccountingEventResponse
{
    public Guid LegalEntityId { get; set; }
    public Guid CausationId { get; set; }
    public string? ComponentsJson { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public List<AccountingEventAttemptResponse> Attempts { get; set; } = new();
}

public sealed class AccountingEventAttemptResponse
{
    public Guid Id { get; set; }
    public int AttemptNumber { get; set; }
    public DateTimeOffset AttemptedAt { get; set; }
    public int? ResponseCode { get; set; }
    public string? ResponseBody { get; set; }
    public int? DurationMs { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>Frontend MUST membedakan HeldForFinalization dari Held dan Failed — tindakan
/// penggunanya berbeda (03-frontend-architecture.md §3.5).</summary>
public sealed class AccountingEventSummaryResponse
{
    public int TotalEvents { get; set; }
    public int PendingCount { get; set; }
    public int HeldForFinalizationCount { get; set; }
    public int SentCount { get; set; }
    public int AcknowledgedCount { get; set; }
    public int HeldCount { get; set; }
    public int FailedCount { get; set; }
}

public sealed class AccountingEventFilterMetadataResponse
{
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
    public List<string> DeliveryStatusOptions { get; set; } = new();
    public List<string> EventTypeCodeOptions { get; set; } = new();
}
