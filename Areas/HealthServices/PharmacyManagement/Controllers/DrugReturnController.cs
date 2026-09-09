using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers;

/// <summary>
/// Pengembalian obat yang sudah diserahkan, kembali ke lokasi penyimpanan.
/// </summary>
/// <remarks>
/// Stok tidak bertambah saat retur diajukan, melainkan setelah diperiksa. Hak memeriksa
/// dipisah dari hak mengajukan: yang mengembalikan obat tidak sekaligus menyatakan obatnya
/// layak kembali ke rak.
/// </remarks>
[ApiController]
[Authorize]
[Route("api/v1/health-services/pharmacy-management/drug-returns")]
[Tags("Health Services / Pharmacy Management / Drug Return")]
[AccessController(
    moduleCode: "HEALTH_SERVICE_PHARMACY_MANAGEMENT",
    moduleName: "Health Service Pharmacy Management",
    displayName: "Drug Return",
    AreaName = "HealthServices",
    ControllerName = "DrugReturn",
    Description = "Retur obat pasien",
    SortOrder = 5)]
public class DrugReturnController : ControllerBase
{
    private readonly DrugReturnService _service;

    public DrugReturnController(DrugReturnService service) => _service = service;

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<DrugReturnSummaryResponse>>), StatusCodes.Status200OK)]
    [AccessAction("Read", "Read Drug Return",
        Description = "Melihat daftar retur obat", AccessType = AccessTypes.Read, SortOrder = 1)]
    [AccessPermission("DrugReturn", "Read")]
    public async Task<IActionResult> GetPaged([FromQuery] DrugReturnPagedQuery query,
        CancellationToken cancellationToken)
    {
        var data = await _service.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DrugReturnSummaryResponse>>.Ok(data,
            "Daftar retur obat berhasil diambil."));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DrugReturnDetailResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [AccessAction("Read", "Read Drug Return",
        Description = "Melihat detail retur obat", AccessType = AccessTypes.Read, SortOrder = 2)]
    [AccessPermission("DrugReturn", "Read")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var data = await _service.GetDetailAsync(id, cancellationToken);
        return data == null
            ? NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound,
                "Retur obat tidak ditemukan."))
            : Ok(ApiResponse<DrugReturnDetailResponse>.Ok(data,
                "Detail retur obat berhasil diambil."));
    }

    /// <summary>Menyusun retur sebagai draft. Belum menyentuh stok.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<DrugReturnDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Create", "Create Drug Return",
        Description = "Mengajukan retur obat", AccessType = AccessTypes.Create, SortOrder = 3)]
    [AccessPermission("DrugReturn", "Create")]
    public Task<IActionResult> Create([FromBody] CreateDrugReturnRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.CreateAsync(request, cancellationToken),
            "Draft retur obat berhasil dibuat.");

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<DrugReturnDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Drug Return",
        Description = "Mengubah draft retur obat", AccessType = AccessTypes.Update, SortOrder = 4)]
    [AccessPermission("DrugReturn", "Update")]
    public Task<IActionResult> Update(Guid id, [FromBody] UpdateDrugReturnRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.UpdateAsync(id, request, cancellationToken),
            "Draft retur obat berhasil diperbarui.");

    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<DrugReturnDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Update", "Update Drug Return",
        Description = "Mengajukan retur untuk diperiksa", AccessType = AccessTypes.Update, SortOrder = 5)]
    [AccessPermission("DrugReturn", "Update")]
    public Task<IActionResult> Submit(Guid id, [FromBody] DrugReturnCommandRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.SubmitAsync(id, request, cancellationToken),
            "Retur obat berhasil diajukan.");

    /// <summary>
    /// Memeriksa retur dan mengembalikan barang yang layak ke stok.
    /// </summary>
    /// <remarks>
    /// Setiap baris wajib diputuskan, walaupun diterima nol. Pemeriksa juga menentukan
    /// keadaan barangnya saat masuk kembali: siap pakai, karantina, atau rusak.
    /// </remarks>
    [HttpPost("{id:guid}/verify")]
    [ProducesResponseType(typeof(ApiResponse<DrugReturnDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Verify", "Verify Drug Return",
        Description = "Memeriksa retur obat dan mengembalikan stok", AccessType = AccessTypes.Update, SortOrder = 6)]
    [AccessPermission("DrugReturn", "Verify")]
    public Task<IActionResult> Verify(Guid id, [FromBody] VerifyDrugReturnRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.VerifyAsync(id, request, cancellationToken),
            "Retur obat berhasil diperiksa dan stok telah diperbarui.");

    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(ApiResponse<DrugReturnDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Verify", "Verify Drug Return",
        Description = "Menolak retur obat", AccessType = AccessTypes.Update, SortOrder = 7)]
    [AccessPermission("DrugReturn", "Verify")]
    public Task<IActionResult> Reject(Guid id, [FromBody] DrugReturnReasonRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.RejectAsync(id, request, cancellationToken),
            "Retur obat berhasil ditolak.");

    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(ApiResponse<DrugReturnDetailResponse>), StatusCodes.Status200OK)]
    [AccessAction("Cancel", "Cancel Drug Return",
        Description = "Membatalkan retur obat", AccessType = AccessTypes.Update, SortOrder = 8)]
    [AccessPermission("DrugReturn", "Cancel")]
    public Task<IActionResult> Cancel(Guid id, [FromBody] DrugReturnReasonRequest request,
        CancellationToken cancellationToken) =>
        Run(() => _service.CancelAsync(id, request, cancellationToken),
            "Retur obat berhasil dibatalkan.");

    private async Task<IActionResult> Run(Func<Task<DrugReturnDetailResponse>> action, string message)
    {
        try
        {
            var data = await action();
            return Ok(ApiResponse<DrugReturnDetailResponse>.Ok(data, message));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<object>.Fail(StatusCodes.Status404NotFound, ex.Message));
        }
        catch (DrugReturnForbiddenException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                ApiResponse<object>.Fail(StatusCodes.Status403Forbidden, ex.Message));
        }
        catch (DrugReturnConflictException ex)
        {
            return StatusCode(StatusCodes.Status409Conflict,
                ApiResponse<object>.Fail(StatusCodes.Status409Conflict, ex.Message, new { ex.Code }));
        }
        catch (DrugReturnUnprocessableException ex)
        {
            return StatusCode(StatusCodes.Status422UnprocessableEntity,
                ApiResponse<object>.Fail(StatusCodes.Status422UnprocessableEntity, ex.Message,
                    new { ex.Code }));
        }
    }
}
