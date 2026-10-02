namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Dtos;

public sealed class PaymentAllocationResponse
{
    public Guid Id { get; set; }
    public string PayableType { get; set; } = string.Empty;
    public Guid? SupplierPayableId { get; set; }
    public Guid? MedicalServicePayableId { get; set; }
    public decimal Amount { get; set; }
    public bool IsReversal { get; set; }
    public Guid? ReversalOfAllocationId { get; set; }
}

public sealed class PaymentDeductionResponse
{
    public Guid Id { get; set; }
    public string DeductionType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class PaymentResponse
{
    public Guid Id { get; set; }
    public string PaymentNumber { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public Guid PayeeReferenceId { get; set; }
    public Guid BankAccountId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal DeductionAmount { get; set; }
    public decimal AdditionAmount { get; set; }
    public decimal NetTransferAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ApprovalTier { get; set; }
    public Guid RequestedBy { get; set; }
    public DateTimeOffset RequestedAt { get; set; }
    public Guid? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public DateTimeOffset? PaidAt { get; set; }
    public string? ReferenceNumber { get; set; }
    public string? RejectionReason { get; set; }
    public string? Notes { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class PaymentDetailResponse : PaymentResponse
{
    public List<PaymentAllocationResponse> Allocations { get; set; } = [];
    public List<PaymentDeductionResponse> Deductions { get; set; } = [];
}

public sealed class PaymentAllocationRequestDto
{
    public string PayableType { get; set; } = string.Empty;
    public Guid PayableId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class PaymentDeductionRequestDto
{
    public string DeductionType { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
}

public sealed class CreatePaymentRequest
{
    public string PaymentType { get; set; } = string.Empty;
    public Guid PayeeReferenceId { get; set; }
    public Guid BankAccountId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<PaymentAllocationRequestDto> Allocations { get; set; } = [];
    public List<PaymentDeductionRequestDto>? Deductions { get; set; }
}

public sealed class UpdatePaymentRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public Guid BankAccountId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public List<PaymentAllocationRequestDto> Allocations { get; set; } = [];
    public List<PaymentDeductionRequestDto>? Deductions { get; set; }
}

public sealed class PaymentRowVersionRequest
{
    public Guid ExpectedRowVersion { get; set; }
}

public sealed class RejectPaymentRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

public sealed class MarkPaymentPaidRequest
{
    public Guid ExpectedRowVersion { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
}
