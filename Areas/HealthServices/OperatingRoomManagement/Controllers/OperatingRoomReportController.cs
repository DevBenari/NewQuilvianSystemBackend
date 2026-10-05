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
[Route("api/v1/health-services/operating-room-management/reports")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_OPERATING_ROOM_MANAGEMENT",
    moduleName: "Health Service Operating Room Management",
    displayName: "Operating Room Report",
    AreaName = "HealthServices",
    ControllerName = "OperatingRoomReport",
    Description = "Laporan kasus, pemakaian ruang, dan traceability material operasi",
    SortOrder = 8)]
[Tags("Health Services / Operating Room Management / Reports")]
public class OperatingRoomReportController(OperatingRoomReportService service) : ControllerBase
{
    [HttpGet("operations")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OprOperationReportRow>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Laporan kasus, tindakan, durasi, dan status operasi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetOperations([FromQuery] OprReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetOperationsAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<OprOperationReportRow>>.Ok(result, "Laporan kasus operasi berhasil diambil."));
    }

    [HttpGet("utilization")]
    [ProducesResponseType(typeof(ApiResponse<OprUtilizationReport>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [AccessAction("Read", "Read Operating Room Case", Description = "Laporan pemakaian ruang dan penundaan", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomCase", "Read")]
    public async Task<IActionResult> GetUtilization([FromQuery] OprUtilizationQuery request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.GetUtilizationAsync(request, cancellationToken);
            return Ok(ApiResponse<OprUtilizationReport>.Ok(result, "Laporan pemakaian ruang operasi berhasil diambil."));
        }
        catch (ArgumentException ex) { return BadRequest(ApiResponse<object>.Fail(400, ex.Message)); }
    }

    [HttpGet("materials")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<OprMaterialReportRow>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Operating Room Material", Description = "Laporan traceability material dan implant", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("OperatingRoomMaterial", "Read")]
    public async Task<IActionResult> GetMaterials([FromQuery] OprMaterialReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.GetMaterialsAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<OprMaterialReportRow>>.Ok(result,
            "Laporan traceability material operasi berhasil diambil."));
    }
}
