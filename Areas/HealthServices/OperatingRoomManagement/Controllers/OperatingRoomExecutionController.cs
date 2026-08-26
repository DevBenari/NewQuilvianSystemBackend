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
    displayName: "Operating Room Execution",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomExecution",
    Description = "Catatan pelaksanaan operasi dan addendum",
    SortOrder = 4)]
[Tags("Health Services / Operating Room Management / Execution")]
public class OperatingRoomExecutionController(OperatingRoomExecutionService service) : ControllerBase
{
    [HttpGet("operation-record")]
    [ProducesResponseType(typeof(ApiResponse<OprExecutionRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Execution", Description = "Melihat catatan pelaksanaan operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetRecord(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetRecordAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Catatan operasi belum tersedia."))
            : Ok(ApiResponse<OprExecutionRecordResponse>.Ok(result, "Catatan operasi berhasil diambil."));
    }

    [HttpPut("operation-record")]
    [ProducesResponseType(typeof(ApiResponse<OprExecutionRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Execution", Description = "Menyimpan atau memfinalisasi catatan operasi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomExecution", "Update")]
    public async Task<IActionResult> SaveRecord(Guid caseId, [FromBody] SaveOprExecutionRecordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.SaveRecordAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprExecutionRecordResponse>.Ok(result, "Catatan operasi berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPost("operation-record/addenda")]
    [ProducesResponseType(typeof(ApiResponse<OprExecutionAddendumResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [AccessAction("Update", "Update Operating Room Execution", Description = "Menambah addendum catatan operasi final", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomExecution", "Update")]
    public async Task<IActionResult> CreateAddendum(Guid caseId,
        [FromBody] CreateOprExecutionAddendumRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateAddendumAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprExecutionAddendumResponse>.Ok(result, "Addendum catatan operasi berhasil dicatat."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
