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
[Route("api/v1/health-services/operating-room-management/cases")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Schedule",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomSchedule",
    Description = "Penjadwalan ruang, tim, dan penundaan kasus operasi",
    SortOrder = 2)]
[Tags("Health Services / Operating Room Management / Cases")]
public class OperatingRoomScheduleController(OperatingRoomSchedulingService service) : ControllerBase
{
    [HttpGet("{id:guid}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<OprScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Melihat jadwal dan tim kasus operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetCurrentSchedule(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await service.GetCurrentScheduleAsync(id, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Jadwal kasus operasi belum tersedia."))
            : Ok(ApiResponse<OprScheduleResponse>.Ok(result, "Jadwal kasus operasi berhasil diambil."));
    }

    [HttpPatch("{id:guid}/schedule")]
    [ProducesResponseType(typeof(ApiResponse<OprScheduleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Schedule", Description = "Menetapkan atau merevisi jadwal dan tim operasi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomSchedule", "Update")]
    public async Task<IActionResult> Schedule(Guid id, [FromBody] ScheduleOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.ScheduleAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprScheduleResponse>.Ok(result, "Jadwal operasi berhasil ditetapkan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPatch("{id:guid}/postpone")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Schedule", Description = "Menunda kasus operasi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomSchedule", "Update")]
    public async Task<IActionResult> Postpone(Guid id, [FromBody] PostponeOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.PostponeAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprCaseStatusResponse>.Ok(result, "Kasus operasi berhasil ditunda."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
