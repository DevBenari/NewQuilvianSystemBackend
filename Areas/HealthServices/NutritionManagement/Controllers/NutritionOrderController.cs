using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.NutritionManagement.Controllers;

/// <summary>
/// Order konsultasi gizi dan catatan asuhan gizi pasien rawat inap.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/nutrition-management/orders")]
[Tags("Health Services / Nutrition Management / Nutrition Order")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_NUTRITION_MANAGEMENT",
    moduleName: "Health Service Nutrition Management",
    displayName: "Nutrition Order",
    AreaName = "HealthServices",
    ControllerName = "NutritionOrder",
    Description = "Asuhan gizi pasien rawat inap",
    SortOrder = 1)]
public class NutritionOrderController : ControllerBase
{
    private readonly NutritionOrderService _service;

    public NutritionOrderController(NutritionOrderService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<GzOrderSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Order",
        Description = "Melihat daftar pemesanan konsultasi gizi", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("NutritionOrder", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] GzOrderPagedQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<GzOrderSummaryResponse>>.Ok(data,
            "Data pemesanan konsultasi gizi berhasil diambil."));
    }

    /// <summary>
    /// Pasien rawat inap yang skrining gizinya berisiko tetapi belum punya order.
    /// </summary>
    [HttpGet("screening-candidates")]
    [ProducesResponseType(typeof(ApiResponse<List<GzScreeningCandidateResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Nutrition Order",
        Description = "Melihat pasien berisiko gizi yang belum dirujuk", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("NutritionOrder", "Read")]
    public async Task<IActionResult> GetScreeningCandidates(CancellationToken cancellationToken)
    {
        var data = await _service.GetScreeningCandidatesAsync(cancellationToken);
        return Ok(ApiResponse<List<GzScreeningCandidateResponse>>.Ok(data,
            "Data pasien berisiko gizi berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GzOrderDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Nutrition Order",
        Description = "Melihat detail asuhan gizi", AccessType = AccessTypes.Read, SortOrder = 3)]
    [AccessPermission("NutritionOrder", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetDetailAsync(id, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Order konsultasi gizi tidak ditemukan."))
            : Ok(ApiResponse<GzOrderDetailResponse>.Ok(data, "Detail asuhan gizi berhasil diambil."));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<GzOrderDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Nutrition Order",
        Description = "Membuat order konsultasi gizi", AccessType = AccessTypes.Create, SortOrder = 4)]
    [AccessPermission("NutritionOrder", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateGzOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CreateAsync(request, cancellationToken);
            return Ok(ApiResponse<GzOrderDetailResponse>.Ok(data,
                "Order konsultasi gizi berhasil dibuat."));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<GzOrderDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Order",
        Description = "Mengubah order konsultasi gizi", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("NutritionOrder", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateGzOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<GzOrderDetailResponse>.Ok(data,
                "Order konsultasi gizi berhasil diperbarui."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    [HttpPost("{id:guid}/close")]
    [ProducesResponseType(typeof(ApiResponse<GzOrderDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Order",
        Description = "Menutup asuhan gizi", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("NutritionOrder", "Update")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseGzOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CloseAsync(id, request, cancellationToken);
            return Ok(ApiResponse<GzOrderDetailResponse>.Ok(data, "Asuhan gizi berhasil ditutup."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<GzOrderDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Cancel", "Cancel Nutrition Order",
        Description = "Membatalkan order konsultasi gizi", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("NutritionOrder", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelGzOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CancelAsync(id, request, cancellationToken);
            return Ok(ApiResponse<GzOrderDetailResponse>.Ok(data,
                "Order konsultasi gizi berhasil dibatalkan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (NutritionForbiddenException ex) { return this.NutritionForbidden(ex); }
        catch (NutritionConflictException ex) { return this.NutritionConflict(ex); }
        catch (NutritionUnprocessableException ex) { return this.NutritionUnprocessable(ex); }
    }

    [HttpPost("{id:guid}/records")]
    [ProducesResponseType(typeof(ApiResponse<GzCareRecordResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Nutrition Care Record",
        Description = "Mencatat kunjungan ahli gizi", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("NutritionCareRecord", "Update")]
    public async Task<IActionResult> SaveCareRecord(Guid id,
        [FromBody] SaveGzCareRecordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.SaveCareRecordAsync(id, request, cancellationToken);
            return Ok(ApiResponse<GzCareRecordResponse>.Ok(data,
                "Catatan kunjungan gizi berhasil disimpan."));
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

/// <summary>
/// Pemetaan tunggal kegagalan domain modul Gizi ke kode HTTP: `403` tidak berwenang,
/// `409` benturan atau transisi tidak sah, `422` prasyarat aturan gizi.
/// </summary>
internal static class NutritionControllerResults
{
    public static ObjectResult NutritionForbidden(this ControllerBase controller,
        NutritionForbiddenException exception) =>
        controller.StatusCode(StatusCodes.Status403Forbidden,
            ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));

    public static ObjectResult NutritionConflict(this ControllerBase controller,
        NutritionConflictException exception) =>
        controller.StatusCode(StatusCodes.Status409Conflict,
            ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message,
                new { exception.Code }));

    public static ObjectResult NutritionUnprocessable(this ControllerBase controller,
        NutritionUnprocessableException exception) =>
        controller.StatusCode(StatusCodes.Status422UnprocessableEntity,
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message,
                new { exception.Code }));
}
