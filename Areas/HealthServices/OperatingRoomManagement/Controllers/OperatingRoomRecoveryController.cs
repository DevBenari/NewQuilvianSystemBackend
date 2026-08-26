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
    displayName: "Operating Room Recovery",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomRecovery",
    Description = "Catatan anestesi, recovery, dan serah terima pasien operasi",
    SortOrder = 5)]
[Tags("Health Services / Operating Room Management / Execution")]
public class OperatingRoomRecoveryController(OperatingRoomRecoveryService service) : ControllerBase
{
    [HttpGet("anesthesia-record")]
    [ProducesResponseType(typeof(ApiResponse<OprAnesthesiaRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Anesthesia", Description = "Melihat catatan anestesi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomAnesthesia", "Read")]
    public async Task<IActionResult> GetAnesthesiaRecord(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetAnesthesiaRecordAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Catatan anestesi belum tersedia."))
            : Ok(ApiResponse<OprAnesthesiaRecordResponse>.Ok(result, "Catatan anestesi berhasil diambil."));
    }

    [HttpPut("anesthesia-record")]
    [ProducesResponseType(typeof(ApiResponse<OprAnesthesiaRecordResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Anesthesia", Description = "Menyimpan atau memfinalisasi catatan anestesi", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomAnesthesia", "Update")]
    public async Task<IActionResult> SaveAnesthesiaRecord(Guid caseId,
        [FromBody] SaveOprAnesthesiaRecordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.SaveAnesthesiaRecordAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprAnesthesiaRecordResponse>.Ok(result, "Catatan anestesi berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpGet("recovery")]
    [ProducesResponseType(typeof(ApiResponse<OprRecoveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Anesthesia", Description = "Melihat pemantauan recovery", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomAnesthesia", "Read")]
    public async Task<IActionResult> GetRecovery(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetRecoveryAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Data recovery belum tersedia."))
            : Ok(ApiResponse<OprRecoveryResponse>.Ok(result, "Data recovery berhasil diambil."));
    }

    [HttpPut("recovery")]
    [ProducesResponseType(typeof(ApiResponse<OprRecoveryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Anesthesia", Description = "Menyimpan pemantauan dan keputusan recovery", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomAnesthesia", "Update")]
    public async Task<IActionResult> SaveRecovery(Guid caseId, [FromBody] SaveOprRecoveryRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.SaveRecoveryAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprRecoveryResponse>.Ok(result, "Data recovery berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpGet("handovers")]
    [ProducesResponseType(typeof(ApiResponse<OprHandoverResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Handover", Description = "Melihat serah terima pasien terkini", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomHandover", "Read")]
    public async Task<IActionResult> GetHandover(Guid caseId, CancellationToken cancellationToken = default)
    {
        var result = await service.GetCurrentHandoverAsync(caseId, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Serah terima belum tersedia."))
            : Ok(ApiResponse<OprHandoverResponse>.Ok(result, "Serah terima berhasil diambil."));
    }

    [HttpPost("handovers")]
    [ProducesResponseType(typeof(ApiResponse<OprHandoverResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Handover", Description = "Mengirim serah terima pasien", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomHandover", "Update")]
    public async Task<IActionResult> CreateHandover(Guid caseId, [FromBody] CreateOprHandoverRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateHandoverAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprHandoverResponse>.Ok(result, "Serah terima pasien berhasil dikirim."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPatch("handovers/{handoverId:guid}/accept")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Handover", Description = "Menerima atau menolak serah terima pasien", AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomHandover", "Update")]
    public async Task<IActionResult> AcceptHandover(Guid caseId, Guid handoverId,
        [FromBody] AcceptOprHandoverRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.AcceptHandoverAsync(caseId, handoverId, request, cancellationToken);
            return Ok(ApiResponse<OprCaseStatusResponse>.Ok(result, "Serah terima pasien berhasil diproses."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
