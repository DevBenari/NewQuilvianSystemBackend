using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

public sealed class ReceivableInvoiceBatchQuery
{
    public string? Search { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? VisitCode { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public Guid? CompanyId { get; set; }
    public string? Status { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 25;
}

public sealed class EligibleReceivableQuery
{
    [Required] public Guid DebtorReferenceId { get; set; }
}

public sealed class EligibleReceivableResponse
{
    public Guid Id { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    // BE-FIN-079: nullable — kosong untuk item migrasi tagihan lama.
    public Guid? InvoiceId { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public DateOnly DueDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public static class ReceivableDueDateSources
{
    public const string CompanyGuarantorTerm = "COMPANY_GUARANTOR_TERM";
    public const string InsuranceProviderTerm = "INSURANCE_PROVIDER_TERM";
    public const string ReceivableDueDate = "RECEIVABLE_DUE_DATE";
    public const string NotConfigured = "NOT_CONFIGURED";
    public const string SystemDefault30Days = "SYSTEM_DEFAULT_30_DAYS";
}

public sealed class ReceivableInvoiceBatchCreateContextQuery
{
    [Required] public Guid DebtorReferenceId { get; set; }
    public string? Category { get; set; }
}

public sealed class ReceivableInvoiceBatchCreateContextResponse
{
    public Guid DebtorReferenceId { get; set; }
    public Guid CompanyId => DebtorReferenceId;
    public string PayerName { get; set; } = string.Empty;
    public string? CompanyName => PayerName;
    public string PayerKind { get; set; } = string.Empty;
    public DateOnly DefaultInvoiceDate { get; set; }
    public int? PaymentTermDays { get; set; }
    public DateOnly? DueDatePreview { get; set; }
    public string DueDateSource { get; set; } = string.Empty;
    public bool IsTermConfigured { get; set; }
}

public sealed class CreateReceivableInvoiceBatchRequest
{
    [Required] public DateOnly PeriodStart { get; set; }
    [Required] public DateOnly PeriodEnd { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
    [Required, MinLength(1)] public List<Guid> ReceivableIds { get; set; } = new();
}

public sealed class ReceivableInvoiceBatchRowVersionRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
}

public class ReceivableInvoiceBatchResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string InvoiceNumber => BatchNumber;
    public string InvoiceNo => BatchNumber;
    public string DebtorType { get; set; } = string.Empty;
    public Guid DebtorReferenceId { get; set; }

    private Guid? _companyId;
    public Guid CompanyId
    {
        get => _companyId ?? DebtorReferenceId;
        set => _companyId = value;
    }

    public string? DebtorName { get; set; }

    private string? _payerName;
    public string? PayerName
    {
        get => _payerName ?? DebtorName;
        set => _payerName = value;
    }

    private string? _companyName;
    public string? CompanyName
    {
        get => _companyName ?? (!string.IsNullOrWhiteSpace(_payerName) ? _payerName : (!string.IsNullOrWhiteSpace(DebtorName) ? DebtorName : null));
        set => _companyName = value;
    }

    public string VisitCode { get; set; } = string.Empty;
    public string VisitLabel { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public int? PaymentTermDays { get; set; }
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; } = 0m;
    public decimal NetAmount { get; set; }
    public int TransactionCount { get; set; }

    private int? _memberCount;
    public int MemberCount
    {
        get => _memberCount ?? TransactionCount;
        set => _memberCount = value;
    }

    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? IssuedAt { get; set; }

    private DateTimeOffset? _sentDate;
    public DateTimeOffset? SentDate
    {
        get => _sentDate ?? IssuedAt;
        set => _sentDate = value;
    }

    public Guid RowVersion { get; set; }

    // Sumbu klaim penjamin (BE-FIN-052, FIN-DEC-097) — aditif, lihat FIN-API-1.3 §D.3.
    public string? ClaimStatus { get; set; }
    public decimal? ApprovedAmount { get; set; }

    /// <summary>Dihitung (TotalAmount - ApprovedAmount), TIDAK disimpan. Null selama belum APPROVED (FIN-DES-071).</summary>
    public decimal? ClaimVarianceAmount { get; set; }

    public string? PayerClaimReference { get; set; }
    public string? ClaimNote { get; set; }
    public DateTimeOffset? PayerVerifiedAt { get; set; }
    public DateTimeOffset? ClaimApprovedAt { get; set; }
    public DateTimeOffset? ClaimClosedAt { get; set; }
}

public sealed class ReceivableInvoiceBatchSummaryResponse
{
    public decimal TotalAmount { get; set; }
    public int TransactionCount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetAmount { get; set; }
}

public sealed class ClaimVerifyRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(100)] public string? PayerClaimReference { get; set; }
    [MaxLength(500)] public string? ClaimNote { get; set; }
}

public sealed class ClaimApproveRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }

    [Required(ErrorMessage = "Nominal yang disetujui penjamin wajib diisi.")]
    [Range(0, double.MaxValue, ErrorMessage = "Nominal yang disetujui tidak boleh kurang dari nol.")]
    public decimal? ApprovedAmount { get; set; }

    [MaxLength(100)] public string? PayerClaimReference { get; set; }

    /// <summary>Wajib diisi bila ApprovedAmount lebih kecil dari TotalAmount (FIN-VAL-151) — diperiksa di service, bukan di sini, karena TotalAmount bukan bagian request.</summary>
    [MaxLength(500)] public string? ClaimNote { get; set; }
}

public sealed class ClaimCloseRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(500)] public string? ClaimNote { get; set; }
}

public sealed class ReceivableInvoiceBatchMemberResponse
{
    public Guid BatchItemId { get; set; }

    private Guid? _id;
    public Guid Id
    {
        get => _id ?? BatchItemId;
        set => _id = value;
    }

    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    // BE-FIN-079: nullable — kosong untuk item migrasi tagihan lama.
    public Guid? InvoiceId { get; set; }
    public Guid? PatientId { get; set; }
    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string BillingNumber { get; set; } = string.Empty;
    public DateOnly? BillingDate { get; set; }
    public DateOnly? InvoiceDate => BillingDate;
    public DateOnly DueDate { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal AlreadyPaidAmount => AllocatedAmount;
    public decimal AdjustedAmount { get; set; }
    public decimal WrittenOffAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string ReceivableStatus { get; set; } = string.Empty;

    private bool? _isSettled;
    public bool IsSettled
    {
        get => _isSettled ?? (OutstandingAmount <= 0m || ReceivableStatus == "SETTLED");
        set => _isSettled = value;
    }

    public string DueDatePeriod { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string? Keterangan => Note;
}

public sealed class ReceivableInvoiceBatchPaymentHistoryAllocationResponse
{
    public Guid AllocationId { get; set; }
    public Guid? ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public decimal Amount { get; set; }
    public bool IsReversal { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
}

public sealed class ReceivableInvoiceBatchPaymentHistoryResponse
{
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public DateTimeOffset PaymentDate { get; set; }
    public DateTimeOffset OccurredAt => PaymentDate;
    public string PaymentMethodName { get; set; } = string.Empty;
    public Guid? BankAccountId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountName { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Amount => TotalAmount;
    public decimal AllocatedAmount { get; set; }
    public decimal Pph23Amount { get; set; }
    public decimal BankAdminFeeAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<ReceivableInvoiceBatchPaymentHistoryAllocationResponse> Allocations { get; set; } = new();
}

public sealed class ReceivableInvoiceBatchDetailResponse : ReceivableInvoiceBatchResponse
{
    public decimal TotalReceivable { get; set; }
    public decimal TotalAllocated { get; set; }
    public decimal TotalOutstanding { get; set; }
    public decimal TotalAdjusted { get; set; }
    public decimal TotalWrittenOff { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? DiscountNote { get; set; }
    public decimal OtherReceiptAmount { get; set; }
    public string? OtherReceiptNote { get; set; }
    public decimal Pph23Amount { get; set; }
    public decimal BankAdminFeeAmount { get; set; }
    public decimal NetInvoiceAmount { get; set; }

    public List<ReceivableInvoiceBatchMemberResponse> Members { get; set; } = new();
    public List<ReceivableInvoiceBatchPaymentHistoryResponse> Payments { get; set; } = new();
}

public sealed class ReceivableInvoiceBatchDocumentResponse
{
    public Guid BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string InvoiceNo => BatchNumber;
    public Guid DebtorReferenceId { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal GrandTotalCoveredAmount { get; set; }
    public List<CompanyGuarantorInvoiceDocumentResponse> Invoices { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

// ====================================================================================
// DTO POSTING PEMBAYARAN AR BATCH
// ====================================================================================

public sealed class PostReceivableInvoiceBatchPaymentAllocationLine
{
    [Required] public Guid ReceivableId { get; set; }
    [Range(0, double.MaxValue, ErrorMessage = "Nominal alokasi tidak boleh negatif.")]
    public decimal Amount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Pph23Amount { get; set; }
    public decimal BankAdminFeeAmount { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

public sealed class PostReceivableInvoiceBatchPaymentDiscount
{
    public Guid? ChartOfAccountId { get; set; }
    [MaxLength(20)] public string? DiscountType { get; set; }
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

public sealed class PostReceivableInvoiceBatchPaymentOtherReceipt
{
    public Guid? ChartOfAccountId { get; set; }
    public decimal Amount { get; set; }
    [MaxLength(500)] public string? Note { get; set; }
}

public sealed class PostReceivableInvoiceBatchPaymentPph23
{
    public Guid? ChartOfAccountId { get; set; }
    [MaxLength(20)] public string? DeductionType { get; set; } = "PERCENT";
    public decimal Value { get; set; }
    public decimal Amount { get; set; }
}

public sealed class PostReceivableInvoiceBatchPaymentAdminFee
{
    public Guid? ChartOfAccountId { get; set; }
    public decimal Amount { get; set; }
}

public sealed class PostReceivableInvoiceBatchPaymentRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
    [MaxLength(30)] public string PaymentSource { get; set; } = "BANK_ACCOUNT";
    public Guid? BankAccountId { get; set; }
    public DateTimeOffset? PaymentDate { get; set; }

    private decimal _receiptAmount;
    public decimal ReceiptAmount
    {
        get => _receiptAmount > 0 ? _receiptAmount : TotalReceiptAmount;
        set => _receiptAmount = value;
    }
    public decimal TotalReceiptAmount
    {
        get => _receiptAmount;
        set => _receiptAmount = value;
    }

    [MaxLength(100)] public string? ReferenceNumber { get; set; }
    [MaxLength(500)] public string? Note { get; set; }

    public decimal DiscountAmount { get; set; }
    public decimal DiscountPercent { get; set; }
    public Guid? DiscountChartOfAccountId { get; set; }
    [MaxLength(500)] public string? DiscountNote { get; set; }

    public decimal OtherReceiptAmount { get; set; }
    public Guid? OtherReceiptChartOfAccountId { get; set; }
    [MaxLength(500)] public string? OtherReceiptNote { get; set; }

    public decimal Pph23Amount { get; set; }
    public Guid? Pph23ChartOfAccountId { get; set; }

    public decimal BankAdminFeeAmount { get; set; }
    public Guid? BankAdminFeeChartOfAccountId { get; set; }

    public List<PostReceivableInvoiceBatchPaymentAllocationLine> Allocations { get; set; } = new();

    private List<PostReceivableInvoiceBatchPaymentAllocationLine>? _memberAllocations;
    public List<PostReceivableInvoiceBatchPaymentAllocationLine> MemberAllocations
    {
        get => _memberAllocations ?? Allocations;
        set => _memberAllocations = value;
    }

    public PostReceivableInvoiceBatchPaymentDiscount? InvoiceDiscount { get; set; }
    public PostReceivableInvoiceBatchPaymentOtherReceipt? OtherReceipt { get; set; }
    public PostReceivableInvoiceBatchPaymentPph23? Pph23 { get; set; }
    public PostReceivableInvoiceBatchPaymentAdminFee? BankAdminFee { get; set; }
}

public sealed class PostReceivableInvoiceBatchPaymentResponse
{
    public Guid BatchId { get; set; }
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string BatchStatus
    {
        get => string.IsNullOrEmpty(_batchStatus) ? Status : _batchStatus;
        set => _batchStatus = value;
    }
    private string? _batchStatus;
    public decimal TotalReceiptAmount { get; set; }
    public decimal TotalDiscountAmount { get; set; }
    public decimal TotalPph23Amount { get; set; }
    public decimal TotalBankAdminFeeAmount { get; set; }
    public decimal TotalOtherReceiptAmount { get; set; }
    public Guid NewRowVersion { get; set; }
    public int AllocationsCount { get; set; }
    public ReceivableInvoiceBatchDetailResponse? BatchDetail { get; set; }
}

// ====================================================================================
// DTO CANCELED INVOICE / CANCELLED RECEIVABLE INVOICE WORKFLOW
// ====================================================================================

public sealed class CanceledInvoiceBatchQuery
{
    public string? Search { get; set; }
    public string? DateType { get; set; } // "INVOICE_DATE" | "CANCEL_DATE"
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? ServiceType { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string? Category { get; set; }
    public string SortBy { get; set; } = "cancelDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 100)] public int PageSize { get; set; } = 10;
}

public sealed class CanceledInvoiceCapabilityItem
{
    public bool Available { get; set; }
    public string? Reason { get; set; }
}

public sealed class CanceledInvoiceDocumentCapabilities
{
    public CanceledInvoiceCapabilityItem Invoice { get; set; } = new() { Available = true };
    public CanceledInvoiceCapabilityItem ReceiptAcknowledgement { get; set; } = new() { Available = true };
    public CanceledInvoiceCapabilityItem CashierReceipt { get; set; } = new();
    public CanceledInvoiceCapabilityItem BillingRecap { get; set; } = new() { Available = true };
}

public sealed class CanceledInvoiceMemberActionCapabilities
{
    public CanceledInvoiceCapabilityItem Print { get; set; } = new() { Available = true };
    public CanceledInvoiceCapabilityItem CareBill { get; set; } = new();
    public CanceledInvoiceCapabilityItem CostDetail { get; set; } = new();
}

public sealed class CanceledInvoiceRowResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string BatchNumber => InvoiceNumber;

    public Guid DebtorReferenceId { get; set; }
    public string? DebtorName { get; set; }
    public string? DebtorKind { get; set; }

    public string? ServiceType { get; set; }
    public string? ServiceTypeName { get; set; }

    public DateOnly? InvoiceDate { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public DateOnly? DueDate { get; set; }

    public decimal TotalAmount { get; set; }

    public Guid CancelledBy { get; set; }
    public string? CancelledByName { get; set; }
    public string? CancelReason { get; set; }

    public Guid? ReissuedToBatchId { get; set; }
    public Guid? ReissuedFromBatchId { get; set; }
    public bool IsReissued => ReissuedToBatchId.HasValue;

    public Guid RowVersion { get; set; }

    public CanceledInvoiceDocumentCapabilities DocumentCapabilities { get; set; } = new();
}

public sealed class CanceledInvoiceSummaryResponse
{
    public decimal TotalAmount { get; set; }
    public int InvoiceCount { get; set; }
    public decimal TargetAmount { get; set; }
    public string TargetName { get; set; } = "Semua Perusahaan";
}

public sealed class CanceledInvoicePagedResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
    public List<CanceledInvoiceRowResponse> Items { get; set; } = new();
    public CanceledInvoiceSummaryResponse? Summary { get; set; }
}

public sealed class CanceledInvoiceMemberDetailResponse
{
    public Guid BatchItemId { get; set; }
    public Guid ReceivableId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? EncounterId { get; set; }
    public Guid? PatientId { get; set; }

    public string PatientName { get; set; } = string.Empty;
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string BillingNumber { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;

    public decimal OriginalAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal AdjustedAmount { get; set; }
    public decimal WrittenOffAmount { get; set; }

    public decimal EditableTotalAmount { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount { get; set; }

    public string ReceivableStatus { get; set; } = string.Empty;

    public bool CanEdit { get; set; }
    public string? EditBlockedReason { get; set; }

    public CanceledInvoiceMemberActionCapabilities ActionCapabilities { get; set; } = new();
    public CanceledInvoiceMemberActionCapabilities DocumentCapabilities => ActionCapabilities;
}

public sealed class CanceledInvoiceDetailResponse
{
    public Guid Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public string BatchNumber => InvoiceNumber;

    public Guid DebtorReferenceId { get; set; }
    public string? DebtorName { get; set; }
    public string? DebtorKind { get; set; }

    public string? ServiceType { get; set; }
    public string? ServiceTypeName { get; set; }

    public DateOnly? InvoiceDate { get; set; }
    public DateTimeOffset? SentDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public DateTimeOffset? CancelledAt { get; set; }
    public string? CancelReason { get; set; }

    public Guid CancelledBy { get; set; }
    public string? CancelledByName { get; set; }

    public decimal TotalAmount { get; set; }
    public Guid RowVersion { get; set; }

    public Guid? ReissuedToBatchId { get; set; }
    public Guid? ReissuedFromBatchId { get; set; }
    public bool IsReissued => ReissuedToBatchId.HasValue;
    public bool CanReissue { get; set; }
    public string? ReissueBlockedReason { get; set; }

    public CanceledInvoiceDocumentCapabilities DocumentCapabilities { get; set; } = new();
    public List<CanceledInvoiceMemberDetailResponse> Items { get; set; } = new();
    public List<CanceledInvoiceMemberDetailResponse> Members => Items;
}

public sealed class CancelReceivableInvoiceBatchRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }

    [Required(ErrorMessage = "Alasan pembatalan wajib diisi.")]
    [MaxLength(500, ErrorMessage = "Alasan pembatalan tidak boleh melebihi 500 karakter.")]
    public string Reason { get; set; } = string.Empty;
}

public sealed class EditCanceledInvoiceMemberAmountRequest
{
    [Required] public Guid BatchItemId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Total Tagihan tidak boleh kurang dari nol.")]
    public decimal TotalTagihan { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Nilai Diskon tidak boleh kurang dari nol.")]
    public decimal DiscountAmount { get; set; } = 0m;

    [MaxLength(500)]
    public string? Reason { get; set; }

    [Required] public Guid ExpectedRowVersion { get; set; }
}

public sealed class EditCanceledInvoiceMemberAmountResponse
{
    public Guid BatchItemId { get; set; }
    public Guid ReceivableId { get; set; }
    public decimal NewTotalTagihan { get; set; }
    public decimal NewDiscountAmount { get; set; }
    public decimal NewFinalAmount { get; set; }
    public Guid? AdjustmentId { get; set; }
    public string? AdjustmentNumber { get; set; }
    public string? AdjustmentStatus { get; set; }
    public string Message { get; set; } = string.Empty;
}

public sealed class ReissueReceivableInvoiceBatchRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
}
