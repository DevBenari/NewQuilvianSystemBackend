using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Controllers;

/// <summary>
/// Endpoint batch migrasi tagihan lama lewat spreadsheet (BE-FIN-079..083, `FIN-API-1.6` F.2,
/// `FIN-DES-093`). BE-FIN-080 membangun `GET /template` (`format=CSV`); BE-FIN-081 menambah
/// `POST /` (unggah) dan `POST /{id}/validate`; BE-FIN-082 menambah `GET /`, `GET /{id}`,
/// `POST /{id}/declare-accounting-opening`, `POST /{id}/approve`, dan `POST /{id}/reject`.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/opening-item-batches")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_OPENING_ITEM_BATCH",
    "Corporate Finance Management Opening Item Batch", "Opening Item Batch",
    AreaName = "Corporate", ControllerName = "FinanceOpeningItemBatch",
    Description = "Migrasi tagihan lama lewat batch spreadsheet", SortOrder = 71)]
[Tags("Corporate / Finance Management / Opening Item Batch")]
public sealed class FinanceOpeningItemBatchesController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly FinanceOpeningItemBatchService _service;

    public FinanceOpeningItemBatchesController(IWebHostEnvironment environment, FinanceOpeningItemBatchService service)
    {
        _environment = environment;
        _service = service;
    }

    /// <summary>Daftar batch migrasi, bersaring jenis dan status.</summary>
    [HttpGet]
    [AccessAction("Read", "Read Opening Item Batch", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceOpeningItemBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OpeningItemBatchResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] OpeningItemBatchPagedQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<OpeningItemBatchResponse>>.Ok(result, "Daftar batch migrasi tagihan lama berhasil diambil."));
    }

    /// <summary>Rincian batch beserta hasil validasi per baris terakhir.</summary>
    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Opening Item Batch", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceOpeningItemBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<OpeningItemBatchDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(ApiResponse<OpeningItemBatchDetailResponse>.Ok(result, "Rincian batch migrasi tagihan lama berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Mengunduh templat sesuai jenis item dan format (FIN-VAL-224). `format=XLSX` kontraktual
    /// sah tetapi templatnya **belum tersedia** — pembaca XLSX menunggu wewenang paket
    /// (`BE-FIN-083`, `FIN-OQ-081`) — sehingga dijawab `503`, bukan `400`: nilainya benar, sistem
    /// yang belum siap memenuhinya (pola sama dengan `FIN-VAL-220`).
    /// </summary>
    [HttpGet("template")]
    [AccessAction("Read", "Read Opening Item Batch", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceOpeningItemBatch", "Read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public IActionResult GetTemplate([FromQuery] OpeningItemBatchTemplateQuery query)
    {
        var format = query.Format?.Trim().ToUpperInvariant();
        if (format is not ("CSV" or "XLSX"))
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Pilih format templat: CSV atau XLSX."));

        var itemKind = query.ItemKind?.Trim().ToUpperInvariant();
        if (itemKind is not (FinOpeningItemBatchItemKinds.Receivable or FinOpeningItemBatchItemKinds.SupplierPayable))
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, "Pilih jenis item: RECEIVABLE atau SUPPLIER_PAYABLE."));

        if (format == "XLSX")
            return StatusCode(StatusCodes.Status503ServiceUnavailable, ApiResponse<object>.Fail(
                StatusCodes.Status503ServiceUnavailable,
                "Templat XLSX belum tersedia — pembaca XLSX menunggu wewenang paket pembaca (FIN-OQ-081). Gunakan format CSV."));

        var fileName = itemKind == FinOpeningItemBatchItemKinds.Receivable
            ? "opening-item-receivable.csv"
            : "opening-item-supplier-payable.csv";

        var physicalPath = Path.Combine(_environment.ContentRootPath, "Storage", "templates", "finance", fileName);
        return PhysicalFile(physicalPath, "text/csv", fileName);
    }

    /// <summary>
    /// Mengunggah spreadsheet migrasi dan membuat batch DRAFT (FIN-VAL-186, 225). Validasi baris
    /// (FIN-VAL-187..191, 226) **belum** dijalankan di sini — panggil `POST /{id}/validate` sesudahnya.
    /// </summary>
    [HttpPost]
    [AccessAction("Create", "Create Opening Item Batch", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceOpeningItemBatch", "Create")]
    [ProducesResponseType(typeof(ApiResponse<OpeningItemBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Upload([FromForm] UploadOpeningItemBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.UploadAsync(request.File, request.ItemKind, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningItemBatchResponse>.Ok(
                await _service.MapWithNameAsync(entity, cancellationToken), "Batch migrasi tagihan lama berhasil diunggah."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Mengunggah ulang berkas pada batch DRAFT (state-transition-matrix.md F.2: DRAFT -> DRAFT). Hasil validasi
    /// sebelumnya dibuang; saldo awal Accounting yang sudah dinyatakan dipertahankan. Batch selain DRAFT dijawab 409.
    /// </summary>
    [HttpPost("{id:guid}/reupload")]
    [AccessAction("Update", "Update Opening Item Batch", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceOpeningItemBatch", "Update")]
    [ProducesResponseType(typeof(ApiResponse<OpeningItemBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Reupload(
        Guid id, [FromForm] ReuploadOpeningItemBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.ReuploadAsync(
                id, request.File, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningItemBatchResponse>.Ok(
                await _service.MapWithNameAsync(entity, cancellationToken), "Berkas batch migrasi tagihan lama berhasil diunggah ulang."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Menjalankan validasi per baris (FIN-VAL-187..191, 226). Batch berpindah ke VALIDATED hanya
    /// bila nol baris bergalat; sebaliknya tetap DRAFT beserta daftar galatnya.
    /// </summary>
    [HttpPost("{id:guid}/validate")]
    [AccessAction("Update", "Update Opening Item Batch", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceOpeningItemBatch", "Update")]
    [ProducesResponseType(typeof(ApiResponse<OpeningItemBatchDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Validate(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var (entity, rows) = await _service.ValidateAsync(id, CurrentUserId(), cancellationToken);
            var message = entity.Status == FinOpeningItemBatchStatuses.Validated
                ? "Validasi selesai — nol baris bergalat, batch berpindah ke VALIDATED."
                : "Validasi selesai — masih ada baris bergalat, batch tetap DRAFT.";
            return Ok(ApiResponse<OpeningItemBatchDetailResponse>.Ok(await _service.MapDetailWithNameAsync(entity, rows, cancellationToken), message));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Menyatakan total saldo awal Accounting dan rujukan dokumennya. Boleh dipanggil dari DRAFT
    /// atau VALIDATED; status batch tidak berubah (state-transition-matrix.md F.2).
    /// </summary>
    [HttpPost("{id:guid}/declare-accounting-opening")]
    [AccessAction("Update", "Update Opening Item Batch", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceOpeningItemBatch", "Update")]
    [ProducesResponseType(typeof(ApiResponse<OpeningItemBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeclareAccountingOpening(
        Guid id, [FromBody] DeclareAccountingOpeningRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.DeclareAccountingOpeningAsync(
                id, request.ExpectedRowVersion, request.DeclaredAccountingOpeningAmount, request.AccountingReferenceDocument,
                CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningItemBatchResponse>.Ok(
                await _service.MapWithNameAsync(entity, cancellationToken), "Saldo awal Accounting berhasil dinyatakan."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Menyetujui batch VALIDATED: melahirkan item piutang/utang beserta mutasi pembukanya dalam
    /// satu transaksi, lalu mengunci batch. Menolak bila total sisa berbeda dari saldo awal
    /// Accounting yang dinyatakan (state-transition-matrix.md F.2).
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Opening Item Batch", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceOpeningItemBatch", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<OpeningItemBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(
        Guid id, [FromBody] ApproveOpeningItemBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.ApproveAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningItemBatchResponse>.Ok(
                await _service.MapWithNameAsync(entity, cancellationToken), "Batch migrasi tagihan lama disetujui dan dikunci."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>Menolak batch. Boleh dipanggil dari DRAFT atau VALIDATED; alasan wajib.</summary>
    [HttpPost("{id:guid}/reject")]
    [AccessAction("Approve", "Approve Opening Item Batch", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceOpeningItemBatch", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<OpeningItemBatchResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Reject(
        Guid id, [FromBody] RejectOpeningItemBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _service.RejectAsync(id, request.ExpectedRowVersion, request.RejectionReason, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningItemBatchResponse>.Ok(await _service.MapWithNameAsync(entity, cancellationToken), "Batch migrasi tagihan lama ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message)),
        OpeningItemBatchConflictException => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message)),
        OpeningItemBatchValidationException => UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message)),
        OpeningItemBatchNotReadyException => StatusCode(StatusCodes.Status503ServiceUnavailable,
            ApiResponse<object>.Fail(StatusCodes.Status503ServiceUnavailable, exception.Message)),
        OpeningItemBatchBadRequestException => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or OpeningItemBatchConflictException or OpeningItemBatchValidationException
        or OpeningItemBatchNotReadyException or OpeningItemBatchBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
