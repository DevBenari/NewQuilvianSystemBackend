using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

/// <summary>
/// Daftar serah terima pasca operasi per unit tujuan, baca saja (<c>BE-RWI-177</c>, API 11.5.3).
/// Bangsal dan Daftar Pantau membaca dari sini dengan hak <c>OperatingRoomHandover : Read</c>.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/handovers")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Handover Queue",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomHandoverQuery",
    Description = "Daftar serah terima pasien pasca operasi per unit tujuan",
    SortOrder = 9)]
[Tags("Health Services / Operating Room Management / Handovers")]
public class OperatingRoomHandoverQueryController(OperatingRoomHandoverQueryService service) : ControllerBase
{
    /// <summary>Serah terima per unit tujuan dan status; <c>overdueOnly</c> memakai ambang pengaturan.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<HandoverQueueItemResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Operating Room Handover", Description = "Melihat daftar serah terima pasien pasca operasi per unit", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomHandover", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] HandoverQueueQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetPagedAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<HandoverQueueItemResponse>>.Ok(result,
            "Daftar serah terima pasca operasi berhasil diambil."));
    }
}
