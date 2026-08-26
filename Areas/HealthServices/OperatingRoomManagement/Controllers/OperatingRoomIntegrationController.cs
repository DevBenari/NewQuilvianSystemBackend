using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/cases/{caseId:guid}/integration")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Integration",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomIntegration",
    Description = "Rekonsiliasi penyerahan data operasi ke Inventory dan Billing",
    SortOrder = 7)]
[Tags("Health Services / Operating Room Management / Integration")]
public class OperatingRoomIntegrationController(OperatingRoomIntegrationService service) : ControllerBase
{
    [HttpGet("reconciliation")]
    [ProducesResponseType(typeof(ApiResponse<OprReconciliationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Integration", Description = "Melihat status penyerahan data ke Inventory dan Billing", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomIntegration", "Read")]
    public async Task<IActionResult> GetReconciliation(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetReconciliationAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<OprReconciliationResponse>.Ok(result, "Status penyerahan data berhasil diambil."));
    }

    [HttpPatch("deliveries/{deliveryId:guid}/attempts")]
    [ProducesResponseType(typeof(ApiResponse<OprIntegrationDeliveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [AccessAction("Update", "Update Operating Room Integration", Description = "Mencatat hasil upaya pengiriman ke consumer", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomIntegration", "Update")]
    public async Task<IActionResult> RecordAttempt(Guid caseId, Guid deliveryId,
        [FromBody] RecordOprDeliveryAttemptRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.RecordAttemptAsync(caseId, deliveryId, request, cancellationToken);
            return Ok(ApiResponse<OprIntegrationDeliveryResponse>.Ok(result, "Hasil pengiriman berhasil dicatat."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPatch("deliveries/{deliveryId:guid}/retry")]
    [ProducesResponseType(typeof(ApiResponse<OprIntegrationDeliveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [AccessAction("Update", "Update Operating Room Integration", Description = "Mengantrekan ulang pengiriman yang gagal", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomIntegration", "Update")]
    public async Task<IActionResult> Retry(Guid caseId, Guid deliveryId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.RetryAsync(caseId, deliveryId, cancellationToken);
            return Ok(ApiResponse<OprIntegrationDeliveryResponse>.Ok(result, "Pengiriman berhasil diantrekan ulang."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
