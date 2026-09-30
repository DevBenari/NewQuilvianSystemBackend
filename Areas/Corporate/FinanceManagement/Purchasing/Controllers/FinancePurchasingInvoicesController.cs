using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Payable.Services;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Controllers;

/// <summary>
/// Purchasing Invoice (BE-FIN-034, FIN-API-1.1 §B.4/§B.9, FIN-PERM-1.1 §B.4). `GET /` (daftar
/// berpaging) belum ada service-nya — gap yang sama seperti controller Purchasing lain
/// (`BE-FIN-032`/`033`). **Cancel tidak dibangun** — `api-contract.md` §B.4 tidak mendaftarkan
/// endpoint ini sama sekali walau `state-transition-matrix.md` §B.4 mengizinkan transisi
/// `DRAFT`/`PENDING_APPROVAL` → `CANCELLED`; acceptance criteria roadmap `BE-FIN-034` juga tidak
/// menyebutnya. Dicatat sebagai kesenjangan dokumen, bukan diam-diam ditambah di sini.
///
/// `Approve` dapat melempar exception dari `FinanceSupplierPayableService` (`Payable*Exception`)
/// selain dari `Purchasing*Exception` milik submodul ini sendiri — keduanya ditangani di sini
/// karena approve memanggil layanan itu di dalam satu transaksi (lihat
/// `FinancePurchasingInvoiceService.ApproveAsync`).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/purchasing/purchasing-invoices")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_PURCHASING_INVOICE", "Corporate Finance Management Purchasing Invoice", "Purchasing Invoice",
    AreaName = "Corporate", ControllerName = "PurchasingInvoice", Description = "Purchasing Invoice Finance — pencatatan, persetujuan berjenjang, dan pengakuan utang", SortOrder = 45)]
[Tags("Corporate / Finance Management / Purchasing / Purchasing Invoice")]
public sealed class FinancePurchasingInvoicesController : ControllerBase
{
    private readonly FinancePurchasingInvoiceService _service;
    private readonly PurchasingIdempotencyService _idempotency;
    public FinancePurchasingInvoicesController(FinancePurchasingInvoiceService service, PurchasingIdempotencyService idempotency)
    {
        _service = service;
        _idempotency = idempotency;
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Purchasing Invoice", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinancePurchasingInvoice", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PurchasingInvoiceDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var purchasingInvoice = await _service.GetByIdAsync(id, cancellationToken);
        if (purchasingInvoice is null) return NotFound(ApiResponse<object>.Fail(404, "Purchasing Invoice tidak ditemukan."));
        return Ok(ApiResponse<PurchasingInvoiceDetailResponse>.Ok(MapDetail(purchasingInvoice), "Detail Purchasing Invoice berhasil diambil."));
    }

    [HttpPost]
    [AccessAction("Create", "Create Purchasing Invoice", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinancePurchasingInvoice", "Create")]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreatePurchasingInvoiceRequest request, CancellationToken cancellationToken)
    {
        var replay = await _idempotency.TryReplayAsync(idempotencyKey, cancellationToken);
        if (replay is not null) return Replay(replay);

        try
        {
            var items = MapItemRequests(request.Items);
            var purchasingInvoice = await _service.CreateAsync(
                request.InvoiceExchangeId, request.DiscountAmount, request.PPNAmount, request.DownPaymentAmount,
                request.OtherDeductionAmount, request.TotalAmount, items, CurrentUserId(), cancellationToken);
            var response = ApiResponse<PurchasingInvoiceResponse>.Ok(Map(purchasingInvoice), "Purchasing Invoice berhasil disusun.");
            await _idempotency.SaveAsync(idempotencyKey, FinPurchasingIdempotencyEntityTypes.PurchasingInvoice,
                FinPurchasingIdempotencyActions.Create, purchasingInvoice.Id, 201, response, cancellationToken);
            return StatusCode(201, response);
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Purchasing Invoice", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinancePurchasingInvoice", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchasingInvoiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var items = MapItemRequests(request.Items);
            var purchasingInvoice = await _service.UpdateAsync(
                id, request.ExpectedRowVersion, request.DiscountAmount, request.PPNAmount, request.DownPaymentAmount,
                request.OtherDeductionAmount, request.TotalAmount, items, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PurchasingInvoiceResponse>.Ok(Map(purchasingInvoice), "Purchasing Invoice berhasil diperbarui."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/submit")]
    [AccessAction("Submit", "Submit Purchasing Invoice", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinancePurchasingInvoice", "Submit")]
    public async Task<IActionResult> Submit(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        Guid id, [FromBody] PurchasingInvoiceRowVersionRequest request, CancellationToken cancellationToken)
    {
        var replay = await _idempotency.TryReplayAsync(idempotencyKey, cancellationToken);
        if (replay is not null) return Replay(replay);

        try
        {
            var purchasingInvoice = await _service.SubmitAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            var response = ApiResponse<PurchasingInvoiceResponse>.Ok(Map(purchasingInvoice), "Purchasing Invoice berhasil diajukan.");
            await _idempotency.SaveAsync(idempotencyKey, FinPurchasingIdempotencyEntityTypes.PurchasingInvoice,
                FinPurchasingIdempotencyActions.Submit, purchasingInvoice.Id, 200, response, cancellationToken);
            return Ok(response);
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/approve")]
    [AccessAction("Approve", "Approve Purchasing Invoice", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinancePurchasingInvoice", "Approve")]
    public async Task<IActionResult> Approve(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        Guid id, [FromBody] PurchasingInvoiceRowVersionRequest request, CancellationToken cancellationToken)
    {
        var replay = await _idempotency.TryReplayAsync(idempotencyKey, cancellationToken);
        if (replay is not null) return Replay(replay);

        try
        {
            var purchasingInvoice = await _service.ApproveAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            var response = ApiResponse<PurchasingInvoiceResponse>.Ok(Map(purchasingInvoice), "Purchasing Invoice berhasil disetujui.");
            await _idempotency.SaveAsync(idempotencyKey, FinPurchasingIdempotencyEntityTypes.PurchasingInvoice,
                FinPurchasingIdempotencyActions.Approve, purchasingInvoice.Id, 200, response, cancellationToken);
            return Ok(response);
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/reject")]
    [AccessAction("Approve", "Approve Purchasing Invoice", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinancePurchasingInvoice", "Approve")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectPurchasingInvoiceRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var purchasingInvoice = await _service.RejectAsync(id, request.ExpectedRowVersion, request.RejectionReason, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<PurchasingInvoiceResponse>.Ok(Map(purchasingInvoice), "Purchasing Invoice berhasil ditolak."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private static List<PurchasingInvoiceItemRequest> MapItemRequests(List<PurchasingInvoiceItemRequestDto> items) =>
        items.Select(x => new PurchasingInvoiceItemRequest(x.ProductName, x.Quantity, x.UnitPrice)).ToList();

    private static PurchasingInvoiceResponse Map(FinPurchasingInvoice purchasingInvoice) => new()
    {
        Id = purchasingInvoice.Id,
        InvoiceNumber = purchasingInvoice.InvoiceNumber,
        InvoiceExchangeId = purchasingInvoice.InvoiceExchangeId,
        SupplierId = purchasingInvoice.SupplierId,
        SubtotalAmount = purchasingInvoice.SubtotalAmount,
        DiscountAmount = purchasingInvoice.DiscountAmount,
        PPNAmount = purchasingInvoice.PPNAmount,
        DownPaymentAmount = purchasingInvoice.DownPaymentAmount,
        OtherDeductionAmount = purchasingInvoice.OtherDeductionAmount,
        TotalAmount = purchasingInvoice.TotalAmount,
        Status = purchasingInvoice.Status,
        ApprovalTier = purchasingInvoice.ApprovalTier,
        RequestedByUserId = purchasingInvoice.RequestedByUserId,
        RequestedAt = purchasingInvoice.RequestedAt,
        ApprovedByUserId = purchasingInvoice.ApprovedByUserId,
        ApprovedAt = purchasingInvoice.ApprovedAt,
        RowVersion = purchasingInvoice.RowVersion
    };

    private static PurchasingInvoiceDetailResponse MapDetail(FinPurchasingInvoice purchasingInvoice)
    {
        var mapped = Map(purchasingInvoice);
        return new PurchasingInvoiceDetailResponse
        {
            Id = mapped.Id,
            InvoiceNumber = mapped.InvoiceNumber,
            InvoiceExchangeId = mapped.InvoiceExchangeId,
            SupplierId = mapped.SupplierId,
            SubtotalAmount = mapped.SubtotalAmount,
            DiscountAmount = mapped.DiscountAmount,
            PPNAmount = mapped.PPNAmount,
            DownPaymentAmount = mapped.DownPaymentAmount,
            OtherDeductionAmount = mapped.OtherDeductionAmount,
            TotalAmount = mapped.TotalAmount,
            Status = mapped.Status,
            ApprovalTier = mapped.ApprovalTier,
            RequestedByUserId = mapped.RequestedByUserId,
            RequestedAt = mapped.RequestedAt,
            ApprovedByUserId = mapped.ApprovedByUserId,
            ApprovedAt = mapped.ApprovedAt,
            RowVersion = mapped.RowVersion,
            Items = purchasingInvoice.Items.Select(x => new PurchasingInvoiceItemResponse
            {
                Id = x.Id,
                ProductName = x.ProductName,
                Quantity = x.Quantity,
                UnitPrice = x.UnitPrice,
                LineTotal = x.LineTotal
            }).ToList()
        };
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PurchasingForbiddenException => StatusCode(403, ApiResponse<object>.Fail(403, exception.Message)),
        PurchasingConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PurchasingValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        PurchasingBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        // Dari FinanceSupplierPayableService (dipanggil di dalam ApproveAsync) — lihat ringkasan kelas.
        PayableConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PayableValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        PayableBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or PurchasingForbiddenException or PurchasingConflictException
        or PurchasingValidationException or PurchasingBadRequestException
        or PayableConflictException or PayableValidationException or PayableBadRequestException;

    private IActionResult Replay(PurchasingIdempotencyService.CachedResult cached) =>
        new ContentResult { StatusCode = cached.StatusCode, Content = cached.ResponseBody, ContentType = "application/json" };

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
