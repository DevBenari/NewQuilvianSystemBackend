using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Permintaan stok barang dan obat dari unit layanan kepada gudang farmasi.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/stock-requests")]
[Tags("Health Services / Pharmacy Management / Stock Request")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Stock Request",
    AreaName = "HealthServices",
    ControllerName = "StockRequest",
    Description = "Permintaan stok barang dan obat",
    SortOrder = 1)]
public class StockRequestController : ControllerBase
{
    private readonly StockRequestService _service;

    public StockRequestController(StockRequestService service) => _service = service;

    /// <summary>
    /// Riwayat permintaan obat, dengan pencarian dan penyaringan.
    /// </summary>
    /// <remarks>
    /// Pencarian mengenai nomor permintaan, nama unit peminta, dan nama serta kode obat
    /// yang ada di dalamnya — sehingga petugas dapat menemukan permintaan lewat obat yang
    /// diingatnya, bukan hanya lewat nomor.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<StockRequestSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Stock Request",
        Description = "Melihat riwayat permintaan obat", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("StockRequest", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] StockRequestPagedQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<StockRequestSummaryResponse>>.Ok(data,
            "Riwayat permintaan obat berhasil diambil."));
    }

    /// <summary>Rincian satu permintaan beserta itemnya dan riwayat statusnya.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Stock Request",
        Description = "Melihat detail permintaan stok barang atau obat", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("StockRequest", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetDetailAsync(id, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Permintaan stok tidak ditemukan."))
            : Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Detail permintaan stok berhasil diambil."));
    }

    /// <summary>Membuat permintaan beserta item dan jumlahnya. Dibuat berstatus Draft.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Stock Request",
        Description = "Membuat permintaan stok barang atau obat", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("StockRequest", "Create")]
    public async Task<IActionResult> Create([FromBody] CreateStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CreateAsync(request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil dibuat."));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>
    /// Mengubah permintaan yang diizinkan, yaitu yang masih berstatus Draft.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Stock Request",
        Description = "Mengubah permintaan stok barang atau obat", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("StockRequest", "Update")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.UpdateAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil diperbarui."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    /// <summary>Mengirim permintaan ke gudang. Sesudah ini isinya tidak dapat diubah.</summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Stock Request",
        Description = "Mengirim permintaan stok ke gudang", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("StockRequest", "Update")]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.SubmitAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil dikirim."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<StockRequestDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Cancel", "Cancel Stock Request",
        Description = "Membatalkan permintaan stok", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("StockRequest", "Cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelStockRequestRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await _service.CancelAsync(id, request, cancellationToken);
            return Ok(ApiResponse<StockRequestDetailResponse>.Ok(data,
                "Permintaan stok berhasil dibatalkan."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (StockRequestForbiddenException ex) { return this.StockRequestForbidden(ex); }
        catch (StockRequestConflictException ex) { return this.StockRequestConflict(ex); }
        catch (StockRequestUnprocessableException ex) { return this.StockRequestUnprocessable(ex); }
    }
}

/// <summary>
/// Pemetaan tunggal kegagalan domain permintaan stok ke kode HTTP: `403` tidak berwenang,
/// `409` benturan atau transisi tidak sah, `422` prasyarat aturan.
/// </summary>
internal static class StockRequestControllerResults
{
    public static ObjectResult StockRequestForbidden(this ControllerBase controller,
        StockRequestForbiddenException exception) =>
        controller.StatusCode(StatusCodes.Status403Forbidden,
            ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, exception.Message));

    public static ObjectResult StockRequestConflict(this ControllerBase controller,
        StockRequestConflictException exception) =>
        controller.StatusCode(StatusCodes.Status409Conflict,
            ApiResponse<object>.Fail(StatusCodes.Status409Conflict, exception.Message,
                new { exception.Code }));

    public static ObjectResult StockRequestUnprocessable(this ControllerBase controller,
        StockRequestUnprocessableException exception) =>
        controller.StatusCode(StatusCodes.Status422UnprocessableEntity,
            ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, exception.Message,
                new { exception.Code }));
}
