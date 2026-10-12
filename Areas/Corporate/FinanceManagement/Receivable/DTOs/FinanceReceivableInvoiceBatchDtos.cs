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
    public string? EncounterNumber { get; set; }
    public string? ServiceType { get; set; }
    public string? ServiceLabel { get; set; }
    public DateOnly? ServiceDate { get; set; }
    public string? PatientClassName { get; set; }
    public string? DoctorName { get; set; }
    public string? PrimaryDiagnosisCode { get; set; }
    public string? PrimaryDiagnosisName { get; set; }
    public string? SecondaryDiagnoses { get; set; }
}

public sealed class ReceivableInvoiceBatchPaymentHistoryAllocationResponse
{
    public Guid AllocationId { get; set; }
    public Guid? ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public decimal Amount { get; set; }
    public bool IsReversal { get; set; }

    /// <summary>Terisi hanya pada baris IsReversal=true — menunjuk AllocationId yang dibalik baris
    /// ini. FE butuh ini untuk menentukan allocation mana yang masih AKTIF (belum pernah dibalik)
    /// sebelum memanggil POST /receipts/{receiptId}/allocations/{allocationId}/reverse — tanpa
    /// field ini FE tidak bisa membedakan "sudah dibalik" dari "belum", dan berisiko membalik baris
    /// yang salah atau membalik baris yang sudah dibalik sebelumnya.</summary>
    public Guid? ReversalOfAllocationId { get; set; }

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

public static class ReceivableInvoiceBatchDocumentTypes
{
    public const string InvoiceLetter = "INVOICE_LETTER";
    public const string Acknowledgement = "ACKNOWLEDGEMENT";
    public const string PaymentReceipt = "PAYMENT_RECEIPT";
    public const string BillingRecap = "BILLING_RECAP";
}

public sealed class ReceivableInvoiceBatchDocumentMetadataResponse
{
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentNumber { get; set; } = string.Empty;
    public bool Available { get; set; } = true;
    public string? Message { get; set; }
    public string Language { get; set; } = "id";
    public Guid? RelatedInvoiceId { get; set; }
    public Guid? RelatedReceiptId { get; set; }
    public bool Printable { get; set; } = true;
    public bool Downloadable { get; set; } = true;
}

public sealed class ReceivableInvoiceBatchBankAccountResponse
{
    public Guid BankAccountId { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string BankCode { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string? Branch { get; set; }
    public string CurrencyCode { get; set; } = "IDR";
    public string AccountType { get; set; } = string.Empty;
}

public sealed class ReceivableInvoiceBatchReceiptResponse
{
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? KwitansiNumber { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string? ProviderReference { get; set; }
    public Guid? BankAccountId { get; set; }
    public string? BankName { get; set; }
    public string? AccountNumber { get; set; }
    public string? AccountName { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class ReceivableInvoiceBatchRecapItemResponse
{
    public Guid? InvoiceItemId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? CategoryCode { get; set; }
    public string? CategoryName { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal ItemDiscount { get; set; }
    public decimal NetAmount { get; set; }
    public decimal CoveredNetAmount { get; set; }
    public decimal CoveredTaxAmount { get; set; }
    public decimal CoveredAmount { get; set; }
    public decimal PatientAmount { get; set; }
    public string? CoverageNote { get; set; }
}

public sealed class ReceivableInvoiceBatchRecapPatientResponse
{
    public int SequenceNumber { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid ReceivableId { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = string.Empty;
    public string BillingNumber { get; set; } = string.Empty;
    public string VisitCode { get; set; } = string.Empty;
    public string VisitType { get; set; } = string.Empty;
    public string? Note { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PatientPaidAmount { get; set; }
    public decimal GuarantorCoveredAmount { get; set; }
    public List<ReceivableInvoiceBatchRecapItemResponse> CoverageItems { get; set; } = new();
}

public sealed class ReceivableInvoiceBatchDocumentResponse
{
    public Guid BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string InvoiceNo => BatchNumber;
    public Guid DebtorReferenceId { get; set; }
    public Guid CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string? PicName { get; set; }
    public string? PicPhone { get; set; }
    public string? PicEmail { get; set; }
    public string? ContractNumber { get; set; }
    public string DebtorType { get; set; } = string.Empty;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public decimal GrandTotalCoveredAmount { get; set; }
    public decimal TotalBillAmount { get; set; }
    public decimal TotalPatientAmount { get; set; }
    public int PatientCount { get; set; }
    public bool IsBankAccountConfigured { get; set; } = true;
    public List<ReceivableInvoiceBatchBankAccountResponse> BankAccounts { get; set; } = new();
    public List<ReceivableInvoiceBatchReceiptResponse> Receipts { get; set; } = new();
    public List<ReceivableInvoiceBatchRecapPatientResponse> Patients { get; set; } = new();
    public List<CompanyGuarantorInvoiceDocumentResponse> Invoices { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

public sealed class ReceivableInvoiceBatchSingleDocumentResponse
{
    public ReceivableInvoiceBatchDocumentMetadataResponse Metadata { get; set; } = new();
    public ReceivableInvoiceBatchDocumentResponse Document { get; set; } = new();
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
    public decimal TotalDiscount { get; set; }
    public decimal NetAmount { get; set; }

    /// <summary>Selalu CANCELLED — baris ini hanya pernah diisi dari hasil query yang sudah
    /// difilter Status == CANCELLED (lihat GetCanceledPagedAsync). Field eksplisit disediakan
    /// agar FE tidak perlu mengasumsikan status dari konteks endpoint.</summary>
    public string Status { get; set; } = "CANCELLED";

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
    /// <summary>Total Filter — agregat berdasarkan seluruh filter aktif (search, rentang tanggal,
    /// jenis layanan, DebtorReferenceId, dll). Ini adalah angka yang merepresentasikan hasil
    /// tabel yang sedang ditampilkan FE.</summary>
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetAmount { get; set; }
    public int InvoiceCount { get; set; }

    /// <summary>Total Target — agregat sepanjang masa untuk target yang dipilih (DebtorReferenceId),
    /// TANPA rentang tanggal/search/jenis layanan. Bila tidak ada DebtorReferenceId yang dipilih,
    /// bernilai sama dengan total seluruh invoice CANCELLED sepanjang masa (target = "Semua
    /// Perusahaan"). Berbeda definisi dari TotalAmount (Total Filter) secara sengaja — lihat
    /// GetCanceledSummaryAsync.</summary>
    public decimal TargetAmount { get; set; }
    public string TargetName { get; set; } = "Semua Perusahaan";

    /// <summary>False bila Category pada request adalah PATIENT/EMPLOYEE — V2 FinReceivableInvoiceBatch
    /// hanya pernah menampung piutang DebtorType PAYER (FIN-DEC-048), dan FinReceivable non-PAYER
    /// tidak pernah memiliki jalur pembatalan (tidak ada service yang menulis Status=CANCELLED untuk
    /// DebtorType PATIENT_GUARANTOR/EMPLOYEE_BENEFIT). Lihat CategoryLimitationReason.</summary>
    public bool CategorySupported { get; set; } = true;
    public string? CategoryLimitationReason { get; set; }
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
    public DateOnly? VisitDate { get; set; }

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

    public string Status { get; set; } = "CANCELLED";

    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetAmount { get; set; }
    public Guid RowVersion { get; set; }

    public Guid? ReissuedToBatchId { get; set; }
    public Guid? ReissuedFromBatchId { get; set; }
    public bool IsReissued => ReissuedToBatchId.HasValue;
    public bool CanReissue { get; set; }
    public string? ReissueBlockedReason { get; set; }

    public CanceledInvoiceDocumentCapabilities DocumentCapabilities { get; set; } = new();
    public CanceledInvoiceDetailSummary Summary { get; set; } = new();
    public List<CanceledInvoiceMemberDetailResponse> Items { get; set; } = new();
    public List<CanceledInvoiceMemberDetailResponse> Members => Items;
}

/// <summary>Ringkasan header Canceled Invoice Detail — dihitung dari kolom batch (TotalAmount/
/// TotalDiscount), BUKAN dari penjumlahan Items, karena DiscountAmount per-anggota belum pernah
/// diisi oleh service manapun (selalu 0, lihat GetCanceledDetailAsync) sehingga menjumlahkannya
/// akan menghasilkan diskon palsu.</summary>
public sealed class CanceledInvoiceDetailSummary
{
    public int ItemCount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal NetAmount { get; set; }
}

public sealed class CancelReceivableInvoiceBatchRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }

    [Required(ErrorMessage = "Alasan pembatalan wajib diisi.")]
    [MaxLength(500, ErrorMessage = "Alasan pembatalan tidak boleh melebihi 500 karakter.")]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Request Revive (un-cancel) — mengembalikan batch CANCELLED ke status sebelum
/// dibatalkan. Tidak ada Reason: ini bukan koreksi finansial, murni pemulihan status sebelum
/// pembayaran apa pun pernah ada (CancelAsync tidak pernah mengizinkan cancel batch yang sudah
/// memiliki alokasi pembayaran — lihat FinanceReceivableInvoiceBatchService.CancelAsync).</summary>
public sealed class ReviveReceivableInvoiceBatchRequest
{
    [Required] public Guid ExpectedRowVersion { get; set; }
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

// ====================================================================================
// REPORT AR CREATED (Laporan Pembuatan AR)
// ====================================================================================

public sealed class ReportArCreatedQuery
{
    public string? Search { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string? VisitType { get; set; } // "ALL", "RAJAL", "RANAP", "IGD", "OP", "IP"
    public string? PeriodType { get; set; } // "ALL", "TODAY", "THIS_WEEK", "THIS_MONTH", "LAST_MONTH", "THIS_YEAR", "CUSTOM"
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? DateType { get; set; } = "INVOICE_DATE"; // "INVOICE_DATE" | "REGISTRATION_DATE"
    public string? Status { get; set; } // default null (excludes CANCELLED)
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 30; // Max 30 enforced
    public string SortBy { get; set; } = "company";
    public string SortDirection { get; set; } = "asc";
}

public sealed class ReportArCreatedItemResponse
{
    public Guid Id { get; set; } // Stable row identifier (bi.Id)
    public Guid BatchId { get; set; }
    public Guid BatchItemId { get; set; }
    public Guid ReceivableId { get; set; }
    public Guid? InvoiceId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty; // NO. INVOICE (b.BatchNumber)
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty; // NAMA PERUSAHAAN / ASURANSI
    public DateTimeOffset? InvoiceDate { get; set; } // TANGGAL INVOICE
    public DateTimeOffset? RegistrationDate { get; set; } // TANGGAL REGISTRASI
    public string BillingNumber { get; set; } = string.Empty; // NO. BILL (inv.InvoiceNumber)
    public string RegistrationNumber { get; set; } = string.Empty; // NO. REG (enc.EncounterNumber)
    public string MedicalRecordNumber { get; set; } = string.Empty; // NO. RM (pat.MedicalRecordNumber)
    public string PatientName { get; set; } = string.Empty; // NAMA PASIEN (pat.FullName)
    public string VisitType { get; set; } = string.Empty; // RAJAL / RANAP / IGD
    public string VisitTypeName { get; set; } = string.Empty; // Rawat Jalan / Rawat Inap / IGD
    public decimal BillingAmount { get; set; } // JUMLAH TAGIHAN (r.OriginalAmount)
    public string BatchStatus { get; set; } = string.Empty;
}

public sealed class ReportArCreatedDateSubtotal
{
    public string Date { get; set; } = string.Empty; // "yyyy-MM-dd"
    public string DateFormatted { get; set; } = string.Empty; // "26 Juni 2026"
    public decimal SubtotalAmount { get; set; }
    public int RowCount { get; set; }
}

public sealed class ReportArCreatedGroupResponse
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal TotalCompanyAmount { get; set; } // Total company across entire filtered dataset
    public decimal PageSubtotalAmount { get; set; } // Subtotal on current page
    public List<ReportArCreatedDateSubtotal> DateSubtotals { get; set; } = new();
    public List<ReportArCreatedItemResponse> Items { get; set; } = new();
}

public sealed class ReportArCreatedCompanySummary
{
    public Guid CompanyId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int InvoiceCount { get; set; }
    public int RowCount { get; set; }
}

public sealed class ReportArCreatedVisitTypeSummary
{
    public string VisitType { get; set; } = string.Empty;
    public string VisitTypeName { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal Amount { get; set; }
}

public sealed class ReportArCreatedSummaryResponse
{
    public int TotalInvoiceCount { get; set; } // Distinct invoice batches
    public int TotalDetailRows { get; set; } // Total billing items
    public decimal TotalBillingAmount { get; set; } // Total tagihan across all filtered data
    public string PeriodLabel { get; set; } = string.Empty;
    public string SelectedCompanyName { get; set; } = "Semua Perusahaan";
    public ReportArCreatedCompanySummary? TopCompanySummary { get; set; }
    public List<ReportArCreatedCompanySummary> TotalByCompany { get; set; } = new();
    public List<ReportArCreatedVisitTypeSummary> VisitTypeSummary { get; set; } = new();
}

public sealed class ReportArCreatedPagedResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
    public List<ReportArCreatedItemResponse> Items { get; set; } = new();
    public List<ReportArCreatedGroupResponse> Groups { get; set; } = new();
    public ReportArCreatedSummaryResponse Summary { get; set; } = new();
}

// ------------------------------------------------------------------------------------
// REVAMP MANAJEMEN KLAIM PENJAMIN V2 (Read-Model DTOs)
// ------------------------------------------------------------------------------------

public sealed class ClaimManagementBatchQuery
{
    public string? Search { get; set; }
    public Guid? PayerId { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string? VisitCode { get; set; }
    public string? PeriodPreset { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? ClaimStatus { get; set; }
    public string? PaymentStatus { get; set; }
    public string SortBy { get; set; } = "createDateTime";
    public string SortDirection { get; set; } = "desc";
    [Range(1, int.MaxValue)] public int PageNumber { get; set; } = 1;
    [Range(1, 30)] public int PageSize { get; set; } = 20;
}

public sealed class ClaimManagementBatchItemResponse
{
    public Guid Id { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string? PayerClaimReference { get; set; }
    public Guid DebtorReferenceId { get; set; }
    public string PayerName { get; set; } = string.Empty;
    public int MemberCount { get; set; }
    public string? SinglePatientName { get; set; }
    public string? SingleMedicalRecordNumber { get; set; }
    public string? SingleBillingNumber { get; set; }
    public string? SingleEncounterNumber { get; set; }
    public string VisitCode { get; set; } = string.Empty;
    public string VisitLabel { get; set; } = string.Empty;
    public string? DiagnosisSummary { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public decimal? ClaimVarianceAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public string? ClaimStatus { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public DateTimeOffset? IssuedAt { get; set; }
    public DateTimeOffset? PayerVerifiedAt { get; set; }
    public DateTimeOffset? ClaimApprovedAt { get; set; }
    public DateTimeOffset? ClaimClosedAt { get; set; }
    public string? ClaimNote { get; set; }
    public string? CancelReason { get; set; }
    public DateTimeOffset? CancelDateTime { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class ClaimManagementSummaryResponse
{
    public int TotalClaims { get; set; }
    public int TotalInvoices { get; set; }
    public int UnpaidCount { get; set; }
    public int PartiallyPaidCount { get; set; }
    public int PaidCount { get; set; }
    public int OverdueCount { get; set; }
    public int PendingVerificationCount { get; set; }
    public int PendingApprovalCount { get; set; }
    public int ApprovedCount { get; set; }
    public int ClosedCount { get; set; }
    public decimal TotalClaimAmount { get; set; }
    public decimal TotalApprovedAmount { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public decimal TotalOutstandingAmount { get; set; }
}

public sealed class ClaimManagementPagedResponse
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalData { get; set; }
    public int TotalPage { get; set; }
    public List<ClaimManagementBatchItemResponse> Items { get; set; } = new();
    public ClaimManagementSummaryResponse Summary { get; set; } = new();
}


