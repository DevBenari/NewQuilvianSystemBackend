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
/// Layanan API posisi dan penutupan kas harian milik Finance (FinDailyCashSnapshot).
/// Angka kas yang sudah ditutup dibekukan (FIN-DES-022, FR-FIN-065).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/daily-cash")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_DAILY_CASH", "Corporate Finance Management Daily Cash", "FinanceDailyCash",
    AreaName = "Corporate", ControllerName = "FinanceDailyCash", Description = "Posisi kas harian Finance", SortOrder = 51)]
[Tags("Corporate / Finance Management / Daily Cash")]
public sealed class FinanceDailyCashController : ControllerBase
{
    private readonly FinanceCashManagementService _service;

    public FinanceDailyCashController(FinanceCashManagementService service) => _service = service;

    [HttpGet("current")]
    [AccessAction("Read", "Read Daily Cash", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceDailyCash", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DailyCashResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrent(CancellationToken cancellationToken) =>
        Ok(ApiResponse<DailyCashResponse>.Ok(
            await _service.GetCurrentDailyCashAsync(cancellationToken), "Posisi kas berjalan hari ini berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Daily Cash", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceDailyCash", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DailyCashResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] DailyCashQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<DailyCashResponse>>.Ok(
            await _service.GetDailyCashPagedAsync(request, cancellationToken), "Riwayat kas harian berhasil diambil."));

    [HttpGet("{cashDate}/breakdown")]
    [AccessAction("Read", "Read Daily Cash", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceDailyCash", "Read")]
    [ProducesResponseType(typeof(ApiResponse<DailyCashBreakdownResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBreakdown(DateOnly cashDate, CancellationToken cancellationToken) =>
        Ok(ApiResponse<DailyCashBreakdownResponse>.Ok(
            await _service.GetDailyCashBreakdownAsync(cashDate, cancellationToken), "Rincian kas harian berhasil diambil."));

    [HttpPost("{cashDate}/close")]
    [AccessAction("Close", "Close Daily Cash", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("FinanceDailyCash", "Close")]
    [ProducesResponseType(typeof(ApiResponse<DailyCashResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Close(DateOnly cashDate, [FromBody] CloseDailyCashRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CloseDailyCashAsync(cashDate, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<DailyCashResponse>.Ok(result, "Kas harian berhasil ditutup dan dibekukan."));
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
