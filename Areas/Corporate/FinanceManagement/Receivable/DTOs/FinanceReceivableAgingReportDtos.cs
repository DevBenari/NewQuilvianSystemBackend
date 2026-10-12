namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;

/// <summary>
/// Parameter query untuk daftar penjamin umur piutang (Halaman A).
/// </summary>
public sealed class FinanceReceivableAgingPayerQuery
{
    public string? Search { get; set; }
    public DateOnly? AsOfDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Baris penjamin pada tabel daftar penjamin umur piutang (Halaman A).
/// </summary>
public sealed class ReceivableAgingPayerItemResponse
{
    public Guid PayerId { get; set; }
    public string PayerCode { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string PayerType { get; set; } = string.Empty;
    public int TotalInvoiceCount { get; set; }
    public decimal TotalOutstanding { get; set; }
}

/// <summary>
/// Hasil terpaginasi daftar penjamin umur piutang (Halaman A).
/// </summary>
public sealed class ReceivableAgingPayerPagedResponse
{
    public List<ReceivableAgingPayerItemResponse> Items { get; set; } = new();
    public int TotalPenjamin { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalPages { get; set; } = 1;
}

/// <summary>
/// Parameter query untuk detail invoice umur piutang per penjamin (Halaman B).
/// </summary>
public sealed class ReceivableAgingDetailQuery
{
    public DateOnly? AsOfDate { get; set; }
    public string? Status { get; set; }
    public string? Search { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
}

/// <summary>
/// Ringkasan tagihan, pembayaran, dan 6 bucket umur piutang per penjamin (Halaman B).
/// </summary>
public sealed class ReceivableAgingDetailSummaryResponse
{
    public Guid PayerId { get; set; }
    public string PayerCode { get; set; } = string.Empty;
    public string PayerName { get; set; } = string.Empty;
    public string PayerType { get; set; } = string.Empty;
    public DateOnly AsOfDate { get; set; }

    // Section 1: Ringkasan
    public int TotalInvoice { get; set; }
    public decimal TotalInvoiceAmount { get; set; }
    public decimal TotalEffectivePayment { get; set; }
    public decimal TotalOutstanding { get; set; }
    public int UnpaidInvoiceCount { get; set; }
    public int PartiallyPaidInvoiceCount { get; set; }
    public int PaidInvoiceCount { get; set; }

    // Section 2: Aging Piutang (Sesuai V1 + Belum Jatuh Tempo)
    public decimal NotDueAmount { get; set; }
    public decimal Bucket1_0_30 { get; set; }
    public decimal Bucket2_31_60 { get; set; }
    public decimal Bucket3_61_90 { get; set; }
    public decimal Bucket4_91_120 { get; set; }
    public decimal Bucket5_Over120 { get; set; }

    // Informasi Piutang Belum Ber-Invoice (jika ada)
    public int UninvoicedReceivableCount { get; set; }
    public decimal UninvoicedReceivableAmount { get; set; }
}

/// <summary>
/// Baris detail invoice pada tabel 13 kolom (Halaman B).
/// </summary>
public sealed class ReceivableAgingInvoiceItemResponse
{
    public Guid BatchId { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateOnly? InvoiceDate { get; set; }
    public DateOnly? DueDate { get; set; }
    public int DaysPastDue { get; set; }
    public bool HasDocument { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal OutstandingAmount { get; set; }
    public decimal Bucket1_0_30 { get; set; }
    public decimal Bucket2_31_60 { get; set; }
    public decimal Bucket3_61_90 { get; set; }
    public decimal Bucket4_91_120 { get; set; }
    public decimal Bucket5_Over120 { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string BatchStatus { get; set; } = string.Empty;
}

/// <summary>
/// Hasil terpaginasi tabel detail invoice umur piutang (Halaman B).
/// </summary>
public sealed class ReceivableAgingInvoicePagedResponse
{
    public List<ReceivableAgingInvoiceItemResponse> Items { get; set; } = new();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalCount { get; set; }
    public int TotalPages { get; set; } = 1;
    public ReceivableAgingDetailSummaryResponse Summary { get; set; } = new();
}

