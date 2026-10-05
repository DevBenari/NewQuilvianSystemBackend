using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Controllers;

/// <summary>
/// Endpoint pengelolaan pemetaan akun control subledger dan saldo awal cutover (BE-FIN-065, FIN-API-1.5 F.1, FIN-DES-080).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/subledger-setup")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_SUBLEDGER_SETUP",
    "Corporate Finance Management Subledger Setup", "Subledger Setup",
    AreaName = "Corporate", ControllerName = "FinanceSubledgerSetup",
    Description = "Pemetaan akun control dan saldo awal cutover", SortOrder = 70)]
[Tags("Corporate / Finance Management / Subledger Setup")]
public sealed class FinanceSubledgerSetupController : ControllerBase
{
    private readonly FinanceSubledgerControlAccountService _service;
    private readonly FinanceOpeningBalanceService _openingBalanceService;
    private readonly ILogger<FinanceSubledgerSetupController> _logger;

    public FinanceSubledgerSetupController(
        FinanceSubledgerControlAccountService service,
        FinanceOpeningBalanceService openingBalanceService,
        ILogger<FinanceSubledgerSetupController> logger)
    {
        _service = service;
        _openingBalanceService = openingBalanceService;
        _logger = logger;
    }

    /// <summary>
    /// Daftar pemetaan kelompok dan segmen ke kode akun control (GET /control-accounts).
    /// </summary>
    [HttpGet("control-accounts")]
    [AccessAction("Read", "Read Subledger Setup", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceSubledgerSetup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ControlAccountMapResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetControlAccounts(
        [FromQuery] ControlAccountMapPagedQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ControlAccountMapResponse>>.Ok(result, "Daftar pemetaan akun control berhasil diambil."));
    }

    /// <summary>
    /// Memeriksa kelengkapan cakupan sebelum snapshot; mengembalikan kelompok dan segmen yang belum terpetakan (GET /control-accounts/coverage).
    /// </summary>
    [HttpGet("control-accounts/coverage")]
    [AccessAction("Read", "Read Subledger Setup", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceSubledgerSetup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<ControlAccountCoverageResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetControlAccountCoverage(CancellationToken cancellationToken)
    {
        var result = await _service.GetCoverageAsync(cancellationToken);
        return Ok(ApiResponse<ControlAccountCoverageResponse>.Ok(result, "Cakupan pemetaan akun control berhasil dievaluasi."));
    }

    /// <summary>
    /// Menambah satu baris pemetaan akun control baru (POST /control-accounts).
    /// </summary>
    [HttpPost("control-accounts")]
    [AccessAction("Create", "Create Subledger Setup", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceSubledgerSetup", "Create")]
    [ProducesResponseType(typeof(ApiResponse<ControlAccountMapResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateControlAccount(
        [FromBody] CreateControlAccountMapRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateAsync(request, CurrentUserId(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<ControlAccountMapResponse>.Ok(result, "Pemetaan akun control berhasil ditambahkan."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Mengoreksi kode akun control atau catatan pada pemetaan yang ada (PUT /control-accounts/{id}).
    /// </summary>
    [HttpPut("control-accounts/{id:guid}")]
    [AccessAction("Update", "Update Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceSubledgerSetup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ControlAccountMapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateControlAccount(
        Guid id,
        [FromBody] UpdateControlAccountMapRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ControlAccountMapResponse>.Ok(result, "Pemetaan akun control berhasil diperbarui."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Menonaktifkan pemetaan akun control; barisnya disimpan sebagai riwayat (POST /control-accounts/{id}/deactivate).
    /// </summary>
    [HttpPost("control-accounts/{id:guid}/deactivate")]
    [AccessAction("Update", "Update Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceSubledgerSetup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ControlAccountMapResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateControlAccount(
        Guid id,
        [FromBody] DeactivateControlAccountMapRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.DeactivateAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ControlAccountMapResponse>.Ok(result, "Pemetaan akun control berhasil dinonaktifkan."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Daftar saldo awal per kelompok saldo (GET /opening-balances).
    /// </summary>
    [HttpGet("opening-balances")]
    [AccessAction("Read", "Read Subledger Setup", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceSubledgerSetup", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<OpeningBalanceResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOpeningBalances(CancellationToken cancellationToken)
    {
        var result = await _openingBalanceService.GetAllAsync(cancellationToken);
        return Ok(ApiResponse<List<OpeningBalanceResponse>>.Ok(result, "Daftar saldo awal cutover berhasil diambil."));
    }

    /// <summary>
    /// Mencatat saldo awal satu kelompok saldo, status DRAFT (POST /opening-balances).
    /// </summary>
    [HttpPost("opening-balances")]
    [AccessAction("Create", "Create Subledger Setup", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceSubledgerSetup", "Create")]
    [ProducesResponseType(typeof(ApiResponse<OpeningBalanceResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateOpeningBalance(
        [FromBody] CreateOpeningBalanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _openingBalanceService.CreateAsync(request, CurrentUserId(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<OpeningBalanceResponse>.Ok(result, "Saldo awal cutover berhasil dicatat dalam status DRAFT."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Mengoreksi saldo awal yang masih berstatus DRAFT (PUT /opening-balances/{id}).
    /// </summary>
    [HttpPut("opening-balances/{id:guid}")]
    [AccessAction("Update", "Update Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("FinanceSubledgerSetup", "Update")]
    [ProducesResponseType(typeof(ApiResponse<OpeningBalanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> UpdateOpeningBalance(
        Guid id,
        [FromBody] UpdateOpeningBalanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _openingBalanceService.UpdateAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningBalanceResponse>.Ok(result, "Saldo awal cutover berhasil diperbarui."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Menyetujui saldo awal cutover (POST /opening-balances/{id}/approve).
    /// </summary>
    [HttpPost("opening-balances/{id:guid}/approve")]
    [AccessAction("Approve", "Approve Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceSubledgerSetup", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<OpeningBalanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ApproveOpeningBalance(
        Guid id,
        [FromBody] ApproveOpeningBalanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _openingBalanceService.ApproveAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningBalanceResponse>.Ok(result, "Saldo awal cutover berhasil disetujui."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    /// <summary>
    /// Mengunci saldo awal; sesudahnya nilainya tidak dapat diubah (POST /opening-balances/{id}/lock).
    /// </summary>
    [HttpPost("opening-balances/{id:guid}/lock")]
    [AccessAction("Approve", "Approve Subledger Setup", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("FinanceSubledgerSetup", "Approve")]
    [ProducesResponseType(typeof(ApiResponse<OpeningBalanceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> LockOpeningBalance(
        Guid id,
        [FromBody] LockOpeningBalanceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _openingBalanceService.LockAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<OpeningBalanceResponse>.Ok(result, "Saldo awal cutover berhasil dikunci permanen."));
        }
        catch (Exception exception) when (IsHandled(exception))
        {
            return Failure(exception);
        }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message)),
        FinanceSubledgerConflictException => Conflict(ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message)),
        FinanceSubledgerValidationException => UnprocessableEntity(ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message)),
        FinanceSubledgerBadRequestException => BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is
        KeyNotFoundException or
        FinanceSubledgerConflictException or
        FinanceSubledgerValidationException or
        FinanceSubledgerBadRequestException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
