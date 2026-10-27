using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

/// <summary>
/// Endpoint pencatatan kelahiran bayi intraoperatif pada persalinan Sectio Caesarea (BE-OPR-014, OPS-DEC-041, OPS-DEC-042).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/cases/{caseId:guid}/execution")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Birth Event",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomBirthEvent",
    Description = "Pencatatan kelahiran bayi intraoperatif meja bedah SC",
    SortOrder = 8)]
[Tags("Health Services / Operating Room Management / Execution")]
public class OperatingRoomBirthEventController(OperatingRoomExecutionService executionService) : ControllerBase
{
    /// <summary>
    /// Mencatat kejadian kelahiran bayi intraoperatif dan menerbitkan event integrasi ke admisi neonatus.
    /// </summary>
    [HttpPost("birth-event")]
    [ProducesResponseType(typeof(ApiResponse<OprBirthEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Create", "Record Birth Event", Description = "Mencatat peristiwa kelahiran bayi intraoperatif SC", AccessType = AccessTypes.Create, SortOrder = 1)]
    [AccessPermission("OperatingRoomExecution", "Create")]
    public async Task<IActionResult> RecordBirthEvent(
        Guid caseId,
        [FromBody] RecordBirthEventRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await executionService.RecordBirthEventAsync(caseId, request, cancellationToken);
            return Ok(ApiResponse<OprBirthEventResponse>.Ok(result, "Data kelahiran bayi intraoperatif berhasil dicatat."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
}
