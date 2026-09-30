using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

/// <summary>
/// Antrean rekonsiliasi penagihan Rawat Jalan — <c>BE-RJE-012</c>, <c>FR-RJE-052</c>,
/// <c>RJ-E2E-DEC-009</c>.
/// </summary>
/// <remarks>
/// Arketipe transaksi <b>worklist</b>: daftar ber-saringan dan dua aksi (<c>retry</c>, <c>resolve</c>)
/// sebagai <c>POST /{itemType}/{id}/&lt;aksi&gt;</c>. Tidak ada <c>DELETE</c> maupun
/// <c>PATCH /status</c> generik — keputusan manual dicatat, bukan menghapus riwayat.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/billing/charge-reconciliations")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_BILLING_MANAGEMENT",
    moduleName: "Health Service Billing Management",
    displayName: "Billing Charge Reconciliation",
    AreaName = "HealthServices",
    ControllerName = "BillingChargeReconciliation",
    Description = "Antrean fakta klinis dan baris folio yang belum masuk tagihan",
    SortOrder = 4)]
[Tags("Health Services / Billing Management / Billing / Charge Reconciliations")]
public sealed class BillingChargeReconciliationController : ControllerBase
{
    private readonly BillingChargeReconciliationService _service;

    public BillingChargeReconciliationController(BillingChargeReconciliationService service) => _service = service;

    /// <summary>Daftar antrean; <c>Status</c> kosong berarti item yang belum selesai.</summary>
    [HttpGet]
    [AccessAction("Read", "Read Billing Charge Reconciliation", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingChargeReconciliation", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ChargeReconciliationItemResponse>>), StatusCodes.Status200OK)]
    public Task<IActionResult> List([FromQuery] ChargeReconciliationQuery query, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<PagedResult<ChargeReconciliationItemResponse>>.Ok(
            await _service.ListAsync(query, cancellationToken), "Antrean rekonsiliasi berhasil diambil.")));

    /// <summary>Mengirim ulang satu item memakai identitas yang sama.</summary>
    /// <remarks><c>422 RJE-VAL-020</c> item sudah tercatat; <c>409 RJE-VAL-021</c> sudah diselesaikan;
    /// <c>422 RJE-VAL-024</c> jenis item salah; <c>409 RJE-VAL-025</c> sedang diproses.</remarks>
    [HttpPost("{itemType}/{id:guid}/retry")]
    [AccessAction("Update", "Retry or Resolve Billing Charge Reconciliation", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("BillingChargeReconciliation", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ChargeReconciliationItemResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Retry(string itemType, Guid id, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<ChargeReconciliationItemResponse>.Ok(
            await _service.RetryAsync(itemType, id, CurrentUserId(), cancellationToken), "Item berhasil dikirim ulang.")));

    /// <summary>Menyatakan item selesai secara manual beserta alasannya.</summary>
    /// <remarks><c>422 RJE-VAL-022</c> catatan kurang dari 10 atau lebih dari 500 karakter;
    /// <c>422 RJE-VAL-023</c> jenis penyelesaian salah; kode lain sama dengan <c>retry</c>.</remarks>
    [HttpPost("{itemType}/{id:guid}/resolve")]
    [AccessAction("Update", "Retry or Resolve Billing Charge Reconciliation", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("BillingChargeReconciliation", "Update")]
    [ProducesResponseType(typeof(ApiResponse<ChargeReconciliationItemResponse>), StatusCodes.Status200OK)]
    public Task<IActionResult> Resolve(
        string itemType, Guid id, [FromBody] ResolveChargeReconciliationRequest request, CancellationToken cancellationToken) =>
        RunAsync(async () => Ok(ApiResponse<ChargeReconciliationItemResponse>.Ok(
            await _service.ResolveAsync(itemType, id, request, CurrentUserId(), cancellationToken), "Item berhasil diselesaikan.")));

    private async Task<IActionResult> RunAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (ChargeReconciliationException exception)
        {
            object? errors = exception.Code == null ? null : new { exception.Code };
            return StatusCode(exception.StatusCode, ApiResponse<object>.Fail(exception.StatusCode, exception.Message, errors));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
