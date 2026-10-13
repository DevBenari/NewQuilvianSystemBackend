using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Collection.Controllers;

/// <summary>
/// Endpoint Ayat Silang (Cross-Entry / Unidentified Payer Receipt).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/collection/cross-entries")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_CROSS_ENTRY", "Corporate Finance Management Cross Entry", "Cross Entry",
    AreaName = "Corporate", ControllerName = "CrossEntry", Description = "Ayat Silang — penerimaan uang asuransi/penjamin yang belum ber-invoice tujuan", SortOrder = 32)]
[Tags("Corporate / Finance Management / Collection / Cross Entry")]
public sealed class FinanceCrossEntriesController : ControllerBase
{
    private readonly FinanceCrossEntryService _service;

    public FinanceCrossEntriesController(FinanceCrossEntryService service)
    {
        _service = service;
    }

    [HttpGet]
    [AccessAction("Read", "Read Cross Entry", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceCrossEntry", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CrossEntryListResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] CrossEntryQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<CrossEntryListResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar Ayat Silang berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Cross Entry", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceCrossEntry", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CrossEntrySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary([FromQuery] CrossEntryQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<CrossEntrySummaryResponse>.Ok(
            await _service.GetSummaryAsync(request, cancellationToken), "Ringkasan data Ayat Silang berhasil diambil."));

    [HttpGet("options")]
    [AccessAction("Read", "Read Cross Entry", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceCrossEntry", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<CrossEntryOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions([FromQuery] CrossEntryOptionQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<CrossEntryOptionResponse>>.Ok(
            await _service.GetOptionsAsync(request, cancellationToken), "Pilihan Ayat Silang untuk pembayaran AR berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Cross Entry", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceCrossEntry", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CrossEntryDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<CrossEntryDetailResponse>.Ok(
                await _service.GetByIdAsync(id, cancellationToken), "Rincian Ayat Silang berhasil diambil."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Cross Entry", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceCrossEntry", "Create")]
    [ProducesResponseType(typeof(ApiResponse<CrossEntryDetailResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCrossEntryRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(request, idempotencyKey, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<CrossEntryDetailResponse>.Ok(result, "Dokumen Ayat Silang berhasil dibuat."));
        }
        catch (Exception ex) when (IsHandled(ex)) { return Failure(ex); }
    }

    [HttpGet("{id:guid}/documents")]
    [AccessAction("Read", "Read Cross Entry", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceCrossEntry", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<CrossEntryDocumentResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDocuments(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<List<CrossEntryDocumentResponse>>.Ok(
                await _service.GetDocumentsAsync(id, cancellationToken), "Daftar dokumen lampiran berhasil diambil."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
    }

    [HttpPost("{id:guid}/documents")]
    [AccessAction("UploadDocument", "Upload Cross Entry Document", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceCrossEntry", "UploadDocument")]
    [ProducesResponseType(typeof(ApiResponse<CrossEntryDocumentResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> UploadDocument(
        Guid id,
        [FromForm] UploadCrossEntryDocumentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var doc = await _service.UploadDocumentAsync(id, request.DocumentName, request.Description, request.File, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<CrossEntryDocumentResponse>.Ok(doc, "Dokumen lampiran berhasil diunggah."));
        }
        catch (Exception ex) when (IsHandled(ex)) { return Failure(ex); }
    }

    [HttpGet("{id:guid}/documents/{documentId:guid}/download")]
    [AccessAction("DownloadDocument", "Download Cross Entry Document", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("FinanceCrossEntry", "DownloadDocument")]
    public async Task<IActionResult> DownloadDocument(Guid id, Guid documentId, CancellationToken cancellationToken)
    {
        try
        {
            var (fileStream, contentType, fileName) = await _service.DownloadDocumentAsync(id, documentId, cancellationToken);
            return File(fileStream, contentType, fileName);
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        FinanceCrossEntryConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        FinanceCrossEntryValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        FinanceCrossEntryBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        FinanceCrossEntryTooLargeException => StatusCode(413, ApiResponse<object>.Fail(413, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or FinanceCrossEntryConflictException
        or FinanceCrossEntryValidationException or FinanceCrossEntryBadRequestException
        or FinanceCrossEntryTooLargeException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
