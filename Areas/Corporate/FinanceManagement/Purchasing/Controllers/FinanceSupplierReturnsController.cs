using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Models;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Purchasing.Controllers;

/// <summary>
/// Retur Pembelian dan Deposit Retur (BE-FIN-035, FIN-API-1.1 §B.5, FIN-PERM-1.1 §B.5).
/// `GET /` (daftar retur berpaging) belum ada service-nya — gap yang sama seperti controller
/// Purchasing lain (`BE-FIN-032`/`033`/`034`).
///
/// **Delta kontrak (dicatat, bukan didiamkan):** `api-contract.md` §B.5 hanya mendaftarkan
/// `GET /`, `GET /{id}`, `POST /`, `GET /deposits` — tidak ada `POST /{id}/confirm` maupun
/// `POST /{id}/cancel` secara eksplisit, padahal `state-transition-matrix.md` §B.5 mendefinisikan
/// `DRAFT -> CONFIRMED` (Konfirmasi) dan `DRAFT -> CANCELLED` (Batalkan) sebagai transisi
/// terpisah, dan roadmap `BE-FIN-035` Cakupan eksplisit menyebut "catat, konfirmasi, batal".
/// Tanpa endpoint ini retur tidak pernah bisa mencapai `CONFIRMED` (sehingga Deposit Retur tidak
/// pernah diterbitkan) maupun `CANCELLED` — kapabilitasnya tidak berfungsi tanpa keduanya. Dibuat
/// mengikuti pola `POST /{id}/&lt;aksi&gt;` yang sudah baku di rumpun ini (`FinanceInvoiceExchangesController.Cancel`),
/// bukan kebijakan baru — action/permission `Confirm`/`Cancel` ikut ditambahkan pada resource
/// `FinanceSupplierReturn` karena `permission-audit-matrix.md` §B.5 juga belum mendaftarkannya.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/purchasing/supplier-returns")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_SUPPLIER_RETURN", "Corporate Finance Management Supplier Return", "Supplier Return",
    AreaName = "Corporate", ControllerName = "SupplierReturn", Description = "Retur Pembelian dan Deposit Retur Finance — pencatatan retur dan penerbitan kredit lintas invoice", SortOrder = 46)]
[Tags("Corporate / Finance Management / Purchasing / Supplier Return")]
public sealed class FinanceSupplierReturnsController : ControllerBase
{
    private readonly FinanceSupplierReturnService _service;
    private readonly PurchasingIdempotencyService _idempotency;
    public FinanceSupplierReturnsController(FinanceSupplierReturnService service, PurchasingIdempotencyService idempotency)
    {
        _service = service;
        _idempotency = idempotency;
    }

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Supplier Return", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceSupplierReturn", "Read")]
    [ProducesResponseType(typeof(ApiResponse<SupplierReturnDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var supplierReturn = await _service.GetByIdAsync(id, cancellationToken);
        if (supplierReturn is null) return NotFound(ApiResponse<object>.Fail(404, "Retur Pembelian tidak ditemukan."));
        return Ok(ApiResponse<SupplierReturnDetailResponse>.Ok(MapDetail(supplierReturn), "Detail Retur Pembelian berhasil diambil."));
    }

    [HttpPost]
    [AccessAction("Create", "Create Supplier Return", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceSupplierReturn", "Create")]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreateSupplierReturnRequest request, CancellationToken cancellationToken)
    {
        var replay = await _idempotency.TryReplayAsync(idempotencyKey, cancellationToken);
        if (replay is not null) return Replay(replay);

        try
        {
            var supplierReturn = await _service.CreateAsync(
                request.PurchasingInvoiceId, request.Reason, request.PPNAmount, request.Items, CurrentUserId(), cancellationToken);
            var response = ApiResponse<SupplierReturnResponse>.Ok(Map(supplierReturn), "Retur Pembelian berhasil dicatat.");
            await _idempotency.SaveAsync(idempotencyKey, FinPurchasingIdempotencyEntityTypes.SupplierReturn,
                FinPurchasingIdempotencyActions.Create, supplierReturn.Id, 201, response, cancellationToken);
            return StatusCode(201, response);
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/confirm")]
    [AccessAction("Confirm", "Confirm Supplier Return", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceSupplierReturn", "Confirm")]
    public async Task<IActionResult> Confirm(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        Guid id, [FromBody] SupplierReturnRowVersionRequest request, CancellationToken cancellationToken)
    {
        var replay = await _idempotency.TryReplayAsync(idempotencyKey, cancellationToken);
        if (replay is not null) return Replay(replay);

        try
        {
            var supplierReturn = await _service.ConfirmAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            var response = ApiResponse<SupplierReturnResponse>.Ok(Map(supplierReturn), "Retur Pembelian berhasil dikonfirmasi. Deposit Retur telah diterbitkan.");
            await _idempotency.SaveAsync(idempotencyKey, FinPurchasingIdempotencyEntityTypes.SupplierReturn,
                FinPurchasingIdempotencyActions.Confirm, supplierReturn.Id, 200, response, cancellationToken);
            return Ok(response);
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Supplier Return", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceSupplierReturn", "Cancel")]
    public async Task<IActionResult> Cancel(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        Guid id, [FromBody] SupplierReturnRowVersionRequest request, CancellationToken cancellationToken)
    {
        var replay = await _idempotency.TryReplayAsync(idempotencyKey, cancellationToken);
        if (replay is not null) return Replay(replay);

        try
        {
            var supplierReturn = await _service.CancelAsync(id, request.ExpectedRowVersion, CurrentUserId(), cancellationToken);
            var response = ApiResponse<SupplierReturnResponse>.Ok(Map(supplierReturn), "Retur Pembelian berhasil dibatalkan.");
            await _idempotency.SaveAsync(idempotencyKey, FinPurchasingIdempotencyEntityTypes.SupplierReturn,
                FinPurchasingIdempotencyActions.Cancel, supplierReturn.Id, 200, response, cancellationToken);
            return Ok(response);
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpGet("deposits")]
    [AccessAction("Read", "Read Supplier Return", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceSupplierReturn", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<SupplierReturnDepositResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeposits([FromQuery] SupplierReturnDepositQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetDepositsPagedAsync(query, cancellationToken);
        var mapped = new PagedResult<SupplierReturnDepositResponse>
        {
            Items = result.Items.Select(MapDeposit).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalData = result.TotalData,
            TotalPage = result.TotalPage
        };
        return Ok(ApiResponse<PagedResult<SupplierReturnDepositResponse>>.Ok(mapped, "Daftar Deposit Retur berhasil diambil."));
    }

    private static SupplierReturnResponse Map(FinSupplierReturn supplierReturn) => new()
    {
        Id = supplierReturn.Id,
        ReturnNumber = supplierReturn.ReturnNumber,
        PurchasingInvoiceId = supplierReturn.PurchasingInvoiceId,
        Reason = supplierReturn.Reason,
        TotalAmount = supplierReturn.TotalAmount,
        PPNAmount = supplierReturn.PPNAmount,
        Status = supplierReturn.Status,
        RowVersion = supplierReturn.RowVersion
    };

    private static SupplierReturnDetailResponse MapDetail(FinSupplierReturn supplierReturn)
    {
        var mapped = Map(supplierReturn);
        return new SupplierReturnDetailResponse
        {
            Id = mapped.Id,
            ReturnNumber = mapped.ReturnNumber,
            PurchasingInvoiceId = mapped.PurchasingInvoiceId,
            Reason = mapped.Reason,
            TotalAmount = mapped.TotalAmount,
            PPNAmount = mapped.PPNAmount,
            Status = mapped.Status,
            RowVersion = mapped.RowVersion,
            Items = supplierReturn.Items.Select(x => new SupplierReturnItemResponse
            {
                Id = x.Id,
                Description = x.Description,
                Quantity = x.Quantity,
                LineTotal = x.LineTotal
            }).ToList(),
            Deposit = supplierReturn.Deposit is null ? null : MapDeposit(supplierReturn.Deposit)
        };
    }

    private static SupplierReturnDepositResponse MapDeposit(FinSupplierReturnDeposit deposit) => new()
    {
        Id = deposit.Id,
        SupplierId = deposit.SupplierId,
        SourceReturnId = deposit.SourceReturnId,
        OriginalAmount = deposit.OriginalAmount,
        AvailableAmount = deposit.AvailableAmount,
        Status = deposit.Status,
        RowVersion = deposit.RowVersion
    };

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        PurchasingForbiddenException => StatusCode(403, ApiResponse<object>.Fail(403, exception.Message)),
        PurchasingConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        PurchasingValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        PurchasingBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or PurchasingForbiddenException or PurchasingConflictException
        or PurchasingValidationException or PurchasingBadRequestException;

    private IActionResult Replay(PurchasingIdempotencyService.CachedResult cached) =>
        new ContentResult { StatusCode = cached.StatusCode, Content = cached.ResponseBody, ContentType = "application/json" };

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
