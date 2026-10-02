using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Controllers;

/// <summary>
/// Monitoring kotak keluar kejadian ke Accounting (GET) dan pemicu kalkulasi snapshot saldo subledger (POST/GET) (BE-FIN-049).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/accounting-events")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_ACCOUNTING_EVENTS", "Corporate Finance Management Accounting Events", "Accounting Events",
    AreaName = "Corporate", ControllerName = "FinanceAccountingEvent", Description = "Pantauan kotak keluar kejadian ke Accounting dan snapshot saldo subledger", SortOrder = 32)]
[Tags("Corporate / Finance Management / Accounting Events")]
public sealed class FinanceAccountingEventsController : ControllerBase
{
    private readonly FinanceAccountingEventService _service;
    private readonly FinanceSubledgerSnapshotService _subledgerSnapshotService;

    public FinanceAccountingEventsController(
        FinanceAccountingEventService service,
        FinanceSubledgerSnapshotService subledgerSnapshotService)
    {
        _service = service;
        _subledgerSnapshotService = subledgerSnapshotService;
    }

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceAccountingEvent", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AccountingEventFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<AccountingEventFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter kejadian berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceAccountingEvent", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AccountingEventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<AccountingEventSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan kejadian berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceAccountingEvent", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AccountingEventResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] AccountingEventQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<AccountingEventResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Kejadian berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceAccountingEvent", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AccountingEventDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<AccountingEventDetailResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail kejadian berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost("subledger-balances/generate")]
    [AccessAction("Create", "Generate Subledger Balance Snapshots", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("FinanceAccountingEvent", "Create")]
    [ProducesResponseType(typeof(ApiResponse<GenerateSubledgerSnapshotsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateSubledgerSnapshots(
        [FromBody] GenerateSubledgerSnapshotsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _subledgerSnapshotService.GenerateMonthlySnapshotsAsync(request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<GenerateSubledgerSnapshotsResponse>.Ok(result, result.Message));
        }
        catch (FinanceSubledgerSnapshotValidationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(400, exception.Message));
        }
        catch (AccountingOutboxException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(400, exception.Message));
        }
    }

    [HttpGet("subledger-balances/{accountingPeriodCode}")]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("FinanceAccountingEvent", "Read")]
    [ProducesResponseType(typeof(ApiResponse<SubledgerPeriodSnapshotsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSubledgerSnapshotsByPeriod(
        string accountingPeriodCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _subledgerSnapshotService.GetSnapshotsByPeriodAsync(accountingPeriodCode, cancellationToken);
            return Ok(ApiResponse<SubledgerPeriodSnapshotsResponse>.Ok(result, "Snapshot saldo subledger periode berhasil diambil."));
        }
        catch (FinanceSubledgerSnapshotValidationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(400, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}

