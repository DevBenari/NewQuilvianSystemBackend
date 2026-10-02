using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Dtos;
using QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.BillingManagement.Billing.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/billing-management/consumer-handoffs")]
[AccessController("HEALTH_SERVICE_BILLING_MANAGEMENT_CONSUMER_HANDOFF", "Health Service Billing Management", "Billing Consumer Handoffs",
    AreaName = "HealthServices", ControllerName = "BillingConsumerHandoff", Description = "Operational query and acknowledgement for unconsumed handoff letters", SortOrder = 6)]
[Tags("Health Services / Billing Management / Consumer Handoffs")]
public sealed class BillingConsumerHandoffsController : ControllerBase
{
    private readonly BilConsumerHandoffService _service;

    public BillingConsumerHandoffsController(BilConsumerHandoffService service)
    {
        _service = service;
    }

    /// <summary>
    /// Melihat daftar surat yang belum diambil modul konsumen (Finance/Farmasi), disaring jenis dan rentang waktu.
    /// Mematuhi BKC-DEC-108, BIL-API-1.3, BIL-SCR-41.
    /// </summary>
    [HttpGet("pending")]
    [AccessAction("Read", "Read Pending Billing Consumer Handoffs", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("BillingConsumerHandoff", "Read")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<PendingHandoffResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingHandoffs(
        [FromQuery] PendingHandoffQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetPendingHandoffsAsync(query, cancellationToken);
            return Ok(ApiResponse<PagedResult<PendingHandoffResponse>>.Ok(
                result, "Daftar surat handoff menggantung berhasil diambil."));
        }
        catch (BillingConsumerHandoffValidationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    /// <summary>
    /// Mencatat bahwa modul konsumen sudah mengambil suratnya (BKC-DEC-108, BIL-API-1.3).
    /// Menolak pengakuan kedua berturut-turut dengan status 409 Conflict tanpa mengubah data (BIL-AT-142).
    /// </summary>
    [HttpPatch("{id:guid}/acknowledge")]
    [AccessAction("Acknowledge", "Acknowledge Billing Consumer Handoff", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("BillingConsumerHandoff", "Acknowledge")]
    [ProducesResponseType(typeof(ApiResponse<HandoffResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AcknowledgeHandoff(
        [FromRoute] Guid id,
        [FromBody] AcknowledgeHandoffRequest? request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.AcknowledgeHandoffAsync(
                id, request, CurrentUserId(), cancellationToken);
            return Ok(ApiResponse<HandoffResponse>.Ok(
                result, result.Message ?? "Surat handoff berhasil diakui."));
        }
        catch (KeyNotFoundException exception)
        {
            return NotFound(ApiResponse<object>.Fail(
                StatusCodes.Status404NotFound, exception.Message));
        }
        catch (BillingConsumerHandoffConflictException exception)
        {
            return Conflict(ApiResponse<object>.Fail(
                StatusCodes.Status409Conflict, exception.Message));
        }
        catch (BillingConsumerHandoffValidationException exception)
        {
            return BadRequest(ApiResponse<object>.Fail(
                StatusCodes.Status400BadRequest, exception.Message));
        }
    }

    private Guid CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("user_id");
        return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
    }
}
