using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Controllers;

/// <summary>
/// Empat laporan Purchasing/AP read-only (BE-FIN-037, FIN-API-1.1 §B.6, FIN-PERM-1.1 §B.5).
/// Seluruh endpoint GET — nol perintah pengubah di controller ini (FIN-DES-044).
/// /aging SENGAJA dikeluarkan (FIN-DEC-059) — layar AP memakai GET api/finance/payable/aging
/// (FinanceApController) yang sudah menghitung umur FinSupplierPayable.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/purchasing/reports")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_PURCHASING_REPORT", "Corporate Finance Management Purchasing Report", "Purchasing Report",
    AreaName = "Corporate", ControllerName = "PurchasingReport",
    Description = "Laporan Purchasing/AP Finance — rekap periodik, Tukar Faktur, jatuh tempo, rekonsiliasi",
    SortOrder = 47)]
[Tags("Corporate / Finance Management / Purchasing / Reports")]
public sealed class FinancePurchasingReportsController : ControllerBase
{
    private readonly FinancePurchasingReportService _service;

    public FinancePurchasingReportsController(FinancePurchasingReportService service)
        => _service = service;

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/summary
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Rekap Purchasing/AP periodik: jumlah dokumen per kelompok, total nilai, dan saldo utang.
    /// Filter SupplierId/PeriodFrom/PeriodTo semuanya opsional.
    /// </summary>
    [HttpGet("summary")]
    [AccessAction("Read", "Read Purchasing Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePurchasingReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PurchasingSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(
        [FromQuery] PurchasingSummaryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetSummaryAsync(query, cancellationToken);
        return Ok(ApiResponse<PurchasingSummaryResponse>.Ok(result, "Rekap Purchasing/AP berhasil diambil."));
    }

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/invoice-exchanges
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Laporan Tukar Faktur beserta status keterkaitan ke Purchasing Invoice.
    /// Tukar Faktur yang belum terhubung (RECEIVED) muncul dengan kolom invoice bernilai null.
    /// </summary>
    [HttpGet("invoice-exchanges")]
    [AccessAction("Read", "Read Purchasing Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePurchasingReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<InvoiceExchangeReportResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInvoiceExchanges(
        [FromQuery] InvoiceExchangeReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetInvoiceExchangesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<InvoiceExchangeReportResponse>>.Ok(result, "Laporan Tukar Faktur berhasil diambil."));
    }

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/due-dates
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Laporan Purchasing Invoice mendekati atau melewati jatuh tempo.
    /// EstimatedDueDate dari Tukar Faktur; ActualDueDate dari FinSupplierPayable bila ada.
    /// DaysUntilDue: positif = masih N hari; negatif = sudah N hari lewat jatuh tempo.
    /// </summary>
    [HttpGet("due-dates")]
    [AccessAction("Read", "Read Purchasing Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePurchasingReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DueDateReportResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDueDates(
        [FromQuery] DueDateReportQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetDueDatesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DueDateReportResponse>>.Ok(result, "Laporan jatuh tempo Purchasing Invoice berhasil diambil."));
    }

    // ──────────────────────────────────────────────────────────────────────────────────
    // GET /purchasing/reports/reconciliation
    // ──────────────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Rekonsiliasi: Tukar Faktur berstatus RECEIVED yang belum menghasilkan Purchasing Invoice.
    /// Tujuan: membantu staf AP mendeteksi dokumen yang "nyangkut" sebelum jatuh tempo.
    /// DaysUntilDue: positif = masih N hari; negatif = sudah lewat.
    /// </summary>
    [HttpGet("reconciliation")]
    [AccessAction("Read", "Read Purchasing Report", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePurchasingReport", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReconciliationResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReconciliation(
        [FromQuery] ReconciliationQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetReconciliationAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ReconciliationResponse>>.Ok(result, "Laporan rekonsiliasi Tukar Faktur berhasil diambil."));
    }
}
