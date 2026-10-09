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

    public FinanceReceivablesController(
        FinanceReceivableService service,
        FinanceReceivableBillingDataService billingDataService)
    {
        _service = service;
        _billingDataService = billingDataService;
    }

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<ReceivableFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter piutang berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<ReceivableSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan piutang berhasil diambil."));

    [HttpGet("aging")]
    [AccessAction("Read", "Read Receivable", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivable", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<ReceivableAgingBucketResult>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAging([FromQuery] ReceivableAgingQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<ReceivableAgingBucketResult>>.Ok(
            await _service.GetAgingSummaryAsync(request.AsOfDate, cancellationToken, request.DebtorType), "Umur piutang berhasil diambil."));

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
            var result = await _billingDataService.GetAsync(request, cancellationToken, CurrentUserId());
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
