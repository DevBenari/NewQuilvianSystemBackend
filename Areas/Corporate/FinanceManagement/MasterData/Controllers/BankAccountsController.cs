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
[Route("api/v1/corporate/finance-management/master-data/bank-accounts")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_MASTER_DATA", "Corporate Finance Management Master Data", "Bank Account",
    AreaName = "Corporate", ControllerName = "BankAccount", Description = "Master data rekening bank rumah sakit milik Finance", SortOrder = 28)]
[Tags("Corporate / Finance Management / Master Data / Bank Account")]
public sealed class BankAccountsController : ControllerBase
{
    private readonly BankAccountService _service;
    public BankAccountsController(BankAccountService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Bank Account", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BankAccount", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BankAccountFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<BankAccountFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter rekening bank berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Bank Account", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BankAccount", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BankAccountSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<BankAccountSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan rekening bank berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Bank Account", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BankAccount", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BankAccountResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] BankAccountQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<BankAccountResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Rekening bank berhasil diambil."));

    [HttpGet("options")]
    [AccessAction("Read", "Read Bank Account", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BankAccount", "Read")]
    [ProducesResponseType(typeof(ApiResponse<List<BankAccountOptionResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOptions(
        [FromQuery] bool onlyActive = true,
        [FromQuery] string? accountType = null,
        CancellationToken cancellationToken = default) =>
        Ok(ApiResponse<List<BankAccountOptionResponse>>.Ok(
            await _service.GetOptionsAsync(onlyActive, accountType, cancellationToken),
            "Data pilihan rekening bank berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Bank Account", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BankAccount", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BankAccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<BankAccountResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail rekening bank berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    [HttpPost]
    [AccessAction("Create", "Create Bank Account", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("BankAccount", "Create")]
    public Task<IActionResult> Create([FromBody] CreateBankAccountRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.CreateAsync(request, CurrentUserId(), cancellationToken));

    [HttpPut("{id:guid}")]
    [AccessAction("Update", "Update Bank Account", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("BankAccount", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateBankAccountRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpPatch("{id:guid}/status")]
    [AccessAction("Update", "Update Bank Account Status", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("BankAccount", "Update")]
    public Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateBankAccountStatusRequest request, CancellationToken cancellationToken) =>
        ExecuteAsync(() => _service.UpdateStatusAsync(id, request, CurrentUserId(), cancellationToken));

    [HttpDelete("{id:guid}")]
    [AccessAction("Delete", "Delete Bank Account", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("BankAccount", "Delete")]
    [ProducesResponseType(typeof(ApiResponse<BankAccountDeleteResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<BankAccountDeleteResponse>.Ok(await _service.DeleteAsync(id, CurrentUserId(), cancellationToken), "Rekening bank berhasil dihapus.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    private async Task<IActionResult> ExecuteAsync(Func<Task<BankAccountResponse>> command)
    {
        try { return Ok(ApiResponse<BankAccountResponse>.Ok(await command(), "Rekening bank berhasil diproses.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
        catch (BankAccountConflictException exception) { return Conflict(ApiResponse<object>.Fail(409, exception.Message)); }
        catch (BankAccountValidationException exception) { return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)); }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
