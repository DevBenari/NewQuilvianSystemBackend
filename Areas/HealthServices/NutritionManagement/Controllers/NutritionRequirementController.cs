using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;

/// <summary>
/// Kebutuhan nutrisi pasien beserta riwayat revisinya, dan diagnosis gizi per kunjungan.
/// </summary>
/// <remarks>
/// Kebutuhan nutrisi tidak pernah ditimpa. Setiap penetapan melahirkan revisi baru, sehingga
/// angka yang dipakai merawat pasien kemarin tetap dapat dibaca hari ini (`GIZ-DEC-012`).
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/nutrition-management/orders/{orderId:guid}")]
[Tags("Health Services / Nutrition Management / Nutrition Requirement")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_NUTRITION_MANAGEMENT",
    moduleName: "Health Service Nutrition Management",
    displayName: "Nutrition Requirement",
    AreaName = "HealthServices",
    ControllerName = "NutritionRequirement",
    Description = "Kebutuhan nutrisi pasien dan riwayat revisinya",
    SortOrder = 4)]
public class NutritionRequirementController : ControllerBase
{
    private readonly NutritionRequirementService _service;

    public NutritionRequirementController(NutritionRequirementService service) => _service = service;

    // --------------------------------------------------------------- pembacaan

    [HttpGet("requirements")]
    [ProducesResponseType(typeof(ApiResponse<List<GziRequirementResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Requirement",
        Description = "Melihat riwayat kebutuhan nutrisi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("NutritionRequirement", "Read")]
    public async Task<IActionResult> GetHistory(Guid orderId, CancellationToken cancellationToken)
    {
        var data = await _service.GetHistoryAsync(orderId, cancellationToken);
        return Ok(ApiResponse<List<GziRequirementResponse>>.Ok(data,
            "Riwayat kebutuhan nutrisi berhasil diambil."));
    }

    [HttpGet("requirements/current")]
    [ProducesResponseType(typeof(ApiResponse<GziRequirementResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Requirement",
        Description = "Melihat kebutuhan nutrisi yang berlaku", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("NutritionRequirement", "Read")]
    public async Task<IActionResult> GetCurrent(Guid orderId, CancellationToken cancellationToken)
    {
        var data = await _service.GetCurrentAsync(orderId, cancellationToken);
        if (data == null)
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Belum ada kebutuhan nutrisi yang ditetapkan pada order ini."));

        return Ok(ApiResponse<GziRequirementResponse>.Ok(data,
            "Kebutuhan nutrisi yang berlaku berhasil diambil."));
    }

    // ----------------------------------------------------------------- perintah

    [HttpPost("requirements")]
    [ProducesResponseType(typeof(ApiResponse<GziRequirementResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Requirement",
        Description = "Menetapkan atau merevisi kebutuhan nutrisi", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("NutritionRequirement", "Update")]
    public async Task<IActionResult> Save(Guid orderId,
        [FromBody] SaveGzRequirementRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.SaveAsync(orderId, request, cancellationToken);
            return Ok(ApiResponse<GziRequirementResponse>.Ok(data,
                "Kebutuhan nutrisi berhasil disimpan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    /// <summary>
    /// Pratinjau kalkulasi. Tidak menyimpan apa pun.
    /// </summary>
    /// <remarks>
    /// Selama registry rumus kosong (`GIZ-OQ-007` ditunda), jawabannya selalu berisi nilai
    /// kosong beserta keterangannya — bukan galat, melainkan keadaan yang memang diputuskan.
    /// </remarks>
    [HttpPost("requirements/calculate")]
    [ProducesResponseType(typeof(ApiResponse<GziCalculationPreviewResponse>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Requirement",
        Description = "Pratinjau kalkulasi kebutuhan nutrisi", AccessType = AccessTypes.Read, SortOrder = 4)]
    [AccessPermission("NutritionRequirement", "Read")]
    public async Task<IActionResult> Calculate(Guid orderId,
        [FromBody] GziCalculationPreviewRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CalculateAsync(request, cancellationToken);
            return Ok(ApiResponse<GziCalculationPreviewResponse>.Ok(data, data.Message));
        }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    // ------------------------------------------------- diagnosis per kunjungan

    [HttpGet("records/{recordId:guid}/diagnoses")]
    [ProducesResponseType(typeof(ApiResponse<List<NutritionCareRecordDiagnosisResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Requirement",
        Description = "Melihat diagnosis gizi pada satu kunjungan", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("NutritionRequirement", "Read")]
    public async Task<IActionResult> GetDiagnoses(Guid orderId, Guid recordId,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetCareRecordDiagnosesAsync(orderId, recordId, cancellationToken);
        return Ok(ApiResponse<List<NutritionCareRecordDiagnosisResponse>>.Ok(data,
            "Diagnosis gizi berhasil diambil."));
    }

    [HttpPut("records/{recordId:guid}/diagnoses")]
    [ProducesResponseType(typeof(ApiResponse<List<NutritionCareRecordDiagnosisResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Requirement",
        Description = "Menyimpan diagnosis gizi pada satu kunjungan", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("NutritionRequirement", "Update")]
    public async Task<IActionResult> SaveDiagnoses(Guid orderId, Guid recordId,
        [FromBody] SaveGzCareRecordDiagnosesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.SaveCareRecordDiagnosesAsync(orderId, recordId, request,
                cancellationToken);
            return Ok(ApiResponse<List<NutritionCareRecordDiagnosisResponse>>.Ok(data,
                "Diagnosis gizi berhasil disimpan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }
}
