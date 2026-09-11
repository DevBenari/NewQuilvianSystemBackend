using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Pemakaian obat dan alat kesehatan untuk pasien.
/// </summary>
/// <remarks>
/// Pemakaian disusun sebagai draft lebih dahulu, lalu dicatat. Stok berkurang pada saat
/// pencatatan, bukan saat draft dibuat. Sesudah dicatat, transaksinya berhenti pada keadaan
/// belum ditagihkan — keputusan menagih milik Billing, bukan Farmasi.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/drug-usages")]
[Tags("Health Services / Pharmacy Management / Drug Usage")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Drug Usage",
    AreaName = "HealthServices",
    ControllerName = "DrugUsage",
    Description = "Pemakaian obat dan alat kesehatan pasien",
    SortOrder = 4)]
public class DrugUsageController : ControllerBase
{
    private readonly DrugUsageService _service;

    public DrugUsageController(DrugUsageService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugUsageSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Usage",
        Description = "Melihat daftar pemakaian obat", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("DrugUsage", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] DrugUsagePagedQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugUsageSummaryResponse>>.Ok(data,
            "Daftar pemakaian obat berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DrugUsageDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Drug Usage",
        Description = "Melihat detail pemakaian obat", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("DrugUsage", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetDetailAsync(id, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Pemakaian obat tidak ditemukan."))
            : Ok(ApiResponse<DrugUsageDetailResponse>.Ok(data,
                "Detail pemakaian obat berhasil diambil."));
    }

    /// <summary>Menyusun pemakaian sebagai draft. Belum menyentuh stok.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DrugUsageDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Drug Usage",
        Description = "Menyusun pemakaian obat pasien", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("DrugUsage", "Create")]
    public Task<IActionResult> Create([FromBody] CreateDrugUsageRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.CreateAsync(request, cancellationToken),
            "Draft pemakaian obat berhasil dibuat.");

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DrugUsageDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Drug Usage",
        Description = "Mengubah draft pemakaian obat", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("DrugUsage", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateDrugUsageRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.UpdateAsync(id, request, cancellationToken),
            "Draft pemakaian obat berhasil diperbarui.");

    /// <summary>
    /// Mencatat pemakaian: stok berkurang dan transaksinya siap ditagihkan.
    /// </summary>
    /// <remarks>
    /// Batch diambil menurut kedaluwarsa terdekat dan disimpan per baris, sehingga obat yang
    /// sudah sampai ke pasien tetap dapat ditelusuri bila batchnya kelak ditarik.
    /// </remarks>
    [HttpPost("{id:guid}/record")]
    [ProducesResponseType(typeof(ApiResponse<DrugUsageDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Record", "Record Drug Usage",
        Description = "Mencatat pemakaian obat dan mengurangi stok", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("DrugUsage", "Record")]
    public Task<IActionResult> Record(Guid id, [FromBody] DrugUsageCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.RecordAsync(id, request, cancellationToken),
            "Pemakaian obat berhasil dicatat dan stok telah berkurang.");

    /// <summary>Membatalkan draft. Yang sudah dicatat tidak dapat dibatalkan dari sini.</summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<DrugUsageDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Cancel", "Cancel Drug Usage",
        Description = "Membatalkan draft pemakaian obat", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("DrugUsage", "Cancel")]
    public Task<IActionResult> Cancel(Guid id, [FromBody] CancelDrugUsageRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.CancelAsync(id, request, cancellationToken),
            "Draft pemakaian obat berhasil dibatalkan.");

    private async Task<IActionResult> Run(Func<Task<DrugUsageDetailResponse>> action, string message)
    {
        try
        {
            var data = await action();
            return Ok(ApiResponse<DrugUsageDetailResponse>.Ok(data, message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (DrugUsageForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
        }
        catch (DrugUsageConflictException ex)
        {
            return StatusCode(StatusCodes.Status409Conflict,
                ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message, new { ex.Code }));
        }
        catch (DrugUsageUnprocessableException ex)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity,
                ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message,
                    new { ex.Code }));
        }
    }
}
