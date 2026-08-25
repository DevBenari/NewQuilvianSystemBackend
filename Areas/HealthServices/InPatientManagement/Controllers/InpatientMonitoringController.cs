using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.InPatientManagement.Controllers
{
    /// <summary>
    /// Daftar pantau Rawat Inap: keadaan yang perlu dibetulkan orang, bukan ditolak sistem.
    /// </summary>
    /// <remarks>
    /// <b>Satu daftar pantau yang sudah dibuka.</b> Task <c>BE-RWI-015</c> membuka
    /// <c>GET /monitoring/isolation-mismatch</c>. Empat daftar pantau lainnya —
    /// <c>pending-closures</c>, <c>closures-without-financial-clearance</c>,
    /// <c>unassigned-nurse-episodes</c>, dan <c>bed-drift</c> — milik <c>BE-RWI-029</c> dan
    /// sengaja belum ada di sini.
    ///
    /// <para>
    /// <b>Kenapa daftar pantau ada.</b> Ketika kondisi klinis berubah di tengah perawatan,
    /// pencatatannya tidak pernah ditahan. Fakta klinis dicatat lebih dulu, lalu sistem
    /// menunjukkan bahwa penempatannya perlu dibetulkan. Menahan pencatatan demi menjaga
    /// aturan penempatan adalah urutan yang terbalik — <c>RWI-RULE-012</c> bagian A aturan 7.
    /// </para>
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/inpatient-management/monitoring")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_INPATIENT",
        moduleName: "Health Service Inpatient",
        displayName: "Inpatient Monitoring",
        AreaName = "HealthServices",
        ControllerName = "InpatientMonitoring",
        Description = "Daftar pantau keadaan rawat inap yang perlu dibetulkan",
        SortOrder = 13
    )]
    [Tags("Health Services / Inpatient Management / Inpatient Monitoring")]
    public class InpatientMonitoringController : ControllerBase
    {
        private readonly InpCensusQueryService _censusQueryService;

        public InpatientMonitoringController(InpCensusQueryService censusQueryService)
        {
            _censusQueryService = censusQueryService;
        }

        /// <summary>
        /// Daftar episode yang kebutuhan isolasinya tidak cocok dengan sifat tempat tidur yang
        /// sedang ditempatinya.
        /// </summary>
        /// <remarks>
        /// Memuat dua arah sekaligus: pasien yang membutuhkan isolasi tetapi berada di tempat
        /// tidur biasa, dan pasien yang tidak membutuhkan isolasi tetapi menempati tempat
        /// tidur isolasi. Daftar yang kosong mengembalikan daftar kosong, bukan galat.
        /// </remarks>
        [HttpGet("isolation-mismatch")]
        [ProducesResponseType(typeof(ApiResponse<IsolationMismatchPagedResult>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Inpatient Monitoring", Description = "Melihat daftar pantau penempatan tidak sesuai kebutuhan isolasi", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientMonitoring", "Read")]
        public async Task<IActionResult> GetIsolationMismatch(
            [FromQuery] IsolationMismatchQuery query,
            CancellationToken cancellationToken = default)
        {
            var result = await _censusQueryService.GetIsolationMismatchAsync(query, cancellationToken);

            return Ok(ApiResponse<IsolationMismatchPagedResult>.Ok(
                result,
                "Daftar pantau penempatan tidak sesuai berhasil diambil."));
        }
    }
}
