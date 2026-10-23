using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.PatientRefund.Dtos;

public sealed class CreatePatientRefundRequestDto
{
    public Guid RefundableCreditId { get; set; }

    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal RequestedAmount { get; set; }

    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? PaymentMethod { get; set; } = "TRANSFER";

    public Guid? BankAccountId { get; set; }

    [MaxLength(100)]
    public string? PayeeBankName { get; set; }

    [MaxLength(50)]
    public string? PayeeAccountNumber { get; set; }

    [MaxLength(150)]
    public string? PayeeAccountName { get; set; }

    public Guid CorrelationId { get; set; }
    public Guid CausationId { get; set; }
}

public sealed class ApprovePatientRefundRequestDto
{
    public decimal? ApprovedAmount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public Guid ExpectedRowVersion { get; set; }
}

public sealed class RejectPatientRefundRequestDto
{
    [Required, MaxLength(500)]
    public string RejectionReason { get; set; } = string.Empty;

    public Guid ExpectedRowVersion { get; set; }
}

public sealed class ExecutePatientRefundRequestDto
{
    [Range(
        typeof(decimal),
        "0.01",
        "9999999999999999.99",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal PaidAmount { get; set; }

    [MaxLength(30)]
    public string? PaymentMethod { get; set; }

    public Guid? BankAccountId { get; set; }

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public Guid ExpectedRowVersion { get; set; }
}

public sealed class PatientRefundItemDto
{
    public Guid Id { get; set; }
    public Guid? RefundRequestId { get; set; }
    public Guid RefundableCreditId { get; set; }
    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public DateTimeOffset RecognizedAt { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? PaymentMethod { get; set; }
    public string? ReferenceNumber { get; set; }
    public Guid? RequestedBy { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PatientRefundSummaryDto
{
    public decimal TotalAvailableRefundAmount { get; set; }
    public decimal TotalRequestedRefundAmount { get; set; }
    public decimal TotalPendingApprovalAmount { get; set; }
    public decimal TotalApprovedWaitingPaymentAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public int PendingCount { get; set; }
    public int ApprovedCount { get; set; }
    public int PaidCount { get; set; }
    public int AvailableCreditCount { get; set; }
}

public sealed class PatientRefundDetailDto
{
    public Guid Id { get; set; }
    public Guid? RefundRequestId { get; set; }
    public Guid RefundableCreditId { get; set; }
    public Guid PatientId { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public Guid InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ServiceType { get; set; } = string.Empty;
    public string? DepositAccountNumber { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid SourceId { get; set; }
    public DateTimeOffset RecognizedAt { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AvailableAmount { get; set; }
    public decimal RequestedAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string? PaymentMethod { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankAccountName { get; set; }
    public string? PayeeBankName { get; set; }
    public string? PayeeAccountNumber { get; set; }
    public string? PayeeAccountName { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? ExecutionNotes { get; set; }
    public string? RejectionReason { get; set; }
    public string? FailedReason { get; set; }
    public Guid? RequestedBy { get; set; }
    public string? RequestedByName { get; set; }
    public Guid? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTimeOffset? SubmittedAt { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PatientRefundFilterDto
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? SourceType { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
}

