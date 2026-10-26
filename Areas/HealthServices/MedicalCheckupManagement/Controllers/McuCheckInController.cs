using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using P = QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Constants.MedicalCheckupPermissions;

namespace QuilvianSystemBackend.Areas.HealthServices.MedicalCheckupManagement.Controllers
{
    /// <summary>
    /// Check-in admisi MCU — <c>MCU-API-v1</c> grup Mcu Check In (<c>BE-MCU-013</c>).
    /// </summary>
    /// <remarks>
    /// Resource-nya tetap <c>McuOrder</c> (action <c>CheckIn</c>), sama dengan <c>McuOrderController</c>;
    /// nama tampil dan keterangannya disamakan karena <c>AccessMenuSeeder</c> mendaftarkan keduanya pada
    /// satu baris resource. Ganti dokter MCU (<c>PATCH mcu-doctor</c>) dari <c>BE-MCU-032</c>.
    /// Action sengaja tanpa <c>&lt;summary&gt;</c> XML.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/medical-checkup-management/orders/{id:guid}")]
    [AccessController(
        moduleCode: P.ModuleCode,
        moduleName: P.ModuleName,
        displayName: "Mcu Order",
        AreaName = P.AreaName,
        ControllerName = P.Order.Resource,
        Description = "Order MCU: Daftar Pasien MCU, detail, booking, jadwal ulang, dan batal",
        SortOrder = 4
    )]
    [Tags("Health Services / Medical Checkup Management / Mcu Check In")]
    public class McuCheckInController : ControllerBase
    {
        private readonly McuCheckInService _service;
        private readonly LoggerService _logger;

        public McuCheckInController(McuCheckInService service, LoggerService logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet("patient-matches")]
        [ProducesResponseType(typeof(ApiResponse<McuPatientMatchResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Order.CheckIn, "Check In Mcu Order", Description = "Mencocokkan peserta MCU dengan data pasien (NIK dan tanggal lahir)", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Order.Resource, P.Order.CheckIn)]
        public async Task<IActionResult> GetPatientMatches(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetPatientMatchesAsync(id, cancellationToken), "Pencocokan pasien berhasil diambil.");

        [HttpGet("check-in/preview")]
        [ProducesResponseType(typeof(ApiResponse<McuCheckInPreviewResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction(P.Order.CheckIn, "Check In Mcu Order", Description = "Melihat persyaratan, harga, dan dokter jaga sebelum check-in", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Order.Resource, P.Order.CheckIn)]
        public async Task<IActionResult> GetPreview(Guid id, CancellationToken cancellationToken = default) =>
            McuHttp.Map(this, await _service.GetPreviewAsync(id, cancellationToken), "Pratinjau check-in berhasil diambil.");

        [HttpPost("check-in")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.CheckIn, "Check In Mcu Order", Description = "Check-in peserta MCU: kunjungan, baris paket, dokter MCU, dan persyaratan", AccessType = AccessTypes.Update, SortOrder = 5)]
        [AccessPermission(P.Order.Resource, P.Order.CheckIn)]
        public async Task<IActionResult> CheckIn(Guid id, [FromBody] McuCheckInRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.CheckInAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Check-in MCU berhasil.", _logger, P.Order.Resource, P.Order.CheckIn,
                x => $"Check-in MCU {x.OrderNumber}: kunjungan {x.EncounterNumber}, dokter MCU {x.McuDoctorName}, " +
                     $"pencocokan {x.PatientMatchMethodLabel}.");

        [HttpPatch("mcu-doctor")]
        [ProducesResponseType(typeof(ApiResponse<McuOrderDetailResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
        [AccessAction(P.Order.ReassignDoctor, "Reassign Mcu Doctor", Description = "Kepala unit MCU mengganti dokter MCU yang ditetapkan beserta alasannya", AccessType = AccessTypes.Update, SortOrder = 6)]
        [AccessPermission(P.Order.Resource, P.Order.ReassignDoctor)]
        public async Task<IActionResult> ReassignDoctor(Guid id, [FromBody] ReassignMcuDoctorRequest request, CancellationToken cancellationToken = default) =>
            await McuHttp.RespondAsync(this,
                await _service.ReassignDoctorAsync(id, request, McuHttp.ActorId(User), cancellationToken),
                "Dokter MCU berhasil diganti.", _logger, P.Order.Resource, P.Order.ReassignDoctor,
                // Alasan penggantian tidak dicatat di log aplikasi.
                x => $"Dokter MCU order {x.OrderNumber} diganti menjadi {x.McuDoctorName}.");
    }
}
