using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.OperatingRoomManagement.Controllers;

/// <summary>
/// Pemetaan kamar operasi ke depo farmasi sumber stoknya.
/// </summary>
/// <remarks>
/// Kamar operasi tidak memiliki saldo stok sendiri. Pemetaan inilah yang menentukan depo mana
/// yang stoknya berkurang ketika kamar operasi mencatat pemakaian material.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/operating-room-management/stock-sources")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Stock Source",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomStockSource",
    Description = "Pemetaan kamar operasi ke depo farmasi",
    SortOrder = 8)]
[Tags("Health Services / Operating Room Management / Stock Source")]
public class OperatingRoomStockSourceController(OperatingRoomStockSourceService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<OprStockSourceResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Operating Room Stock Source",
        Description = "Melihat pemetaan kamar operasi ke depo farmasi",
        AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomStockSource", "Read")]
    public async Task<IActionResult> List([FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var result = await service.ListAsync(includeInactive, cancellationToken);
        return Ok(ApiResponse<List<OprStockSourceResponse>>.Ok(result, "Pemetaan berhasil diambil."));
    }

    [HttpPut]
    [ProducesResponseType(typeof(ApiResponse<OprStockSourceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    [AccessAction("Update", "Update Operating Room Stock Source",
        Description = "Menetapkan depo farmasi sumber stok kamar operasi",
        AccessType = AccessTypes.Update, SortOrder = 2)]
    [AccessPermission("OperatingRoomStockSource", "Update")]
    public async Task<IActionResult> Save([FromBody] SaveOprStockSourceRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.SaveAsync(request, cancellationToken);
            return Ok(ApiResponse<OprStockSourceResponse>.Ok(result, "Pemetaan berhasil disimpan."));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<object>.Fail(404, ex.Message)); }
        catch (OperatingRoomForbiddenException ex) { return this.OperatingRoomForbidden(ex); }
        catch (OperatingRoomUnprocessableException ex) { return this.OperatingRoomUnprocessable(ex); }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }
}
