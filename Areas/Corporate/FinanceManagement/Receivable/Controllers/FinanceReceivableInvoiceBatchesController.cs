using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// Batch Tagihan AR (BE-FIN-039, FIN-API-1.1 §B.7, FIN-PERM-1.1 §B.5). Aggregate ber-lifecycle —
/// perpindahan status lewat aksi POST /{id}/&lt;aksi&gt;, bukan PATCH /{id}/status generik
/// (transaction-endpoint-standard.md). Resource hak akses memakai nama kanonikal penuh
/// `FinanceReceivableInvoiceBatch` sesuai `permission-audit-matrix.md` dan `FIN-DES-062` —
/// bukan nama pendek `Receivable` yang masih dipakai controller legacy sebelum `FIN-CQ-08`.
/// BE-FIN-052 (FIN-DEC-097, FIN-DES-070/071): tiga aksi klaim penjamin (claim/verify,
/// claim/approve, claim/close) menulis sumbu KEDUA (ClaimStatus) yang terpisah dari Status di
/// atas — lihat state-transition-matrix.md §D.1. Memakai FinanceReceivableInvoiceBatch : Update
/// yang sudah terdaftar, NOL action baru.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/receivable-invoice-batches")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIVABLE_INVOICE_BATCH", "Corporate Finance Management Receivable Invoice Batch", "Receivable Invoice Batch",
    AreaName = "Corporate", ControllerName = "ReceivableInvoiceBatch", Description = "Batch Tagihan AR — penggabungan piutang satu penjamin menjadi satu dokumen tagihan resmi", SortOrder = 31)]
[Tags("Corporate / Finance Management / Receivable Invoice Batch")]
public sealed class FinanceReceivableInvoiceBatchesController : ControllerBase
{
    private readonly FinanceReceivableInvoiceBatchService _service;
    public FinanceReceivableInvoiceBatchesController(FinanceReceivableInvoiceBatchService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Receivable Invoice Batch", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ReceivableInvoiceBatchResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] ReceivableInvoiceBatchQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<ReceivableInvoiceBatchResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar Batch Tagihan AR berhasil diambil."));

    [HttpGet("eligible-receivables")]
    [AccessAction("Read", "Read Receivable Invoice Batch", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<EligibleReceivableResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEligibleReceivables([FromQuery] EligibleReceivableQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<List<EligibleReceivableResponse>>.Ok(
            await _service.GetEligibleReceivablesAsync(request.DebtorReferenceId, cancellationToken),
            "Daftar piutang yang memenuhi syarat digabung berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Receivable Invoice Batch", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableInvoiceBatchDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<ReceivableInvoiceBatchDetailResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Rincian Batch Tagihan AR berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpGet("{id:guid}/document")]
    [AccessAction("Read", "Read Receivable Invoice Batch", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ReceivableInvoiceBatchDocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDocument(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<ReceivableInvoiceBatchDocumentResponse>.Ok(await _service.GetDocumentAsync(id, CurrentUserId(), cancellationToken), "Dokumen tagihan gabungan berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Receivable Invoice Batch", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateReceivableInvoiceBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _service.CreateAsync(request.PeriodStart, request.PeriodEnd, request.ReceivableIds, CurrentUserId(), cancellationToken);
            return StatusCode(201, ApiResponse<ReceivableInvoiceBatchResponse>.Ok(Map(batch), "Batch Tagihan AR berhasil dibuat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/issue")]
    [AccessAction("Issue", "Issue Receivable Invoice Batch", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Issue")]
    public async Task<IActionResult> Issue(Guid id, [FromBody] ReceivableInvoiceBatchRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _service.IssueAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableInvoiceBatchResponse>.Ok(Map(batch), "Batch Tagihan AR berhasil diterbitkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Update", "Update Receivable Invoice Batch", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Update")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ReceivableInvoiceBatchRowVersionRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _service.CancelAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableInvoiceBatchResponse>.Ok(Map(batch), "Batch Tagihan AR berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    // ------------------------------------------------------------------------------------
    // Sumbu klaim penjamin (BE-FIN-052, FIN-API-1.3 §D.1). Ketiganya memakai
    // FinanceReceivableInvoiceBatch : Update yang sudah terdaftar — NOL action baru (FIN-DES-072).
    // ------------------------------------------------------------------------------------

    [HttpPost("{id:guid}/claim/verify")]
    [AccessAction("Update", "Update Receivable Invoice Batch", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Update")]
    public async Task<IActionResult> ClaimVerify(Guid id, [FromBody] ClaimVerifyRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _service.VerifyClaimAsync(
                id, request.ExpectedRowVersion, request.PayerClaimReference, request.ClaimNote, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableInvoiceBatchResponse>.Ok(Map(batch), "Klaim ditandai sudah diverifikasi penjamin."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/claim/approve")]
    [AccessAction("Update", "Update Receivable Invoice Batch", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Update")]
    public async Task<IActionResult> ClaimApprove(Guid id, [FromBody] ClaimApproveRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _service.ApproveClaimAsync(
                id, request.ExpectedRowVersion, request.ApprovedAmount!.Value, request.PayerClaimReference,
                request.ClaimNote, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableInvoiceBatchResponse>.Ok(Map(batch), "Nominal persetujuan klaim berhasil dicatat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/claim/close")]
    [AccessAction("Update", "Update Receivable Invoice Batch", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceReceivableInvoiceBatch", "Update")]
    public async Task<IActionResult> ClaimClose(Guid id, [FromBody] ClaimCloseRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var batch = await _service.CloseClaimAsync(id, request.ExpectedRowVersion, request.ClaimNote, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ReceivableInvoiceBatchResponse>.Ok(Map(batch), "Klaim berhasil ditutup."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private static ReceivableInvoiceBatchResponse Map(FinReceivableInvoiceBatch batch) => new()
    {
        Id = batch.Id,
        BatchNumber = batch.BatchNumber,
        DebtorType = batch.DebtorType,
        DebtorReferenceId = batch.DebtorReferenceId,
        PeriodStart = batch.PeriodStart,
        PeriodEnd = batch.PeriodEnd,
        TotalAmount = batch.TotalAmount,
        Status = batch.Status,
        IssuedAt = batch.IssuedAt,
        RowVersion = batch.RowVersion,
        ClaimStatus = batch.ClaimStatus,
        ApprovedAmount = batch.ApprovedAmount,
        ClaimVarianceAmount = batch.ApprovedAmount.HasValue ? batch.TotalAmount - batch.ApprovedAmount.Value : null,
        PayerClaimReference = batch.PayerClaimReference,
        ClaimNote = batch.ClaimNote,
        PayerVerifiedAt = batch.PayerVerifiedAt,
        ClaimApprovedAt = batch.ClaimApprovedAt,
        ClaimClosedAt = batch.ClaimClosedAt
    };

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        ReceivableInvoiceBatchConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        ReceivableInvoiceBatchValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        ReceivableInvoiceBatchBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or ReceivableInvoiceBatchConflictException
        or ReceivableInvoiceBatchValidationException or ReceivableInvoiceBatchBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
