using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;

public sealed class PettyCashVoucherQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? CustomPeriod { get; set; }
    public string SortBy { get; set; } = "submittedAt";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class CreatePettyCashVoucherRequest
{
    [Required, MaxLength(150)] public string RecipientName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Purpose { get; set; } = string.Empty;
}

public sealed class ApprovePettyCashVoucherRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class RejectPettyCashVoucherRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string RejectionReason { get; set; } = string.Empty;
}

public sealed class CancelPettyCashVoucherRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class DisbursePettyCashVoucherRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class AttachPettyCashProofRequest
{
    public Guid ExpectedRowVersion { get; set; }
    [MaxLength(60)] public string ProofReferenceNumber { get; set; } = string.Empty;
}

public sealed class PettyCashVoucherResponse
{
    public Guid Id { get; set; }
    public string VoucherNumber { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusLabel { get; set; } = string.Empty;
    public bool IsCancelled { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? DecidedAt { get; set; }
    public DateTimeOffset? DisbursedAt { get; set; }
    public DateTimeOffset? ProofSubmittedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public Guid? DecidedBy { get; set; }
    public string? DecidedByName { get; set; }
    public Guid? DisbursedBy { get; set; }
    public string? DisbursedByName { get; set; }
    public string? RejectionReason { get; set; }
    public string? ProofReferenceNumber { get; set; }
    public List<string> AvailableActions { get; set; } = new();
    public Guid RowVersion { get; set; }
    public bool IsReplay { get; set; }
}

public sealed class PettyCashVoucherCommandResponse
{
    public Guid Id { get; set; }
    public string CommandType { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string? ActorName { get; set; }
    public string ActorRole { get; set; } = string.Empty;
    public string? StatusBefore { get; set; }
    public string StatusAfter { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class PettyCashVoucherDetailResponse
{
    public PettyCashVoucherResponse Voucher { get; set; } = new();
    public List<PettyCashVoucherCommandResponse> Commands { get; set; } = new();
}

public sealed class PettyCashVoucherSummaryResponse
{
    public int WaitingApprovalCount { get; set; }
    public int ApprovedCount { get; set; }
    public int CashReceivedCount { get; set; }
    public int CompletedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
    public decimal TotalWaitingApprovalAmount { get; set; }
}

public sealed class PettyCashVoucherDefaultFilterResponse
{
    public string? Status { get; set; }
    public Guid? CategoryId { get; set; }
    public string? Search { get; set; }
    public string SortBy { get; set; } = "submittedAt";
    public string SortDirection { get; set; } = "desc";
}

public sealed class PettyCashVoucherStatusOptionResponse
{
    public string Value { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
}

public sealed class PettyCashVoucherFilterMetadataResponse
{
    public PettyCashVoucherDefaultFilterResponse DefaultFilter { get; set; } = new();
    public List<int> PageSizeOptions { get; set; } = new();
    public List<PettyCashVoucherStatusOptionResponse> StatusOptions { get; set; } = new();
}
