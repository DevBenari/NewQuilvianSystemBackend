using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.DTOs;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Controllers;

/// <summary>
/// Layanan API setoran bank milik Finance (aggregate ber-lifecycle, bukan master data).
/// Perpindahan status menggunakan aksi POST /{id}/<aksi> (transaction-endpoint-standard.md).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/bank-deposits")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_BANK_DEPOSIT", "Corporate Finance Management Bank Deposit", "FinanceBankDeposit",
    AreaName = "Corporate", ControllerName = "FinanceBankDeposit", Description = "Setoran kas ke bank milik Finance", SortOrder = 50)]
[Tags("Corporate / Finance Management / Bank Deposit")]
public sealed class FinanceBankDepositsController : ControllerBase
{
    private readonly FinanceCashManagementService _service;

    public FinanceBankDepositsController(FinanceCashManagementService service) => _service = service;

    [HttpGet]
    [AccessAction("Read", "Read Bank Deposit", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceBankDeposit", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BankDepositResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] BankDepositQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<BankDepositResponse>>.Ok(
            await _service.GetBankDepositsPagedAsync(request, cancellationToken), "Daftar setoran bank berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Bank Deposit", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceBankDeposit", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BankDepositResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<BankDepositResponse>.Ok(
                await _service.GetBankDepositByIdAsync(id, cancellationToken), "Rincian setoran bank berhasil diambil."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpGet("available-balance")]
    [AccessAction("Read", "Read Bank Deposit", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceBankDeposit", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AvailableCashBalanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAvailableBalance([FromQuery] AvailableBalanceQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<AvailableCashBalanceResponse>.Ok(
                await _service.GetAvailableCashBalanceAsync(request.Date, request.CashierShiftId, cancellationToken),
                "Saldo kas tersedia berhasil dihitung."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Bank Deposit", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceBankDeposit", "Create")]
    [ProducesResponseType(typeof(ApiResponse<BankDepositResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateBankDepositRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateBankDepositAsync(request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<BankDepositResponse>.Ok(result, "Draf setoran bank berhasil dibuat."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/post")]
    [AccessAction("Post", "Post Bank Deposit", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceBankDeposit", "Post")]
    [ProducesResponseType(typeof(ApiResponse<BankDepositResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Post(Guid id, [FromBody] PostBankDepositRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.PostBankDepositAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<BankDepositResponse>.Ok(result, "Setoran bank berhasil diposting."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/verify")]
    [AccessAction("Verify", "Verify Bank Deposit", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceBankDeposit", "Verify")]
    [ProducesResponseType(typeof(ApiResponse<BankDepositResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Verify(Guid id, [FromBody] VerifyBankDepositRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.VerifyBankDepositAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<BankDepositResponse>.Ok(result, "Setoran bank berhasil diverifikasi dengan rekening koran."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    [HttpPost("{id:guid}/cancel")]
    [AccessAction("Cancel", "Cancel Bank Deposit", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("FinanceBankDeposit", "Cancel")]
    [ProducesResponseType(typeof(ApiResponse<BankDepositResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelBankDepositRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CancelBankDepositAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<BankDepositResponse>.Ok(result, "Setoran bank berhasil dibatalkan."));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        CashConflictException => Conflict(ApiResponse<object>.Fail(409, exception.Message)),
        CashValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        CashBadRequestException => BadRequest(ApiResponse<object>.Fail(400, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or CashConflictException or CashValidationException or CashBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
