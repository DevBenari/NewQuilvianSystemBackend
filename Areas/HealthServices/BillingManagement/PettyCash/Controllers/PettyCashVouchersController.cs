using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.PettyCash.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/petty-cash/vouchers")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_PETTY_CASH", "Health Service Billing Management Petty Cash", "Petty Cash Vouchers",
    AreaName = "HealthServices", ControllerName = "PettyCashVoucher", Description = "Siklus hidup voucher kas kecil", SortOrder = 2)]
[Tags("Health Services / Billing Management / Petty Cash / Vouchers")]
public sealed class PettyCashVouchersController : ControllerBase
{
    private readonly PettyCashVoucherService _service;
    public PettyCashVouchersController(PettyCashVoucherService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Petty Cash Voucher", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashVoucher", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashVoucherFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<PettyCashVoucherFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter voucher kas kecil berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Petty Cash Voucher", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashVoucher", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashVoucherSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<PettyCashVoucherSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan voucher kas kecil berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Petty Cash Voucher", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashVoucher", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PettyCashVoucherResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] PettyCashVoucherQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<PettyCashVoucherResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Daftar voucher kas kecil berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Petty Cash Voucher", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("PettyCashVoucher", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashVoucherDetailResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<PettyCashVoucherDetailResponse>.Ok(
                await _service.GetByIdAsync(id, cancellationToken), "Detail voucher kas kecil berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Petty Cash Voucher", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("PettyCashVoucher", "Create")]
    [ProducesResponseType(typeof(ApiResponse<PettyCashVoucherResponse>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CreatePettyCashVoucherRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken);
            var statusCode = result.IsReplay ? StatusCodes.Status200OK : StatusCodes.Status201Created;
            var message = result.IsReplay ? "Voucher sudah dibuat; hasil sebelumnya dikembalikan." : "Voucher kas kecil berhasil dibuat.";
            return StatusCode(statusCode, Success(result, statusCode, message));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Petty Cash Voucher", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("PettyCashVoucher", "Cancel")]
    public Task<IActionResult> Cancel(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] CancelPettyCashVoucherRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.CancelAsync(id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken),
            "Voucher sudah dibatalkan; hasil sebelumnya dikembalikan.", "Voucher kas kecil berhasil dibatalkan.");

    [HttpPost("{id:guid}/disburse")]
    [AccessAction("Disburse", "Disburse Petty Cash Voucher", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("PettyCashVoucher", "Disburse")]
    public Task<IActionResult> Disburse(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] DisbursePettyCashVoucherRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.DisburseAsync(id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken),
            "Uang voucher sudah diserahkan; hasil sebelumnya dikembalikan.", "Uang voucher kas kecil berhasil diserahkan.");

    [HttpPost("{id:guid}/proofs")]
    [AccessAction("AttachProof", "Attach Petty Cash Voucher Proof", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("PettyCashVoucher", "AttachProof")]
    public Task<IActionResult> AttachProof(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] AttachPettyCashProofRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.AttachProofAsync(id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken),
            "Bukti nota sudah dimasukkan; hasil sebelumnya dikembalikan.", "Bukti nota voucher kas kecil berhasil disimpan.");

    [HttpPost("{id:guid}/returns")]
    [AccessAction("Return", "Return Petty Cash Voucher Cash", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("PettyCashVoucher", "Return")]
    public Task<IActionResult> Return(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] PettyCashVoucherReturnRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.ReturnAsync(id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken),
            "Pengembalian ini sudah pernah dicatat; hasil sebelumnya dikembalikan.", "Sisa uang berhasil dicatat sebagai pengembalian.");

    [HttpPost("{id:guid}/reversals")]
    [AccessAction("Reverse", "Reverse Petty Cash Voucher Disbursement", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("PettyCashVoucher", "Reverse")]
    public Task<IActionResult> Reverse(
        Guid id,
        [FromHeader(Name = "Idempotency-Key")] Guid idempotencyKey,
        [FromBody] PettyCashVoucherReversalRequest request,
        CancellationToken cancellationToken) =>
        ExecuteAsync(
            () => _service.ReverseAsync(id, request, idempotencyKey, CurrentUserId(), CurrentRole(), cancellationToken),
            "Pencairan ini sudah pernah dibalik; hasil sebelumnya dikembalikan.", "Pencairan voucher kas kecil berhasil dibalik.");

    private async Task<IActionResult> ExecuteAsync(
        Func<Task<PettyCashVoucherResponse>> command, string replayMessage, string successMessage)
    {
        try
        {
            var result = await command();
            return Ok(ApiResponse<PettyCashVoucherResponse>.Ok(result, result.IsReplay ? replayMessage : successMessage));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message)),
        PettyCashVoucherForbiddenException => StatusCode(
            StatusCodes.Status403Forbidden, ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message)),
        PettyCashVoucherConflictException => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message)),
        PettyCashBudgetConflictException => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message)),
        PettyCashBudgetInsufficientBalanceException => UnprocessableEntity(
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message)),
        // BE-BKC-057, BIL-VAL-099/BIL-VAL-106: ApplyReturnAsync/ApplyReversalAsync melempar ini
        // untuk kegagalan validasi sisi anggaran (mis. tidak ada periode ACTIVE) — dipetakan 422
        // di sini agar sesuai amendment 15 September 2026, berbeda dari PettyCashBudgetController
        // yang memetakan exception ini ke 400 untuk endpoint miliknya sendiri (di luar scope task ini).
        PettyCashBudgetValidationException => UnprocessableEntity(
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message)),
        PettyCashVoucherValidationException => UnprocessableEntity(
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message)),
        PettyCashVoucherBadRequestException => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or
        PettyCashVoucherForbiddenException or
        PettyCashVoucherConflictException or
        PettyCashBudgetConflictException or
        PettyCashBudgetInsufficientBalanceException or
        PettyCashBudgetValidationException or
        PettyCashVoucherValidationException or
        PettyCashVoucherBadRequestException;

    private static ApiResponse<T> Success<T>(T data, int statusCode, string message) => new()
    {
        Success = true,
        StatusCode = statusCode,
        Message = message,
        Data = data
    };

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }

    private string CurrentRole() =>
        User.FindFirstValue(ClaimTypes.Role) ?? User.FindFirstValue("role") ?? "AuthenticatedUser";
}
