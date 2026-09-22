using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class ReceivableQuery
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? DebtorType { get; set; }
    public DateOnly? DueDateFrom { get; set; }
    public DateOnly? DueDateTo { get; set; }
    public string SortBy { get; set; } = "dueDate";
    public string SortDirection { get; set; } = "asc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class ReceivableResponse
{
    public Guid Id { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string DebtorType { get; set; } = string.Empty;
    public Guid? DebtorReferenceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal AdjustedAmount { get; set; }
    public decimal WrittenOffAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ClaimStatus { get; set; } = string.Empty;
    public DateTimeOffset RecognizedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class ReceivableItemResponse
{
    public Guid Id { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid? InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public sealed class ReceivableDocumentResponse
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public bool IsReceived { get; set; }
    public DateTimeOffset? ReceivedAt { get; set; }
}

public sealed class ReceivableAdjustmentResponse
{
    public Guid Id { get; set; }
    public string AdjustmentNumber { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class ReceivableWriteOffResponse
{
    public Guid Id { get; set; }
    public string WriteOffNumber { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class ReceivableDetailResponse
{
    public ReceivableResponse Receivable { get; set; } = new();
    public List<ReceivableItemResponse> Items { get; set; } = new();
    public List<ReceivableDocumentResponse> Documents { get; set; } = new();
    public List<ReceivableAdjustmentResponse> Adjustments { get; set; } = new();
    public List<ReceivableWriteOffResponse> WriteOffs { get; set; } = new();
}

public sealed class ReceivableSummaryResponse
{
    public int TotalReceivable { get; set; }
    public int OutstandingCount { get; set; }
    public int PartialCount { get; set; }
    public int SettledCount { get; set; }
    public int WrittenOffCount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}

public sealed class ReceivableFilterMetadataResponse
{
    public List<int> PageSizeOptions { get; set; } = new();
    public List<string> SortableFields { get; set; } = new();
    public List<string> StatusOptions { get; set; } = new();
}

public sealed class ReceivableAgingQuery
{
    public DateOnly? AsOfDate { get; set; }
}

public sealed class RequestReceivableAdjustmentRequest
{
    [Required] public string Direction { get; set; } = string.Empty;
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}

public sealed class DecideReceivableRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string? RejectionReason { get; set; }
}

public sealed class RequestReceivableWriteOffRequest
{
    [Range(0.01, double.MaxValue)] public decimal Amount { get; set; }
    [Required, MaxLength(500)] public string Reason { get; set; } = string.Empty;
}
