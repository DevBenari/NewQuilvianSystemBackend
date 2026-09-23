using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers
{
    /// <summary>
    /// Jam shift perawat per unit atau bawaan — <c>BE-RWI-120</c> kriteria 4, <c>FR-KEP-060</c>, api-contract
    /// <c>keperawatan</c> 0.5.0 bagian 7.9.
    /// </summary>
    /// <remarks>
    /// <b>Arketipe: konfigurasi ganti-seluruh.</b> Jam shift hanya membagi total cairan dan <b>tidak pernah</b> menentukan
    /// siapa boleh menulis (<c>AC-KEP-093</c>). Hak akses <c>NursingShift : Read</c>/<c>: Update</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/clinical-management/nursing-shifts")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_CLINICAL",
        moduleName: "Health Service Clinical",
        displayName: "Nursing Shift",
        AreaName = "HealthServices",
        ControllerName = "NursingShift",
        Description = "Jam shift perawat untuk pembagian balance cairan",
        SortOrder = 25)]
    [Tags("Health Services / Clinical Management / Nursing Shift")]
    public class NursingShiftController : ControllerBase
    {
        private readonly DailyMonitoringService _service;

        public NursingShiftController(DailyMonitoringService service)
        {
            _service = service;
        }

        /// <summary>Shift satu unit; bila unit tidak punya shift sendiri, shift bawaan dengan <c>IsDefault = true</c>.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<NursingShiftSetResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Nursing Shift", Description = "Melihat jam shift perawat", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("NursingShift", "Read")]
        public async Task<IActionResult> Get(
            [FromQuery] Guid? serviceUnitId,
            CancellationToken cancellationToken = default)
            => Ok(ApiResponse<NursingShiftSetResponse>.Ok(
                await _service.GetShiftSetAsync(serviceUnitId, cancellationToken),
                "Jam shift berhasil diambil."));

        /// <summary>Mengganti seluruh shift satu unit atau bawaan. <c>400</c> bila tidak menutup 24 jam atau bertumpuk.</summary>
        [HttpPut]
        [ProducesResponseType(typeof(ApiResponse<NursingShiftSetResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Nursing Shift", Description = "Mengubah jam shift perawat", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("NursingShift", "Update")]
        public async Task<IActionResult> Replace(
            [FromBody] ReplaceNursingShiftSetRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.ReplaceShiftSetAsync(request, this.CurrentUserId(), cancellationToken));
    }
}
