using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;

/// <summary>
/// Daftar pasien gizi, diet pasien, produksi makanan, dan distribusi makanan.
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

    // --------------------------------------------------- 1. daftar pasien gizi

    /// <summary>
    /// Seluruh pasien rawat inap aktif beserta diet yang sedang berlaku.
    /// </summary>
    /// <remarks>
    /// Berbeda dari daftar order konsultasi gizi: daftar ini memuat SEMUA pasien rawat
    /// inap, karena setiap pasien yang dirawat perlu makan. Order konsultasi hanya untuk
    /// pasien yang secara khusus dirujuk ke ahli gizi.
    /// </remarks>
    [HttpGet("patients")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GzNutritionPatientResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Patient",
        Description = "Melihat daftar pasien gizi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetPatients([FromQuery] GzNutritionPatientQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetNutritionPatientsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<GzNutritionPatientResponse>>.Ok(data,
            "Daftar pasien gizi berhasil diambil."));
    }

    // --------------------------------------------------------- 2. diet pasien

    /// <summary>Riwayat diet satu kunjungan; diet lama tetap tersimpan utuh.</summary>
    [HttpGet("history/{encounterId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<List<GzPatientDietResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Patient Diet",
        Description = "Melihat riwayat diet satu kunjungan", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetHistory(Guid encounterId, CancellationToken cancellationToken)
    {
        var data = await _service.GetDietHistoryAsync(encounterId, cancellationToken);
        return Ok(ApiResponse<List<GzPatientDietResponse>>.Ok(data,
            "Riwayat diet berhasil diambil."));
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

    // ----------------------------------------------------------- 3. produksi

    [HttpGet("batches")]
    [ProducesResponseType(typeof(ApiResponse<List<GzProductionBatchSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Food Production",
        Description = "Melihat daftar batch produksi", AccessType = AccessTypes.Read, SortOrder = 5)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetBatches([FromQuery] DateOnly? serviceDate,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetBatchesAsync(serviceDate, cancellationToken);
        return Ok(ApiResponse<List<GzProductionBatchSummaryResponse>>.Ok(data,
            "Daftar batch produksi berhasil diambil."));
    }

    /// <summary>
    /// Detail satu batch: rekap porsi untuk dapur, daftar distribusi per pasien, dan
    /// penanda pasien yang dietnya berubah setelah batch dibuat.
    /// </summary>
    [HttpGet("batches/{batchId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GzProductionBatchDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Food Production",
        Description = "Melihat detail batch dan distribusinya", AccessType = AccessTypes.Read, SortOrder = 6)]
    [AccessPermission("NutritionPatientDiet", "Read")]
    public async Task<IActionResult> GetBatchDetail(Guid batchId, CancellationToken cancellationToken)
    {
        var data = await _service.GetBatchDetailAsync(batchId, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Batch produksi tidak ditemukan."))
            : Ok(ApiResponse<GzProductionBatchDetailResponse>.Ok(data,
                "Detail batch produksi berhasil diambil."));
    }

    [HttpPost("batches")]
    [ProducesResponseType(typeof(ApiResponse<GzProductionBatchDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Food Production",
        Description = "Membuat batch produksi makanan", AccessType = AccessTypes.Create, SortOrder = 7)]
    [AccessPermission("NutritionPatientDiet", "Update")]
    public async Task<IActionResult> CreateBatch([FromBody] CreateGzProductionBatchRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CreateBatchAsync(request, cancellationToken);
            return Ok(ApiResponse<GzProductionBatchDetailResponse>.Ok(data,
                "Batch produksi berhasil dibuat."));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    [HttpPost("batches/{batchId:guid}/status")]
    [ProducesResponseType(typeof(ApiResponse<GzProductionBatchDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Food Production",
        Description = "Mengubah status batch produksi", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("NutritionPatientDiet", "Update")]
    public async Task<IActionResult> ChangeBatchStatus(Guid batchId,
        [FromBody] ChangeGzBatchStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.ChangeBatchStatusAsync(batchId, request, cancellationToken);
            return Ok(ApiResponse<GzProductionBatchDetailResponse>.Ok(data,
                "Status batch produksi berhasil diperbarui."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    // --------------------------------------------------------- 4. distribusi

    [HttpPost("distribution")]
    [ProducesResponseType(typeof(ApiResponse<GzProductionBatchDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Food Distribution",
        Description = "Mencatat penyerahan makanan", AccessType = AccessTypes.Update, SortOrder = 9)]
    [AccessPermission("NutritionPatientDiet", "Update")]
    public async Task<IActionResult> RecordDelivery(
        [FromBody] RecordGzMealDeliveryRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.RecordDeliveryAsync(request, cancellationToken);
            return Ok(ApiResponse<GzProductionBatchDetailResponse>.Ok(data,
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
