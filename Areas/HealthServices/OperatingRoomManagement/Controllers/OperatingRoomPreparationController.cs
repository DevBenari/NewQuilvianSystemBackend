using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Enums;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/cases/{caseId:guid}/preparation")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Preparation",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomPreparation",
    Description = "Persiapan, checklist keselamatan, dan kesiapan kasus operasi",
    SortOrder = 3)]
[Tags("Health Services / Operating Room Management / Preparation")]
public class OperatingRoomPreparationController(OperatingRoomPreparationService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<OprPreparationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Preparation", Description = "Melihat consent, checklist, dan sign-off kesiapan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomPreparation", "Read")]
    public async Task<IActionResult> Get(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<OprPreparationResponse>.Ok(result, "Data persiapan operasi berhasil diambil."));
    }

    [HttpPut("checklists/{phase}")]
    [ProducesResponseType(typeof(ApiResponse<OprChecklistResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Preparation", Description = "Menyimpan checklist keselamatan per fase", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomPreparation", "Update")]
    public async Task<IActionResult> SaveChecklist(Guid caseId, OprChecklistPhase phase,
        [FromBody] SaveOprChecklistRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.SaveChecklistAsync(caseId, phase, request, cancellationToken);
            return Ok(ApiResponse<OprChecklistResponse>.Ok(result, "Checklist keselamatan berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPost("sign-offs")]
    [ProducesResponseType(typeof(ApiResponse<OprPreparationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Preparation", Description = "Memberikan sign-off kesiapan operasi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomPreparation", "Update")]
    public async Task<IActionResult> CreateSignOff(Guid caseId,
        [FromBody] CreateOprReadinessSignOffRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateSignOffAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprPreparationResponse>.Ok(result, "Sign-off kesiapan berhasil dicatat."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPost("emergency-bypass")]
    [ProducesResponseType(typeof(ApiResponse<OprPreparationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Preparation", Description = "Mencatat jalur darurat persiapan operasi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomPreparation", "Update")]
    public async Task<IActionResult> CreateEmergencyBypass(Guid caseId,
        [FromBody] CreateOprEmergencyBypassRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateEmergencyBypassAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprPreparationResponse>.Ok(result, "Jalur darurat persiapan berhasil dicatat."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
