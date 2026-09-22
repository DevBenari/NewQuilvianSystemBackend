using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.MasterData.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/master-data/currencies")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_MASTER_DATA", "Corporate Finance Management Master Data", "Currency",
    AreaName = "Corporate", ControllerName = "Currency", Description = "Master data mata uang dan kurs harian milik Finance", SortOrder = 29)]
[Tags("Corporate / Finance Management / Master Data / Currency")]
public sealed class CurrenciesController : ControllerBase
{
    private readonly CurrencyService _service;
    public CurrenciesController(CurrencyService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Currency", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Currency", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CurrencyFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<CurrencyFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter mata uang berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Currency", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Currency", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CurrencySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<CurrencySummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan mata uang berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Currency", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Currency", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CurrencyResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] CurrencyQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<CurrencyResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Mata uang berhasil diambil."));

    [HttpGet("options")]
    [AccessAction("Read", "Read Currency", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Currency", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<CurrencyOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions([FromQuery] bool onlyActive = true, CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<List<CurrencyOptionResponse>>.Ok(
            await _service.GetOptionsAsync(onlyActive, cancellationToken), "Data pilihan mata uang berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Currency", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Currency", "Read")]
    [ProducesResponseType(typeof(ApiResponse<CurrencyResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<CurrencyResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail mata uang berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Currency", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("Currency", "Create")]
    public Task<IActionResult> Create([FromBody] CreateCurrencyRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken));

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Currency", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("Currency", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateCurrencyRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Currency Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("Currency", "Update")]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateCurrencyStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateStatusAsync(id, request, CurrentUserId(), cancellationToken));

    // Tidak ada DELETE — FIN-API-1.0 (locked) sengaja tidak mengunci endpoint hapus mata uang;
    // riwayat kurs bergantung padanya, sehingga nonaktifkan (PATCH status) adalah satu-satunya jalur.

    [HttpGet("{id:guid}/exchange-rates")]
    [AccessAction("Read", "Read Currency", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("Currency", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ExchangeRateResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExchangeRates(Guid id, [FromQuery] ExchangeRateQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return Ok(ApiResponse<PagedResult<ExchangeRateResponse>>.Ok(
                await _service.GetExchangeRatesAsync(id, request, cancellationToken), "Riwayat kurs berhasil diambil."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost("{id:guid}/exchange-rates")]
    [AccessAction("Update", "Update Currency", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("Currency", "Update")]
    public async Task<IActionResult> CreateExchangeRate(Guid id, [FromBody] CreateExchangeRateRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.CreateExchangeRateAsync(id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<ExchangeRateResponse>.Ok(result, "Kurs harian berhasil dicatat."));
        }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (CurrencyConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task<CurrencyResponse>> command)
    {
        try { return Ok(ApiResponse<CurrencyResponse>.Ok(await command(), "Mata uang berhasil diproses.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (CurrencyConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (CurrencyValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
