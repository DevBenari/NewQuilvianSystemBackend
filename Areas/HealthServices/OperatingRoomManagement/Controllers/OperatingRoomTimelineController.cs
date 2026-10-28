using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

/// <summary>
/// Endpoint visualisasi timeline jadwal ruang kamar bedah dan sterilisasi (BE-OPR-012, OPS-DEC-038, OPS-DEC-039).
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/rooms")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Timeline",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomTimeline",
    Description = "Visualisasi timeline jadwal ruang kamar bedah dan sterilisasi",
    SortOrder = 2)]
[Tags("Health Services / Operating Room Management / Scheduling")]
public class OperatingRoomTimelineController(OperatingRoomSchedulingService service) : ControllerBase
{
    /// <summary>
    /// Menampilkan timeline jadwal seluruh ruang OK dalam bentuk slot (Occupied, SterilizationBuffer, Available).
    /// </summary>
    [HttpGet("timeline")]
    [ProducesResponseType(typeof(ApiResponse<List<OprRoomTimelineResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [AccessAction("Read", "Read Operating Room Timeline", Description = "Melihat visualisasi timeline jadwal ruang operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomSchedule", "Read")]
    public async Task<IActionResult> GetTimeline(
        [FromQuery] string date,
        [FromQuery] string? floorLocation = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.GetRoomsTimelineAsync(date, floorLocation, cancellationToken);
            return Ok(ApiResponse<List<OprRoomTimelineResponse>>.Ok(result, "Timeline ruang operasi berhasil diambil."));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(StatusCodes.Status400BadRequest, ex.Message));
        }
    }
}
