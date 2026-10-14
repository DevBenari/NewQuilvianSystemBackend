using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.Receivable.Controllers;

/// <summary>
/// BE-FIN-098, FIN-API-1.9 §P.2. Impor saldo lama piutang manfaat karyawan — satu endpoint,
/// semua-atau-tidak-sama-sekali (M.5.1-M.5.3). Resource baru, berdiri sendiri dari
/// FinanceOpeningItemBatch (lihat ringkasan kelas FinanceReceivableMigrationService).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/receivables/migration-batches")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_RECEIVABLE_MIGRATION", "Corporate Finance Management Receivable Migration", "Receivable Migration",
    AreaName = "Corporate", ControllerName = "FinanceReceivableMigration", Description = "Impor saldo lama piutang manfaat karyawan", SortOrder = 34)]
[Tags("Corporate / Finance Management / Receivable / Migration")]
public sealed class FinanceReceivableMigrationController : ControllerBase
{
    private readonly FinanceReceivableMigrationService _service;
    public FinanceReceivableMigrationController(FinanceReceivableMigrationService service) => _service = service;

    [HttpPost("employee")]
    [AccessAction("Import", "Import Employee Receivable Batch", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("FinanceReceivableMigration", "Import")]
    [ProducesResponseType(typeof(ApiResponse<EmployeeReceivableBatchResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ImportEmployeeBatch([FromForm] ImportEmployeeReceivableBatchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ImportAsync(request, CurrentUserId(), cancellationToken);
            return StatusCode(StatusCodes.Status201Created,
                ApiResponse<EmployeeReceivableBatchResponse>.Ok(result, $"Batch migrasi berhasil diimpor — {result.TotalItemCount} kartu piutang diterbitkan."));
        }
        catch (ReceivableMigrationValidationException exception)
        {
            return UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message,
                exception.RowErrors.Count > 0 ? exception.RowErrors : null));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
