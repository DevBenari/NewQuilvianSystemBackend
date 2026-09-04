using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.LaboratoryManagement.Controllers
{
    /// <summary>
    /// Daftar kerja laboratorium dan daftar pantau keterlambatan cito.
    ///
    /// Kedua daftar diturunkan dari pesanan, wadah, dan pemeriksaan yang sudah ada. Tidak ada
    /// tabel daftar kerja (<c>FR-04.4</c>), sehingga tidak ada pula jalur tulis pada grup ini —
    /// yang tersedia hanya dua jalur baca.
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/laboratory-management/lab-worklists")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_LABORATORY_MANAGEMENT",
        moduleName: "Health Service Laboratory Management",
        displayName: "Lab Worklist",
        AreaName = "HealthServices",
        ControllerName = "LabWorklist",
        Description = "Daftar kerja dan pemantauan keterlambatan cito laboratorium",
        SortOrder = 7
    )]
    [Tags("Health Services / Laboratory Management / Lab Worklist")]
    public class LabWorklistController : ControllerBase
    {
        private readonly LabWorklistService _labWorklistService;

        public LabWorklistController(LabWorklistService labWorklistService)
        {
            _labWorklistService = labWorklistService;
        }

        // Pekerjaan yang belum selesai, cito di urutan atas.
        //
        // Satuannya pemeriksaan, bukan pesanan: satu pesanan dapat memuat Kalium cito dan
        // Kolesterol biasa, dan hanya Kalium yang naik ke atas (AC-39).
        [HttpGet("pending")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabWorklistItemResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Worklist", Description = "Melihat daftar kerja laboratorium", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabWorklist", "Read")]
        public async Task<IActionResult> GetPending(
            [FromQuery] LabWorklistPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _labWorklistService.GetPendingAsync(query, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabWorklistItemResponse>>.Ok(
                hasil, "Daftar kerja laboratorium berhasil diambil."));
        }

        // Pesanan cito yang melewati batas waktunya, dihitung sejak wadah dinyatakan layak.
        //
        // Pemeriksaan cito yang jenisnya belum punya batas waktu tetap ditampilkan, tetapi
        // tidak dianggap terlambat dan diberi keterangan (VAL-39).
        [HttpGet("cito-overdue")]
        [ProducesResponseType(typeof(ApiResponse<PagedResult<LabCitoOverdueResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Lab Worklist", Description = "Melihat daftar pantau keterlambatan cito", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("LabWorklist", "Read")]
        public async Task<IActionResult> GetCitoOverdue(
            [FromQuery] LabWorklistPagedQuery query,
            CancellationToken cancellationToken = default)
        {
            var hasil = await _labWorklistService.GetCitoOverdueAsync(query, asOf: null, cancellationToken);

            return Ok(ApiResponse<PagedResult<LabCitoOverdueResponse>>.Ok(
                hasil, "Daftar pantau keterlambatan cito berhasil diambil."));
        }
    }
}
