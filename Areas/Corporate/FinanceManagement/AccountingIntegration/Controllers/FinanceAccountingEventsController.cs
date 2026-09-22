using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.AccountingIntegration.Controllers;

/// <summary>
/// Monitoring/laporan baca-saja (transaction-endpoint-standard.md §2.4) — HANYA GET. Tidak ada
/// endpoint pengirim/retry di sini; itu wewenang EPIC FIN-12 yang menunggu endpoint penerima
/// Accounting (roadmap 01-backend-roadmap.md baris BE-FIN-012).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/accounting-events")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_ACCOUNTING_EVENTS", "Corporate Finance Management Accounting Events", "Accounting Events",
    AreaName = "Corporate", ControllerName = "AccountingEvents", Description = "Pantauan kotak keluar kejadian ke Accounting, baca saja", SortOrder = 32)]
[Tags("Corporate / Finance Management / Accounting Events")]
public sealed class FinanceAccountingEventsController : ControllerBase
{
    private readonly FinanceAccountingEventService _service;
    public FinanceAccountingEventsController(FinanceAccountingEventService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AccountingEvents", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AccountingEventFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<AccountingEventFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter kejadian berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AccountingEvents", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AccountingEventSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<AccountingEventSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan kejadian berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AccountingEvents", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<AccountingEventResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] AccountingEventQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<AccountingEventResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Kejadian berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("AccountingEvents", "Read")]
    [ProducesResponseType(typeof(ApiResponse<AccountingEventDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<AccountingEventDetailResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail kejadian berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }
}
