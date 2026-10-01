namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;

// ===========================================================================================
// BE-FIN-037 — Empat laporan Purchasing/AP (FIN-API-1.1 §B.6, FIN-DES-044, FIN-DEC-059)
// Seluruh DTO pada file ini READ-ONLY; nol tabel laporan baru (FIN-DES-044).
// /aging SENGAJA dikeluarkan — FIN-DEC-059 mencabut endpoint ini dari kontrak; layar AP
// memakai GET api/finance/payable/aging (FinanceApController) yang sudah berjalan.
// ===========================================================================================

// ──────────────────────────────────────────────────────────────────────────────────────────
// GET /purchasing/reports/summary  →  PurchasingSummaryResponse
// ──────────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Query penyaring untuk rekap Purchasing/AP periodik (FIN-API-1.1 §B.6).
/// Kedua tanggal nullable agar pengguna bebas memilih rentang terbuka di salah satu ujung.
/// </summary>
public sealed class PurchasingSummaryQuery
{
    public DateOnly? PeriodFrom { get; set; }
    public DateOnly? PeriodTo   { get; set; }
    public Guid?     SupplierId { get; set; }
}

/// <summary>
/// Rekap Purchasing/AP satu periode: jumlah dokumen per status, total nilai, dan saldo utang.
/// Seluruh nominal dalam Rupiah (tidak ada multi-currency di modul ini).
/// </summary>
public sealed class PurchasingSummaryResponse
{
    // ── Purchase Order ──
    public int    PurchaseOrderCount   { get; set; }
    public decimal PurchaseOrderTotal  { get; set; }

    // ── Tanda Terima Barang ──
    public int    GoodsReceiptCount    { get; set; }

    // ── Tukar Faktur ──
    public int    InvoiceExchangeCount  { get; set; }
    /// <summary>
    /// Tukar Faktur berstatus RECEIVED yang belum terhubung ke Purchasing Invoice.
    /// Angka ini adalah dasar utama laporan /reconciliation.
    /// </summary>
    public int    PendingExchangeCount  { get; set; }

    // ── Purchasing Invoice ──
    public int    PurchasingInvoiceCount   { get; set; }
    public decimal PurchasingInvoiceTotal  { get; set; }
    /// <summary>
    /// Invoice berstatus APPROVED — sudah menjadi utang supplier.
    /// </summary>
    public int    ApprovedInvoiceCount     { get; set; }
    public decimal ApprovedInvoiceTotal    { get; set; }

    // ── Utang Supplier dari Purchasing Invoice ──
    /// <summary>
    /// Saldo utang yang bersumber dari Purchasing Invoice periode ini.
    /// Dihitung dari FinSupplierPayable.OutstandingAmount di mana SourcePurchasingInvoiceId terisi.
    /// </summary>
    public decimal OutstandingPayableTotal { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────────────────
// GET /purchasing/reports/invoice-exchanges  →  PagedResult<InvoiceExchangeReportResponse>
// ──────────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Query penyaring untuk laporan Tukar Faktur beserta status keterkaitan ke Purchasing Invoice.
/// </summary>
public sealed class InvoiceExchangeReportQuery
{
    public int      PageNumber  { get; set; } = 1;
    public int      PageSize    { get; set; } = 25;
    public Guid?    SupplierId  { get; set; }
    /// <summary>
    /// Filter status Tukar Faktur: RECEIVED, LINKED_TO_INVOICE, atau CANCELLED.
    /// </summary>
    public string?  Status      { get; set; }
    public DateOnly? ReceivedDateFrom { get; set; }
    public DateOnly? ReceivedDateTo   { get; set; }
}

/// <summary>
/// Satu baris pada laporan Tukar Faktur (BE-FIN-037, FIN-API-1.1 §B.6).
/// </summary>
public sealed class InvoiceExchangeReportResponse
{
    public Guid    Id                    { get; set; }
    public string  ExchangeNumber        { get; set; } = string.Empty;
    public Guid    SupplierId            { get; set; }
    public string  SupplierName          { get; set; } = string.Empty;
    public Guid?   PurchaseOrderId       { get; set; }
    public string? PONumber              { get; set; }
    public Guid?   GoodsReceiptId        { get; set; }
    public string  SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate  { get; set; }
    public DateOnly ReceivedDate         { get; set; }
    public DateOnly EstimatedDueDate     { get; set; }
    public string  Status                { get; set; } = string.Empty;

    // Informasi keterkaitan ke Purchasing Invoice (null bila belum terhubung)
    public Guid?   PurchasingInvoiceId     { get; set; }
    public string? PurchasingInvoiceNumber { get; set; }
    public string? PurchasingInvoiceStatus { get; set; }
    public decimal? PurchasingInvoiceTotal { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────────────────
// GET /purchasing/reports/due-dates  →  PagedResult<DueDateReportResponse>
// ──────────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Query penyaring untuk laporan Purchasing Invoice mendekati/melewati jatuh tempo.
/// Bila hanya DueDateTo yang diisi, menampilkan semua invoice yang jatuh tempo hingga tanggal itu.
/// </summary>
public sealed class DueDateReportQuery
{
    public int       PageNumber   { get; set; } = 1;
    public int       PageSize     { get; set; } = 25;
    public Guid?     SupplierId   { get; set; }
    /// <summary>
    /// Filter status invoice. Default: APPROVED (yang sudah menjadi utang).
    /// </summary>
    public string?   Status       { get; set; }
    public DateOnly? DueDateFrom  { get; set; }
    public DateOnly? DueDateTo    { get; set; }
    /// <summary>
    /// Bila true, hanya tampilkan invoice yang EstimatedDueDate-nya sudah lewat hari ini.
    /// </summary>
    public bool      OverdueOnly  { get; set; } = false;
}

/// <summary>
/// Satu baris pada laporan jatuh tempo Purchasing Invoice (BE-FIN-037, FIN-API-1.1 §B.6).
/// EstimatedDueDate berasal dari Tukar Faktur yang terhubung (FIN-DEC-051); DueDate dari utang
/// supplier aktual bila sudah ada (FinSupplierPayable.DueDate).
/// </summary>
public sealed class DueDateReportResponse
{
    public Guid    PurchasingInvoiceId     { get; set; }
    public string  PurchasingInvoiceNumber { get; set; } = string.Empty;
    public Guid    SupplierId              { get; set; }
    public string  SupplierName            { get; set; } = string.Empty;
    public decimal TotalAmount             { get; set; }
    public string  Status                  { get; set; } = string.Empty;
    public string  ApprovalTier            { get; set; } = string.Empty;
    public DateTimeOffset? ApprovedAt      { get; set; }

    // Jatuh tempo dari Tukar Faktur (dihitung ReceivedDate + PaymentTermDays)
    public DateOnly? EstimatedDueDate      { get; set; }

    // Jatuh tempo aktual dari utang supplier (bila invoice sudah APPROVED → FinSupplierPayable)
    public DateOnly? ActualDueDate         { get; set; }

    /// <summary>
    /// Selisih hari dari hari ini: positif = masih N hari lagi; negatif = sudah N hari lewat.
    /// Dihitung service dari tanggal paling definitif yang tersedia (ActualDueDate ?? EstimatedDueDate).
    /// </summary>
    public int?    DaysUntilDue            { get; set; }

    public Guid?   SupplierPayableId       { get; set; }
    public decimal? OutstandingAmount      { get; set; }
}

// ──────────────────────────────────────────────────────────────────────────────────────────
// GET /purchasing/reports/reconciliation  →  PagedResult<ReconciliationResponse>
// ──────────────────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Query penyaring untuk rekonsiliasi: Tukar Faktur yang belum menghasilkan Purchasing Invoice.
/// </summary>
public sealed class ReconciliationQuery
{
    public int       PageNumber        { get; set; } = 1;
    public int       PageSize          { get; set; } = 25;
    public Guid?     SupplierId        { get; set; }
    public DateOnly? ReceivedDateFrom  { get; set; }
    public DateOnly? ReceivedDateTo    { get; set; }
    /// <summary>
    /// Bila true, hanya tampilkan Tukar Faktur yang EstimatedDueDate-nya sudah lewat hari ini.
    /// </summary>
    public bool      OverdueOnly       { get; set; } = false;
}

/// <summary>
/// Satu baris pada laporan rekonsiliasi (BE-FIN-037, FIN-API-1.1 §B.6).
/// Hanya menampilkan Tukar Faktur berstatus RECEIVED — belum terhubung ke Purchasing Invoice.
/// Tujuan: membantu staf AP mendeteksi dokumen yang "nyangkut" sebelum jatuh tempo.
/// </summary>
public sealed class ReconciliationResponse
{
    public Guid    InvoiceExchangeId     { get; set; }
    public string  ExchangeNumber        { get; set; } = string.Empty;
    public Guid    SupplierId            { get; set; }
    public string  SupplierName          { get; set; } = string.Empty;
    public Guid?   PurchaseOrderId       { get; set; }
    public string? PONumber              { get; set; }
    public string  SupplierInvoiceNumber { get; set; } = string.Empty;
    public DateOnly SupplierInvoiceDate  { get; set; }
    public DateOnly ReceivedDate         { get; set; }
    public DateOnly EstimatedDueDate     { get; set; }

    /// <summary>
    /// Selisih hari dari hari ini: positif = masih N hari lagi jatuh tempo; negatif = sudah lewat.
    /// </summary>
    public int     DaysUntilDue          { get; set; }
}
