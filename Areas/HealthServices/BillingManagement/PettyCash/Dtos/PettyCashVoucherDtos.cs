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

/// <summary>BE-BKC-057, PC-DES-019. Total seluruh pengembalian pada satu voucher MUST NOT
/// melampaui Amount voucher (BIL-VAL-098) — ditegakkan di service, bukan data annotation,
/// karena batasnya bergantung pada ReturnedAmount voucher saat ini.</summary>
public sealed class PettyCashVoucherReturnRequest
{
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid ExpectedRowVersion { get; set; }
}

/// <summary>BE-BKC-057, PC-DES-020. Tidak ada field Amount — pembalikan selalu sebesar
/// Amount voucher dikurangi ReturnedAmount yang sudah kembali lebih dulu.</summary>
public sealed class PettyCashVoucherReversalRequest
{
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
    public Guid ExpectedRowVersion { get; set; }
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

    /// <summary>BE-BKC-057. Akumulasi sisa yang sudah dikembalikan penerima. 0 bila belum ada.</summary>
    public decimal ReturnedAmount { get; set; }

    /// <summary>BE-BKC-057. Amount − ReturnedAmount — nilai yang benar-benar masih di tangan penerima.</summary>
    public decimal OutstandingAmount { get; set; }

    /// <summary>BE-BKC-057. Terisi hanya pada status REVERSED.</summary>
    public DateTimeOffset? ReversedAt { get; set; }
    public string? ReversedByName { get; set; }
    public string? ReversalReason { get; set; }

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
    /// <summary>Menggantikan WaitingApprovalCount + ApprovedCount (PC-DES-015) — kini satu
    /// hitungan "belum dicairkan" karena gerbang persetujuan dicabut (PC-DEC-016).</summary>
    public int PendingDisbursementCount { get; set; }
    public int CashReceivedCount { get; set; }
    public int CompletedCount { get; set; }

    /// <summary>Hanya baris warisan sebelum 15 September 2026 (PC-DEC-016).</summary>
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }

    /// <summary>Menggantikan TotalWaitingApprovalAmount.</summary>
    public decimal TotalPendingDisbursementAmount { get; set; }
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
