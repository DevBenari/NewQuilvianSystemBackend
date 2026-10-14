namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;

public sealed class FinReceiptAllocationResponse
{
    public Guid Id { get; set; }
    public Guid ReceiptId { get; set; }
    public Guid? ReceivableId { get; set; }
    public string TargetType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsReversal { get; set; }
    public Guid? ReversalOfAllocationId { get; set; }
    public Guid AllocatedBy { get; set; }
    public DateTimeOffset AllocatedAt { get; set; }
}

public class FinReceiptResponse
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public Guid? SourceTenderId { get; set; }
    public Guid? InvoiceId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public Guid? CashierShiftId { get; set; }
    public decimal Amount { get; set; }
    public decimal AllocatedAmount { get; set; }
    public decimal UnallocatedAmount { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid? ReversalOfReceiptId { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class FinReceiptDetailResponse : FinReceiptResponse
{
    public List<FinReceiptAllocationResponse> Allocations { get; set; } = [];
}

/// <summary>Satu baris potongan sisi penerimaan (BE-FIN-040, FIN-DES-048). Hanya sah pada baris
/// alokasi ber-ReceivableId terisi (FIN-VAL-128).</summary>
public sealed class ReceiptDeductionLineRequestDto
{
    public string DeductionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
}

/// <summary>Satu baris permintaan alokasi. ReceivableId kosong = INVOICE_DIRECT (FIN-DES-011).</summary>
public sealed class AllocationLineRequestDto
{
    public Guid? ReceivableId { get; set; }
    public decimal Amount { get; set; }
    public List<ReceiptDeductionLineRequestDto>? Deductions { get; set; }
}

public sealed class AllocateReceiptRequest
{
    public List<AllocationLineRequestDto> Lines { get; set; } = [];
}

/// <summary>Baris GET /receipts/{id}/deductions (FIN-API-1.2 B.8).</summary>
public sealed class ReceiptDeductionResponse
{
    public Guid Id { get; set; }
    public string DeductionNumber { get; set; } = string.Empty;
    public Guid ReceiptId { get; set; }
    public Guid ReceiptAllocationId { get; set; }
    public string DeductionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
    public bool IsReversal { get; set; }
    public Guid? ReversalOfDeductionId { get; set; }
    public Guid RowVersion { get; set; }
}

public sealed class FinReceiptQuery
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }
    public string? Status { get; set; }
    public string? SourceType { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public string SortBy { get; set; } = "OccurredAt";
    public string SortDirection { get; set; } = "desc";
}

/// <summary>BE-FIN-050, `GET /receipts/register` (FIN-API-1.0). Buku penerimaan kasir per tanggal.</summary>
public sealed class ReceiptRegisterQuery
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public Guid? CashierShiftId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu baris buku penerimaan kasir — menelusur ke tender dan kwitansi asalnya
/// (KwitansiNumber/SourceTenderId tidak ada pada FinReceiptResponse umum).</summary>
public sealed class ReceiptRegisterResponse
{
    public Guid Id { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public string? KwitansiNumber { get; set; }
    public Guid? SourceTenderId { get; set; }
    public Guid? CashierShiftId { get; set; }
    public Guid? PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}

/// <summary>BE-FIN-050, `GET /receipts/shift-reconciliation` (FIN-API-1.0). Satu shift per
/// pemanggilan — kontrak menyatakan response tunggal, bukan daftar berpaging.</summary>
public sealed class ShiftReconciliationQuery
{
    [System.ComponentModel.DataAnnotations.Required] public Guid CashierShiftId { get; set; }
}

/// <summary>Perbandingan total penerimaan tunai Finance dengan SystemCash Billing untuk satu
/// shift kasir. Baca saja — tidak pernah menulis FinReceipt maupun BilCashierShift.</summary>
public sealed class ShiftReconciliationResponse
{
    public Guid CashierShiftId { get; set; }
    public string ShiftNumber { get; set; } = string.Empty;
    public string CashierShiftStatus { get; set; } = string.Empty;
    public decimal SystemCashAmount { get; set; }
    public decimal FinanceNetCashReceiptAmount { get; set; }
    public decimal Variance { get; set; }
    public int CashReceiptCount { get; set; }
}

// ----------------------------------------------------------------------------------------
// BE-FIN-054 (FIN-DEC-094, FIN-DES-073) — daftar baris alokasi yang DIBALIK, untuk layar
// "Receiveable AR Canceled" (03-frontend-architecture.md §17.2). Baca saja: pembalikan alokasi
// TETAP lewat POST /{id}/allocations/{allocationId}/reverse yang sudah ada — nol aksi baru.
// Grain-nya baris alokasi (IsReversal = true), bukan penerimaan.
// ----------------------------------------------------------------------------------------

public sealed class ReversedAllocationQuery
{
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public Guid? ReceiptId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
}

/// <summary>Satu baris pembalik alokasi (FinReceiptAllocation.IsReversal = true). Kontrak
/// FIN-API-1.3 §D.2 awalnya menyebut field ReversalReason — DICABUT di sini: FinReceiptAllocation
/// tidak punya kolom alasan sama sekali (lihat Models/FinReceiptAllocation.cs), dan ReverseAllocationAsync
/// tidak menerima parameter alasan. Dicatat sebagai delta kontrak-vs-source, bukan dikarang.</summary>
public sealed class ReversedAllocationRowResponse
{
    public Guid AllocationId { get; set; }
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid? ReceivableId { get; set; }
    public string? ReceivableNumber { get; set; }
    public decimal Amount { get; set; }
    public Guid ReversalOfAllocationId { get; set; }
    public DateTimeOffset ReversedAt { get; set; }
}

/// <summary>
/// Parameter kueri filter dan paginasi untuk daftar Canceled AR Payment Sessions (Receivable AR Canceled).
/// Mendukung pencarian server-side, rentang tanggal transaksi pembayaran original, penjamin, tipe kunjungan, dan nomor invoice.
/// </summary>
public sealed class CanceledArPaymentQuery
{
    public string? Search { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public Guid? DebtorReferenceId { get; set; }
    public string? VisitType { get; set; }
    public string? InvoiceNumber { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Satu baris sesi pembayaran AR yang dibatalkan (1 payment session = 1 row).
/// Merepresentasikan satu transaksi pembayaran yang kemudian direverse/dibatalkan.
/// </summary>
public sealed class CanceledArPaymentRowResponse
{
    public Guid SessionId { get; set; }
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid? BatchId { get; set; }
    public string InvoiceNumber { get; set; } = "-";
    public Guid? DebtorReferenceId { get; set; }
    public string CompanyName { get; set; } = "-";
    public string? VisitType { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public decimal TotalTagihan { get; set; }
    public decimal TotalDiterima { get; set; }
    public decimal HistoricalRemaining { get; set; }
    public string Keterangan { get; set; } = "-";
    public string Status { get; set; } = "Canceled";
    public DateTimeOffset ReversedAt { get; set; }
    public Guid ReversedBy { get; set; }
    public string ReversedByName { get; set; } = "-";
    public string FollowUpStatus { get; set; } = "Belum Diposting Ulang";
}

/// <summary>
/// Ringkasan agregat 4 metrik untuk daftar Canceled AR Payment Sessions berdasarkan seluruh filter aktif.
/// </summary>
public sealed class CanceledArPaymentSummaryResponse
{
    public int TransactionCount { get; set; }
    public decimal TotalTagihan { get; set; }
    public decimal TotalBayar { get; set; }
    public decimal TotalSisa { get; set; }
}

/// <summary>
/// Rincian lengkap sesi pembayaran AR yang dibatalkan untuk halaman Detail Receivable AR Canceled.
/// </summary>
public sealed class CanceledArPaymentDetailResponse
{
    public Guid SessionId { get; set; }
    public Guid ReceiptId { get; set; }
    public string ReceiptNumber { get; set; } = string.Empty;
    public Guid? BatchId { get; set; }
    public string InvoiceNumber { get; set; } = "-";
    public Guid? DebtorReferenceId { get; set; }
    public string CompanyName { get; set; } = "-";
    public string? VisitType { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public int PaymentSequenceNumber { get; set; } = 1;
    public DateTimeOffset ReversedAt { get; set; }
    public Guid ReversedBy { get; set; }
    public string ReversedByName { get; set; } = "-";
    public string PaymentSource { get; set; } = "CASH_BANK"; // CASH_BANK | AYAT_SILANG
    public Guid? BankAccountId { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankAccountName { get; set; }
    public string? BankName { get; set; }
    public Guid? CrossEntryId { get; set; }
    public string? CrossEntryNumber { get; set; }
    public string? CrossEntryStatus { get; set; }
    public decimal TotalTagihan { get; set; }
    public decimal TotalDiterima { get; set; }
    public decimal HistoricalRemaining { get; set; }
    public string? CurrentBatchStatus { get; set; }
    public string? BatchCancelReason { get; set; }
    public bool CanReapply { get; set; }
    public string? ReapplyBlockedReason { get; set; }
    public Guid ReceiptRowVersion { get; set; }
    public Guid? BatchRowVersion { get; set; }
    public string? Note { get; set; }
    public decimal ReceiptAmount { get; set; }
    public decimal ReceiptUnallocatedAmount { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal OtherReceiptAmount { get; set; }
    public string FollowUpStatus { get; set; } = "Belum Diposting Ulang";
    public List<CanceledArPaymentMemberDetailResponse> Members { get; set; } = [];
    public List<CanceledArPaymentDeductionResponse> Deductions { get; set; } = [];
}

/// <summary>
/// Rincian tagihan per pasien / piutang dalam sesi pembayaran yang dibatalkan.
/// </summary>
public sealed class CanceledArPaymentMemberDetailResponse
{
    public Guid ReceivableId { get; set; }
    public string ReceivableNumber { get; set; } = string.Empty;
    public string PatientName { get; set; } = "-";
    public string MedicalRecordNumber { get; set; } = "-";
    public string BillingNumber { get; set; } = "-";
    public string RegistrationNumber { get; set; } = "-";
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public string? DueDatePeriod { get; set; }
    public decimal OriginalAmount { get; set; }
    public decimal OriginalPaidAmount { get; set; }
    public decimal HistoricalRemainingAmount { get; set; }
    public decimal CurrentOutstandingAmount { get; set; }
    public decimal CurrentAllocatedAmount { get; set; }
    public bool WasFullyPaid { get; set; }
    public string? Note { get; set; }
}

/// <summary>
/// Rincian potongan (PPh23, Biaya Admin Bank) yang melekat pada alokasi sesi pembayaran original.
/// </summary>
public sealed class CanceledArPaymentDeductionResponse
{
    public string DeductionType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? Reason { get; set; }
    public string? ReferenceNumber { get; set; }
}


