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
    displayName: "Operating Room Case",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomCase",
    Description = "Pengelolaan kasus operasi pasien",
    SortOrder = 1)]
[Tags("Health Services / Operating Room Management / Cases")]
public class OperatingRoomCaseController(OperatingRoomCaseService service,
    OperatingRoomExecutionService executionService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OprCaseSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Melihat daftar kasus operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] OprCasePagedQuery request, CancellationToken cancellationToken = default)
    {
        var result = await service.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<OprCaseSummaryResponse>>.Ok(result, "Daftar kasus operasi berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Melihat detail kasus operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await service.GetDetailAsync(id, cancellationToken);
        return result == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, "Kasus operasi tidak ditemukan."))
            : Ok(ApiResponse<OprCaseDetailResponse>.Ok(result, "Detail kasus operasi berhasil diambil."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [AccessAction("Create", "Create Operating Room Case", Description = "Membuat permintaan operasi", AccessType = AccessTypes.Create, SortOrder = 2)]
    [AccessPermission("OperatingRoomCase", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateOprCaseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.CreateAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = result.Id },
                ApiResponse<OprCaseDetailResponse>.Ok(result, "Permintaan operasi berhasil dibuat."));
        }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [AccessAction("Update", "Update Operating Room Case", Description = "Memperbarui permintaan operasi", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("OperatingRoomCase", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOprCaseRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprCaseDetailResponse>.Ok(result, "Permintaan operasi berhasil diperbarui."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPatch("{id:guid}/start")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Execution", Description = "Memulai operasi", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("OperatingRoomExecution", "Update")]
    public async Task<IActionResult> Start(Guid id, [FromBody] StartOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await executionService.StartAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprCaseStatusResponse>.Ok(result, "Operasi berhasil dimulai."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpPatch("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<OprCaseStatusResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Cancel", "Cancel Operating Room Case", Description = "Membatalkan kasus operasi sebelum dimulai", AccessType = AccessTypes.Delete, SortOrder = 5)]
    [AccessPermission("OperatingRoomCase", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOprCaseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await executionService.CancelAsync(id, request, cancellationToken);
            return Ok(ApiResponse<OprCaseStatusResponse>.Ok(result, "Kasus operasi berhasil dibatalkan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomConflictException ex) { return this.OperatingRoomConflict(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
