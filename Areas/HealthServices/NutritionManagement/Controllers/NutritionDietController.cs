using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;

/// <summary>
/// Diet pasien, rekap produksi makanan, dan distribusi makanan.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/nutrition-management/diets")]
[Tags("Health Services / Nutrition Management / Patient Diet")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_NUTRITION_MANAGEMENT",
    moduleName: "Health Service Nutrition Management",
    displayName: "Nutrition Patient Diet",
    AreaName = "HealthServices",
    ControllerName = "NutritionPatientDiet",
    Description = "Diet, produksi, dan distribusi makanan pasien",
    SortOrder = 2)]
public class NutritionDietController : ControllerBase
{
    private readonly NutritionDietService _service;

    public NutritionDietController(NutritionDietService service) => _service = service;

    /// <summary>Seluruh diet yang sedang berlaku. Inilah yang dibaca dapur.</summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(ApiResponse<List<GzPatientDietResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Patient Diet",
        Description = "Melihat diet pasien yang sedang berlaku", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetActive(CancellationToken cancellationToken)
    {
        var data = await _service.GetActiveDietsAsync(cancellationToken);
        return Ok(ApiResponse<List<GzPatientDietResponse>>.Ok(data,
            "Data diet aktif berhasil diambil."));
    }

    /// <summary>Riwayat diet pada satu order, termasuk diet yang sudah diganti.</summary>
    [HttpGet("by-order/{orderId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<GzPatientDietResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Patient Diet",
        Description = "Melihat riwayat diet satu pasien", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetByOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var data = await _service.GetDietsByOrderAsync(orderId, cancellationToken);
        return Ok(ApiResponse<List<GzPatientDietResponse>>.Ok(data,
            "Riwayat diet pasien berhasil diambil."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GzPatientDietResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Patient Diet",
        Description = "Menetapkan atau mengubah diet pasien", AccessType = AccessTypes.Update, SortOrder = 3)]
    [AccessPermission("NutritionPatientDiet", "Update")]
    public async Task<IActionResult> Prescribe([FromBody] PrescribeGzDietRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.PrescribeAsync(request, cancellationToken);
            return Ok(ApiResponse<GzPatientDietResponse>.Ok(data, "Diet pasien berhasil ditetapkan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    [HttpPost("{dietId:guid}/stop")]
    [ProducesResponseType(typeof(ApiResponse<GzPatientDietResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Patient Diet",
        Description = "Menghentikan diet pasien", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("NutritionPatientDiet", "Update")]
    public async Task<IActionResult> Stop(Guid dietId, [FromBody] StopGzDietRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.StopAsync(dietId, request, cancellationToken);
            return Ok(ApiResponse<GzPatientDietResponse>.Ok(data, "Diet pasien berhasil dihentikan."));
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
    /// Rekap kebutuhan produksi. Seluruhnya hasil hitungan atas diet yang sedang aktif,
    /// bukan data tersimpan, sehingga dapur tidak pernah memasak dari angka basi.
    /// </summary>
    [HttpGet("production")]
    [ProducesResponseType(typeof(ApiResponse<List<GzProductionSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Food Production",
        Description = "Melihat rekap kebutuhan produksi makanan", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetProduction([FromQuery] DateOnly? serviceDate,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetProductionSummaryAsync(serviceDate, cancellationToken);
        return Ok(ApiResponse<List<GzProductionSummaryResponse>>.Ok(data,
            "Rekap kebutuhan produksi berhasil dihitung."));
    }

    [HttpGet("distribution")]
    [ProducesResponseType(typeof(ApiResponse<List<GzMealDistributionRowResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Food Distribution",
        Description = "Melihat daftar distribusi makanan", AccessType = AccessTypes.Read, SortOrder = 6)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetDistribution([FromQuery] Guid mealScheduleId,
        [FromQuery] DateOnly? serviceDate, CancellationToken cancellationToken)
    {
        var data = await _service.GetDistributionAsync(mealScheduleId, serviceDate, cancellationToken);
        return Ok(ApiResponse<List<GzMealDistributionRowResponse>>.Ok(data,
            "Daftar distribusi makanan berhasil diambil."));
    }

    [HttpPost("distribution")]
    [ProducesResponseType(typeof(ApiResponse<GzMealDistributionRowResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Food Distribution",
        Description = "Mencatat penyerahan makanan", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("NutritionPatientDiet", "Update")]
    public async Task<IActionResult> RecordDelivery(
        [FromBody] RecordGzMealDeliveryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.RecordDeliveryAsync(request, cancellationToken);
            return Ok(ApiResponse<GzMealDistributionRowResponse>.Ok(data,
                "Penyerahan makanan berhasil dicatat."));
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
