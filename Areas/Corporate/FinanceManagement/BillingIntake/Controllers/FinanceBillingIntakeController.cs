using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Dtos;
using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.Corporate.FinanceManagement.BillingIntake.Controllers;

/// <summary>
/// Worklist fakta dari Billing (aggregate ber-lifecycle, bukan master data). Cakupan runtime
/// HANYA HandoffType AR — lihat ringkasan kelas FinanceBillingIntakeService.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/corporate/finance-management/billing-intake")]
[AccessController("CORPORATE_FINANCE_MANAGEMENT_BILLING_INTAKE", "Corporate Finance Management Billing Intake", "Billing Intake",
    AreaName = "Corporate", ControllerName = "BillingIntake", Description = "Pintu masuk fakta dari Billing menjadi piutang", SortOrder = 31)]
[Tags("Corporate / Finance Management / Billing Intake")]
public sealed class FinanceBillingIntakeController : ControllerBase
{
    private readonly FinanceBillingIntakeService _service;
    public FinanceBillingIntakeController(FinanceBillingIntakeService service) => _service = service;

    [HttpGet("filters/metadata")]
    [AccessAction("Read", "Read Billing Intake", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingIntake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BillingIntakeFilterMetadataResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFilterMetadata(CancellationToken cancellationToken) =>
        Ok(ApiResponse<BillingIntakeFilterMetadataResponse>.Ok(
            await _service.GetFilterMetadataAsync(cancellationToken), "Metadata filter fakta masuk berhasil diambil."));

    [HttpGet("summary")]
    [AccessAction("Read", "Read Billing Intake", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingIntake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BillingIntakeSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken) =>
        Ok(ApiResponse<BillingIntakeSummaryResponse>.Ok(
            await _service.GetSummaryAsync(cancellationToken), "Ringkasan fakta masuk berhasil diambil."));

    [HttpGet]
    [AccessAction("Read", "Read Billing Intake", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingIntake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BillingIntakeResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get([FromQuery] BillingIntakeQuery request, CancellationToken cancellationToken) =>
        Ok(ApiResponse<PagedResult<BillingIntakeResponse>>.Ok(
            await _service.GetPagedAsync(request, cancellationToken), "Fakta masuk berhasil diambil."));

    [HttpGet("{id:guid}")]
    [AccessAction("Read", "Read Billing Intake", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingIntake", "Read")]
    [ProducesResponseType(typeof(ApiResponse<BillingIntakeResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try { return Ok(ApiResponse<BillingIntakeResponse>.Ok(await _service.GetByIdAsync(id, cancellationToken), "Detail fakta masuk berhasil diambil.")); }
        catch (KeyNotFoundException exception) { return NotFound(ApiResponse<object>.Fail(404, exception.Message)); }
    }

    /// <summary>Menemukan fakta AR baru dari Billing yang belum tercatat sebagai fakta masuk.</summary>
    [HttpPost("sync")]
    [AccessAction("Sync", "Sync Billing Intake", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("BillingIntake", "Sync")]
    public async Task<IActionResult> Sync(CancellationToken cancellationToken)
    {
        var count = await _service.SyncNewFactsAsync(CurrentUserId(), cancellationToken);
        return Ok(ApiResponse<SyncBillingIntakeResponse>.Ok(
            new SyncBillingIntakeResponse { DiscoveredCount = count },
            count > 0 ? $"{count} fakta baru ditemukan." : "Tidak ada fakta baru."));
    }

    /// <summary>Menjalankan pengolahan satu fakta masuk (UAT-03) — juga dipakai untuk mengulang baris ERROR (FR-FIN-012).</summary>
    [HttpPost("{id:guid}/process")]
    [AccessAction("Process", "Process Billing Intake", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("BillingIntake", "Process")]
    public async Task<IActionResult> Process(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.ProcessAsync(id, CurrentUserId(), cancellationToken);
            var message = result.Status switch
            {
                "ACKNOWLEDGED" => "Fakta berhasil diolah menjadi piutang.",
                "ERROR" => "Pengolahan gagal — lihat pesan galat pada baris ini. Dapat diulang setelah penyebabnya diperbaiki.",
                _ => "Fakta masuk diproses."
            };
            return Ok(ApiResponse<BillingIntakeResponse>.Ok(result, message));
        }
        catch (Exception exception) when (IsHandled(exception)) { return Failure(exception); }
    }

    private IActionResult Failure(Exception exception) => exception switch
    {
        KeyNotFoundException => NotFound(ApiResponse<object>.Fail(404, exception.Message)),
        BillingIntakeValidationException => UnprocessableEntity(ApiResponse<object>.Fail(422, exception.Message)),
        _ => throw exception
    };

    private static bool IsHandled(Exception exception) => exception is KeyNotFoundException or BillingIntakeValidationException;

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
