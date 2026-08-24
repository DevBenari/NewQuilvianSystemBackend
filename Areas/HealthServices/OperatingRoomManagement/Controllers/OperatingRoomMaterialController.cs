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
[Route("api/v1/health-services/operating-room-management/cases/{caseId:guid}/execution")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Material",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomMaterial",
    Description = "Ledger pemakaian material dan implant operasi",
    SortOrder = 6)]
[Tags("Health Services / Operating Room Management / Execution")]
public class OperatingRoomMaterialController(OperatingRoomMaterialService service) : ControllerBase
{
    [HttpGet("materials")]
    [ProducesResponseType(typeof(ApiResponse<OprMaterialLedgerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Material", Description = "Melihat ledger pemakaian material operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomMaterial", "Read")]
    public async Task<IActionResult> GetLedger(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetLedgerAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<OprMaterialLedgerResponse>.Ok(result, "Ledger material operasi berhasil diambil."));
    }

    [HttpPost("materials")]
    [ProducesResponseType(typeof(ApiResponse<OprMaterialUsageResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Material", Description = "Mencatat pemakaian, retur, waste, atau koreksi material", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomMaterial", "Update")]
    public async Task<IActionResult> Record(Guid caseId, [FromBody] CreateOprMaterialUsageRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.RecordAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprMaterialUsageResponse>.Ok(result, "Pemakaian material berhasil dicatat."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
