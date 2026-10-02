using QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.Enums;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuilvianSystemBackend.Areas.Corporate.AccountingManagement.AccountingEvent.DTOs
{
    public class ReceiveAccountingEventRequest
    {
        public string? EventNumber { get; set; }

        public string? EventTypeCode { get; set; }

        public string? SourceModule { get; set; }

        public string? SourceTransactionId { get; set; }

        public string? SourceVersion { get; set; }

        public DateTimeOffset? EventOccurredAt { get; set; }

        public DateTime? AccountingDate { get; set; }

        public decimal? Amount { get; set; }

        public string? CurrencyCode { get; set; }

        public Guid? LegalEntityId { get; set; }

        public Guid? CorrelationId { get; set; }

        public Guid? CausationId { get; set; }

        public List<AccountingEventComponentRequest>? Components { get; set; }

        public AccountingEventSubledgerBalanceRequest? SubledgerBalance { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? AdditionalFields { get; set; }
    }

    public class AccountingEventComponentRequest
    {
        public string? ComponentCode { get; set; }

        public decimal? Amount { get; set; }
    }

    public class AccountingEventSubledgerBalanceRequest
    {
        public string? AccountingPeriodCode { get; set; }

        public string? ControlAccountCode { get; set; }
    }

    public class AccountingEventPagedQuery
    {
        public int PageNumber { get; set; } = 1;

        public int PageSize { get; set; } = 25;

        public Guid? LegalEntityId { get; set; }

        public AccountingEventStatus? EventStatus { get; set; }

        public string? EventTypeCode { get; set; }

        public string? PeriodCode { get; set; }

        public string? Search { get; set; }

        public string? SortBy { get; set; }

        public string? SortDirection { get; set; }
    }

    public class AccountingEventListDto
    {
        public Guid Id { get; set; }

        public Guid LegalEntityId { get; set; }

        public string EventNumber { get; set; } = string.Empty;

        public string EventTypeCode { get; set; } = string.Empty;

        public string? EventTypeName { get; set; }

        public string SourceModule { get; set; } = string.Empty;

        public DateTime AccountingDate { get; set; }

        public decimal Amount { get; set; }

        public string CurrencyCode { get; set; } = string.Empty;

        public AccountingEventStatus EventStatus { get; set; }

        public string? HoldReasonCode { get; set; }

        public Guid? JournalId { get; set; }

        public string? JournalNumber { get; set; }

        public int AttemptCount { get; set; }

        public DateTime ReceivedAt { get; set; }
    }

    public class AccountingEventDetailDto : AccountingEventListDto
    {
        public string SourceTransactionId { get; set; } = string.Empty;

        public string SourceVersion { get; set; } = string.Empty;

        public DateTimeOffset EventOccurredAt { get; set; }

        public DateTime DocumentDate { get; set; }

        public Guid CorrelationId { get; set; }

        public Guid CausationId { get; set; }

        public string? IgnoreReason { get; set; }

        public string? JournalStatus { get; set; }

        public string? JournalPeriodCode { get; set; }

        public string RawPayload { get; set; } = string.Empty;

        public List<AccountingEventComponentDto> Components { get; set; } = new();

        public List<AccountingEventAttemptDto> Attempts { get; set; } = new();
    }

    public class AccountingEventComponentDto
    {
        public string ComponentCode { get; set; } = string.Empty;

        public decimal Amount { get; set; }
    }

    public class AccountingEventAttemptDto
    {
        public int AttemptNumber { get; set; }

        public DateTimeOffset AttemptedAt { get; set; }

        public bool IsSuccess { get; set; }

        public string? FailureMessage { get; set; }
    }

    public class AccountingEventSummaryDto
    {
        public Guid? LegalEntityId { get; set; }

        public int Total { get; set; }

        public int Diterima { get; set; }

        public int Tertahan { get; set; }

        public int Gagal { get; set; }

        public int Terjurnal { get; set; }

        public int Diabaikan { get; set; }

        public int Tercatat { get; set; }
    }

    public class IgnoreAccountingEventRequest
    {
        public string? Reason { get; set; }
    }

    public class AccountingEventReceiptDto
    {
        public Guid AccountingEventId { get; set; }

        public string EventNumber { get; set; } = string.Empty;

        public string EventStatus { get; set; } = string.Empty;

        public string? JournalNumber { get; set; }

        public string? AccountingPeriodCode { get; set; }

        public string? HoldReasonCode { get; set; }

        public DateTimeOffset ReceivedAt { get; set; }
    }

    public class AccountingEventRetryCycleResult
    {
        public int Considered { get; set; }

        public int Journaled { get; set; }

        public int Held { get; set; }

        public int StillPending { get; set; }

        public int MarkedFailed { get; set; }

        public int Skipped { get; set; }

        public List<string> Errors { get; set; } = new();
    }
}
