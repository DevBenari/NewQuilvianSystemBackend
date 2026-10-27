using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// Aggregate ber-lifecycle (transaksi, bukan master data) — perpindahan status lewat aksi
/// POST /{id}/&lt;aksi&gt;, bukan PATCH /{id}/status generik (transaction-endpoint-standard.md).
/// BE-FIN-053 (FIN-DES-073): GET /write-offs adalah permukaan BACA lintas piutang untuk layar
/// "Pemutihan Piutang" — nol aksi baru, pembuatan/persetujuan/penolakan write-off tetap lewat
/// endpoint per-piutang di bawah beserta maker-checker-nya.
/// BE-FIN-055: GET /aging menerima parameter opsional DebtorType (PAYER/PATIENT_GUARANTOR/
/// EMPLOYEE_BENEFIT) — BUKAN "segmen Kasir" seperti dugaan rancangan awal, karena nilai itu tidak
/// ada pada FinReceivable. "Umur Piutang Kasir" pada menu V1 memanggil endpoint ini TANPA
/// saringan apa pun.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/receivables")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIVABLE", "Corporate Finance Management Receivable", "Receivable",
    AreaName = "Corporate", ControllerName = "FinanceReceivable", Description = "Buku piutang Finance — umur, koreksi, penghapusan", SortOrder = 30)]
[Tags("Corporate / Finance Management / Receivable")]
public sealed class FinanceReceivablesController : ControllerBase
{
    private readonly FinanceReceivableService _service;
    private readonly FinanceReceivableBillingDataService _billingDataService;
    private readonly FinanceReceivableClearanceService _clearanceService;
    private readonly FinanceBillingArReportService _billingArReportService;
    private readonly FinanceReceivableAgingReportService _agingReportService;
    private readonly FinanceSettlementService _settlementService;
    private readonly FinanceReceivablePaymentReportService _paymentReportService;
    private readonly FinanceReceivableSeparationNoticeService _separationNoticeService;

    public FinanceReceivablesController(
        FinanceReceivableService service,
        FinanceReceivableBillingDataService billingDataService,
        FinanceReceivableClearanceService clearanceService,
        FinanceBillingArReportService billingArReportService,
        FinanceReceivableAgingReportService agingReportService,
        FinanceSettlementService settlementService,
        FinanceReceivablePaymentReportService paymentReportService,
        FinanceReceivableSeparationNoticeService separationNoticeService)
    {
        _service = service;
        _clearanceService = clearanceService;
        _billingDataService = billingDataService;
        _billingArReportService = billingArReportService;
        _agingReportService = agingReportService;
        _settlementService = settlementService;
        _paymentReportService = paymentReportService;
        _separationNoticeService = separationNoticeService;
    }

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<ReceivableFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter piutang berhasil diambil."));

    // BE-FIN-100 (FR-FIN-219, FIN-DEC-176): parameter debtorType opsional, pola yang sama dengan
    // GET /aging di bawah (BE-FIN-055) — null/kosong berarti seluruh FinReceivable tanpa saringan.
    [HttpGet("summary")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] string? debtorType, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ReceivableSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken, debtorType), "Ringkasan piutang berhasil diambil."));

    [HttpGet("aging")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<ReceivableAgingBucketResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAging([FromQuery] ReceivableAgingQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<ReceivableAgingBucketResult>>.Ok(
            await _service.GetAgingSummaryAsync(request.AsOfDate, cancellationToken, request.DebtorType), "Umur piutang berhasil diambil."));

    // ------------------------------------------------------------------------------------
    // Laporan Aging AR V2 Mengikuti V1 (Daftar Penjamin & Detail Aging per Penjamin)
    // ------------------------------------------------------------------------------------

    [HttpGet("aging/payers")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableAgingPayerPagedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgingPayers([FromQuery] FinanceReceivableAgingPayerQuery query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ReceivableAgingPayerPagedResponse>.Ok(
            await _agingReportService.GetAgingPayersPagedAsync(query, cancellationToken), "Daftar penjamin umur piutang berhasil diambil."));

    [HttpGet("aging/payers/{payerId:guid}/summary")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableAgingDetailSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgingPayerSummary(Guid payerId, [FromQuery] DateOnly? asOfDate, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ReceivableAgingDetailSummaryResponse>.Ok(
            await _agingReportService.GetAgingPayerSummaryAsync(payerId, asOfDate, cancellationToken), "Ringkasan umur piutang penjamin berhasil diambil."));

    [HttpGet("aging/payers/{payerId:guid}/invoices")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableAgingInvoicePagedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgingPayerInvoices(Guid payerId, [FromQuery] ReceivableAgingDetailQuery query, CancellationToken cancellationToken) =>
        Ok(ApiResponse<ReceivableAgingInvoicePagedResponse>.Ok(
            await _agingReportService.GetAgingPayerInvoicesPagedAsync(payerId, query, cancellationToken), "Detail invoice umur piutang penjamin berhasil diambil."));

    [HttpGet("aging/payers/{payerId:guid}/export/excel")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportAgingPayerExcel(Guid payerId, [FromQuery] ReceivableAgingDetailQuery query, CancellationToken cancellationToken)
    {
        var fileBytes = await _agingReportService.ExportAgingPayerExcelAsync(payerId, query, cancellationToken);
        var fileName = $"Laporan_Aging_AR_{payerId}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
        return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("aging/payers/{payerId:guid}/export/pdf")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportAgingPayerPdf(Guid payerId, [FromQuery] ReceivableAgingDetailQuery query, CancellationToken cancellationToken)
    {
        var fileBytes = await _agingReportService.ExportAgingPayerPdfAsync(payerId, query, cancellationToken);
        var fileName = $"Laporan_Aging_AR_{payerId}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
        return File(fileBytes, "application/pdf", fileName);
    }

    [HttpGet]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReceivableResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] ReceivableQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<ReceivableResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Piutang berhasil diambil."));

    // Data Tagihan (Finance > Transaksi A/R > Tagihan/Billing). Sub-resource baca seperti summary dan aging;
    // memakai hak akses FinanceReceivable : Read yang sudah terdaftar, tanpa action baru. Daftar dan ringkasan
    // keluar dari satu query yang sama (FinanceReceivableBillingDataService).
    // MURNI BACA: tidak lagi menyinkronkan atau mengolah fakta AR dari Billing. Fakta yang belum menjadi piutang
    // dilaporkan pada PendingFacts, dan diolah lewat POST billing-intake/sync dan POST billing-intake/process-pending-ar.
    [HttpGet("billing-data")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BillingDataPagedResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBillingData([FromQuery] BillingDataQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _billingDataService.GetAsync(request, cancellationToken);
            return Ok(ApiResponse<BillingDataPagedResponse>.Ok(result, result.Notice ?? "Data tagihan berhasil diambil."));
        }
        catch (BillingDataBadRequestException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(400, exception.Message));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(404, exception.Message));
        }
    }

    // Isi field pilihan asuransi/perusahaan pada Data Tagihan kategori company. Ringan: dibatasi Limit, dicari lewat Search.
    [HttpGet("billing-data/payer-options")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<BillingDataPayerOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingDataPayerOptions(
        [FromQuery] BillingDataPayerOptionQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<BillingDataPayerOptionResponse>>.Ok(
            await _billingDataService.GetPayerOptionsAsync(request, cancellationToken), "Pilihan asuransi/perusahaan berhasil diambil."));

    // Laporan AR (Finance > Transaksi A/R > Laporan & Monitoring > Report AR).
    // Read-only report yang menampilkan daftar invoice Billing lunas per pasien dikelompokkan menurut penjamin (Payer/Asuransi/Perusahaan) ASC.
    [HttpGet("reports/billing-ar")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BillingArReportPagedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingArReport(
        [FromQuery] BillingArReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _billingArReportService.GetReportAsync(request, cancellationToken);
        return Ok(ApiResponse<BillingArReportPagedResponse>.Ok(result, "Laporan AR berhasil diambil."));
    }

    [HttpGet("reports/billing-ar/payer-options")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<BillingDataPayerOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBillingArReportPayerOptions(
        [FromQuery] string? search, [FromQuery] int limit = 20, CancellationToken cancellationToken = default)
    {
        var result = await _billingArReportService.GetPayerOptionsAsync(search, limit, cancellationToken);
        return Ok(ApiResponse<List<BillingDataPayerOptionResponse>>.Ok(result, "Pilihan penjamin berhasil diambil."));
    }

    [HttpGet("reports/billing-ar/export/excel")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportBillingArExcel(
        [FromQuery] BillingArReportQuery request, CancellationToken cancellationToken)
    {
        var bytes = await _billingArReportService.ExportExcelAsync(request, cancellationToken);
        var fileName = $"Laporan_AR_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("reports/billing-ar/export/pdf")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportBillingArPdf(
        [FromQuery] BillingArReportQuery request, CancellationToken cancellationToken)
    {
        var bytes = await _billingArReportService.ExportPdfAsync(request, cancellationToken);
        var fileName = $"Laporan_AR_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    // ------------------------------------------------------------------------------------
    // Laporan Pembayaran AR (Finance > Transaksi A/R > Laporan & Monitoring > Laporan Pembayaran AR)
    // Read-model pembayaran yang dialokasikan ke Invoice AR tertentu dari Kas/Bank maupun Ayat Silang
    // ------------------------------------------------------------------------------------

    [HttpGet("reports/payment-ar")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentArReportPagedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentArReport(
        [FromQuery] PaymentArReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _paymentReportService.GetPaymentArReportAsync(request, cancellationToken);
        return Ok(ApiResponse<PaymentArReportPagedResponse>.Ok(result, "Laporan pembayaran AR berhasil diambil."));
    }

    [HttpGet("reports/payment-ar/summary")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentArReportSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentArSummary(
        [FromQuery] PaymentArReportQuery request, CancellationToken cancellationToken)
    {
        var result = await _paymentReportService.GetPaymentArSummaryAsync(request, cancellationToken);
        return Ok(ApiResponse<PaymentArReportSummaryResponse>.Ok(result, "Ringkasan laporan pembayaran AR berhasil diambil."));
    }

    [HttpGet("reports/payment-ar/{receiptId:guid}/{batchId:guid}/detail")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentArDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentArDetail(
        Guid receiptId, Guid batchId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _paymentReportService.GetPaymentArDetailAsync(receiptId, batchId, cancellationToken);
            return Ok(ApiResponse<PaymentArDetailResponse>.Ok(result, "Detail laporan pembayaran AR berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(404, ex.Message));
        }
    }

    [HttpGet("reports/payment-ar/{paymentKey}/detail")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PaymentArDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentArDetailByKey(
        string paymentKey, CancellationToken cancellationToken)
    {
        var parts = paymentKey.Split('_');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var receiptId) || !Guid.TryParse(parts[1], out var batchId))
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Format identifier pembayaran tidak valid. Gunakan format '{ReceiptId}_{BatchId}'."));
        }

        try
        {
            var result = await _paymentReportService.GetPaymentArDetailAsync(receiptId, batchId, cancellationToken);
            return Ok(ApiResponse<PaymentArDetailResponse>.Ok(result, "Detail laporan pembayaran AR berhasil diambil."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(404, ex.Message));
        }
    }

    [HttpGet("reports/payment-ar/export/excel")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportPaymentArExcel(
        [FromQuery] PaymentArReportQuery request, CancellationToken cancellationToken)
    {
        var bytes = await _paymentReportService.ExportListExcelAsync(request, cancellationToken);
        var fileName = $"Laporan_Pembayaran_AR_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("reports/payment-ar/export/pdf")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportPaymentArPdf(
        [FromQuery] PaymentArReportQuery request, CancellationToken cancellationToken)
    {
        var bytes = await _paymentReportService.ExportListPdfAsync(request, cancellationToken);
        var fileName = $"Laporan_Pembayaran_AR_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    [HttpGet("reports/payment-ar/{receiptId:guid}/{batchId:guid}/export/excel")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportPaymentArDetailExcel(
        Guid receiptId, Guid batchId, CancellationToken cancellationToken)
    {
        var bytes = await _paymentReportService.ExportDetailExcelAsync(receiptId, batchId, cancellationToken);
        var fileName = $"Detail_Pembayaran_AR_{batchId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("reports/payment-ar/{receiptId:guid}/{batchId:guid}/export/pdf")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportPaymentArDetailPdf(
        Guid receiptId, Guid batchId, CancellationToken cancellationToken)
    {
        var bytes = await _paymentReportService.ExportDetailPdfAsync(receiptId, batchId, cancellationToken);
        var fileName = $"Detail_Pembayaran_AR_{batchId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    [HttpGet("reports/payment-ar/{paymentKey}/export/excel")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportPaymentArDetailExcelByKey(
        string paymentKey, CancellationToken cancellationToken)
    {
        var parts = paymentKey.Split('_');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var receiptId) || !Guid.TryParse(parts[1], out var batchId))
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Format identifier pembayaran tidak valid."));
        }

        var bytes = await _paymentReportService.ExportDetailExcelAsync(receiptId, batchId, cancellationToken);
        var fileName = $"Detail_Pembayaran_AR_{batchId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("reports/payment-ar/{paymentKey}/export/pdf")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportPaymentArDetailPdfByKey(
        string paymentKey, CancellationToken cancellationToken)
    {
        var parts = paymentKey.Split('_');
        if (parts.Length != 2 || !Guid.TryParse(parts[0], out var receiptId) || !Guid.TryParse(parts[1], out var batchId))
        {
            return BadRequest(ApiResponse<object>.Fail(400, "Format identifier pembayaran tidak valid."));
        }

        var bytes = await _paymentReportService.ExportDetailPdfAsync(receiptId, batchId, cancellationToken);
        var fileName = $"Detail_Pembayaran_AR_{batchId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    // Settlement AR (Finance > Transaksi A/R > Pembayaran & Pelunasan > Settlement AR)
    [HttpGet("settlement")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<FinanceSettlementPagedResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettlement([FromQuery] FinanceSettlementQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<FinanceSettlementPagedResponse>.Ok(
            await _settlementService.GetSettlementPagedAsync(request, cancellationToken), "Daftar settlement AR berhasil diambil."));

    [HttpGet("settlement/summary")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<FinanceSettlementSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettlementSummary([FromQuery] FinanceSettlementQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<FinanceSettlementSummaryResponse>.Ok(
            await _settlementService.GetSettlementSummaryAsync(request, cancellationToken), "Ringkasan settlement AR berhasil diambil."));

    [HttpGet("settlement/{id:guid}")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<FinanceSettlementDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSettlementDetail(Guid id, [FromQuery] DateOnly? startDate, [FromQuery] DateOnly? endDate, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _settlementService.GetSettlementDetailAsync(id, startDate, endDate, cancellationToken);
            return Ok(ApiResponse<FinanceSettlementDetailResponse>.Ok(result, "Detail settlement AR berhasil diambil."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(404, exception.Message));
        }
    }

    [HttpGet("settlement/export/excel")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportSettlementExcel([FromQuery] FinanceSettlementQuery request, CancellationToken cancellationToken)
    {
        var bytes = await _settlementService.ExportExcelAsync(request, cancellationToken);
        var fileName = $"Settlement_AR_{request.Category}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx";
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }

    [HttpGet("settlement/export/pdf")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    public async Task<IActionResult> ExportSettlementPdf([FromQuery] FinanceSettlementQuery request, CancellationToken cancellationToken)
    {
        var bytes = await _settlementService.ExportPdfAsync(request, cancellationToken);
        var fileName = $"Settlement_AR_{request.Category}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        return File(bytes, "application/pdf", fileName);
    }

    [HttpGet("write-offs")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReceivableWriteOffRowResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWriteOffs([FromQuery] ReceivableWriteOffQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<ReceivableWriteOffRowResponse>>.Ok(
            await _service.GetWriteOffsAsync(request, cancellationToken), "Daftar penghapusan piutang berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<ReceivableDetailResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail piutang berhasil diambil. Menampilkan rincian, dokumen, koreksi, dan penghapusan — termasuk penelusuran ke tagihan asal (InvoiceId).")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpGet("{id:guid}/documents")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableDocumentsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<ReceivableDocumentsResponse>.Ok(await _service.GetDocumentsAsync(id, cancellationToken), "Daftar dokumen piutang berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpGet("{id:guid}/movements")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReceivableMovementResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMovements(Guid id, [FromQuery] ReceivableMovementQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetMovementsPagedAsync(id, query, cancellationToken);
            return Ok(ApiResponse<PagedResult<ReceivableMovementResponse>>.Ok(result, "Riwayat mutasi piutang berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }


    [HttpPost("{id:guid}/adjustments")]
    [AccessAction("RequestAdjustment", "Request Receivable Adjustment", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceReceivable", "RequestAdjustment")]
    public async Task<IActionResult> RequestAdjustment(Guid id, [FromBody] RequestReceivableAdjustmentRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RequestAdjustmentAsync(id, request.Direction, request.Amount, request.Reason, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<ReceivableAdjustmentResponse>.Ok(FinanceReceivableService.MapAdjustment(result), "Pengajuan koreksi piutang berhasil dibuat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/adjustments/{adjustmentId:guid}/approve")]
    [AccessAction("ApproveAdjustment", "Approve Receivable Adjustment", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceReceivable", "ApproveAdjustment")]
    public async Task<IActionResult> ApproveAdjustment(Guid id, Guid adjustmentId, [FromBody] DecideReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveAdjustmentAsync(adjustmentId, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableAdjustmentResponse>.Ok(FinanceReceivableService.MapAdjustment(result), "Koreksi piutang berhasil disetujui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/adjustments/{adjustmentId:guid}/reject")]
    [AccessAction("ApproveAdjustment", "Approve Receivable Adjustment", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceReceivable", "ApproveAdjustment")]
    public async Task<IActionResult> RejectAdjustment(Guid id, Guid adjustmentId, [FromBody] DecideReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectAdjustmentAsync(adjustmentId, request.ExpectedRowVersion, request.RejectionReason ?? string.Empty, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableAdjustmentResponse>.Ok(FinanceReceivableService.MapAdjustment(result), "Koreksi piutang berhasil ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/write-offs")]
    [AccessAction("RequestWriteOff", "Request Receivable Write-Off", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("FinanceReceivable", "RequestWriteOff")]
    public async Task<IActionResult> RequestWriteOff(Guid id, [FromBody] RequestReceivableWriteOffRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RequestWriteOffAsync(id, request.Amount, request.Reason, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<ReceivableWriteOffResponse>.Ok(FinanceReceivableService.MapWriteOff(result), "Pengajuan penghapusan piutang berhasil dibuat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/write-offs/{writeOffId:guid}/approve")]
    [AccessAction("ApproveWriteOff", "Approve Receivable Write-Off", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinanceReceivable", "ApproveWriteOff")]
    public async Task<IActionResult> ApproveWriteOff(Guid id, Guid writeOffId, [FromBody] DecideReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ApproveWriteOffAsync(writeOffId, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffResponse>.Ok(FinanceReceivableService.MapWriteOff(result), "Penghapusan piutang berhasil disetujui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/write-offs/{writeOffId:guid}/reject")]
    [AccessAction("ApproveWriteOff", "Approve Receivable Write-Off", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinanceReceivable", "ApproveWriteOff")]
    public async Task<IActionResult> RejectWriteOff(Guid id, Guid writeOffId, [FromBody] DecideReceivableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.RejectWriteOffAsync(writeOffId, request.ExpectedRowVersion, request.RejectionReason ?? string.Empty, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableWriteOffResponse>.Ok(FinanceReceivableService.MapWriteOff(result), "Penghapusan piutang berhasil ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    // BE-FIN-096, FIN-API-1.9 O.4: dua endpoint status bebas tanggungan menempel di sini, BUKAN
    // controller baru — ControllerName FinanceReceivable sudah terdaftar, nol resource baru
    // (02-backend-architecture.md O.5.9). Hak akses sama persis dengan endpoint baca lain di atas.
    [HttpGet("clearance/{benefitOwnerId:guid}")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeClearanceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClearance(Guid benefitOwnerId, CancellationToken cancellationToken)
    {
        var result = await _clearanceService.GetAsync(benefitOwnerId, cancellationToken);
        return Ok(ApiResponse<EmployeeClearanceResponse>.Ok(
            result, result.IsCleared ? "Pegawai bebas tanggungan." : "Pegawai masih memiliki tanggungan."));
    }

    [HttpGet("clearance")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<EmployeeClearanceResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetClearanceBatch([FromQuery] ClearanceBatchQuery request, CancellationToken cancellationToken)
    {
        if (request.BenefitOwnerIds.Count == 0 && request.BenefitOwnerWorkforceProfileIds.Count == 0)
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest,
                "BenefitOwnerIds atau BenefitOwnerWorkforceProfileIds wajib diisi."));

        var result = new List<EmployeeClearanceResponse>();
        if (request.BenefitOwnerIds.Count > 0)
            result.AddRange(await _clearanceService.GetBatchAsync(request.BenefitOwnerIds, cancellationToken));
        if (request.BenefitOwnerWorkforceProfileIds.Count > 0)
            result.AddRange(await _clearanceService.GetBatchByWorkforceAsync(request.BenefitOwnerWorkforceProfileIds, cancellationToken));
        return Ok(ApiResponse<List<EmployeeClearanceResponse>>.Ok(result, "Status bebas tanggungan berhasil diambil."));
    }

    // INT-HR-FIN-003 §P.5 (FIN-DEC-169, FIN-DEC-194): Notifikasi pemisahan pegawai dari HR ke Finance.
    [HttpPost("separation-notice")]
    [AccessAction("Update", "Update Receivable", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("FinanceReceivable", "Update")]
    [ProducesResponseType(typeof(ApiResponse<SeparationNoticeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ReceiveSeparationNotice([FromBody] SeparationNoticeRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _separationNoticeService.ProcessNoticeAsync(request, cancellationToken);
            return Ok(ApiResponse<SeparationNoticeResponse>.Ok(result, result.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(400, ex.Message));
        }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        ReceivableConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        ReceivableValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        ReceivableBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or ReceivableConflictException or ReceivableValidationException or ReceivableBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
