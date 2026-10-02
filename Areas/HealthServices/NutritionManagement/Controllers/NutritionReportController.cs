using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;

/// <summary>
/// Laporan operasional Gizi: pelayanan, diet pasien, dan kebutuhan nutrisi (`GIZ-DEC-013`).
/// </summary>
/// <remarks>
/// Seluruh jalur hanya membaca. Laporan menyusun ulang data yang sudah ada dan tidak menetapkan
/// aturan klinis baru.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/nutrition-management/reports")]
[Tags("Health Services / Nutrition Management / Reports")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_NUTRITION_MANAGEMENT",
    moduleName: "Health Service Nutrition Management",
    displayName: "Nutrition Report",
    AreaName = "HealthServices",
    ControllerName = "NutritionReport",
    Description = "Laporan pelayanan gizi, diet pasien, dan kebutuhan nutrisi",
    SortOrder = 5)]
public class NutritionReportController(NutritionReportService service) : ControllerBase
{
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponse<GziReportSummary>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Report",
        Description = "Ringkasan angka laporan gizi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("NutritionReport", "Read")]
    public async Task<IActionResult> GetSummary([FromQuery] GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var data = await service.GetSummaryAsync(request, cancellationToken);
        return Ok(ApiResponse<GziReportSummary>.Ok(data, "Ringkasan laporan gizi berhasil diambil."));
    }

    [HttpGet("services")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GziServiceReportRow>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Report",
        Description = "Laporan pelayanan gizi per kunjungan", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("NutritionReport", "Read")]
    public async Task<IActionResult> GetServices([FromQuery] GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var data = await service.GetServicesAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<GziServiceReportRow>>.Ok(data,
            "Laporan pelayanan gizi berhasil diambil."));
    }

    [HttpGet("diets")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GziDietReportRow>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Report",
        Description = "Laporan diet pasien", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("NutritionReport", "Read")]
    public async Task<IActionResult> GetDiets([FromQuery] GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var data = await service.GetDietsAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<GziDietReportRow>>.Ok(data,
            "Laporan diet pasien berhasil diambil."));
    }

    [HttpGet("requirements")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GziRequirementReportRow>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Report",
        Description = "Laporan kebutuhan nutrisi beserta revisinya", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("NutritionReport", "Read")]
    public async Task<IActionResult> GetRequirements([FromQuery] GziReportQuery request,
        CancellationToken cancellationToken = default)
    {
        var data = await service.GetRequirementsAsync(request, cancellationToken);
        return Ok(ApiResponse<PagedResult<GziRequirementReportRow>>.Ok(data,
            "Laporan kebutuhan nutrisi berhasil diambil."));
    }
}
