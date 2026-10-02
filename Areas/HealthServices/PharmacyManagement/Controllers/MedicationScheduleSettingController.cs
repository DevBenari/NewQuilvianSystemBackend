using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.ClinicalManagement.Controllers;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;

namespace QuilvianSystemBackend.Areas.HealthServices.PharmacyManagement.Controllers
{
    /// <summary>
    /// Konfigurasi MAR milik Farmasi — <c>BE-RWI-114</c> kriteria 5 dan 6, api-contract <c>keperawatan</c> 0.5.0 bagian 7.13.
    /// </summary>
    /// <remarks>
    /// <b>Arketipe: konfigurasi ganti-seluruh</b>, bukan master data sembilan endpoint: satu kode frekuensi satu set jam
    /// per unit atau bawaan, dan satu baris pengaturan aktif. Hak akses <c>MedicationScheduleSetting : Read</c>/<c>: Update</c>.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/pharmacy-management/medication-schedule-settings")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_PHARMACY",
        moduleName: "Health Service Pharmacy",
        displayName: "Medication Schedule Setting",
        AreaName = "HealthServices",
        ControllerName = "MedicationScheduleSetting",
        Description = "Jam pemberian obat per kode frekuensi dan pengaturan MAR",
        SortOrder = 15)]
    [Tags("Health Services / Pharmacy Management / Medication Schedule Setting")]
    public class MedicationScheduleSettingController : ControllerBase
    {
        private readonly MedicationAdministrationService _service;

        public MedicationScheduleSettingController(MedicationAdministrationService service)
        {
            _service = service;
        }

        [HttpGet("schedule-times")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicationScheduleTimeSetResponse>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medication Schedule Setting", Description = "Melihat jam pemberian obat dan pengaturan MAR", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationScheduleSetting", "Read")]
        public async Task<IActionResult> GetScheduleTimes(
            [FromQuery] Guid? serviceUnitId,
            [FromQuery] string? frequencyCode,
            CancellationToken cancellationToken = default)
            => Ok(ApiResponse<List<MedicationScheduleTimeSetResponse>>.Ok(
                await _service.GetScheduleTimesAsync(serviceUnitId, frequencyCode, cancellationToken),
                "Jam pemberian obat berhasil diambil."));

        /// <summary>Mengganti slot satu kode frekuensi untuk satu unit atau bawaan. Dosis yang sudah terbentuk tidak dipindahkan.</summary>
        [HttpPut("schedule-times")]
        [ProducesResponseType(typeof(ApiResponse<List<MedicationScheduleTimeSetResponse>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Medication Schedule Setting", Description = "Mengubah jam pemberian obat dan pengaturan MAR", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicationScheduleSetting", "Update")]
        public async Task<IActionResult> ReplaceScheduleTimes(
            [FromBody] ReplaceMedicationScheduleTimesRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.ReplaceScheduleTimesAsync(request, this.CurrentUserId(), cancellationToken));

        [HttpGet("frequency-codes-without-schedule")]
        [ProducesResponseType(typeof(ApiResponse<List<string>>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medication Schedule Setting", Description = "Melihat jam pemberian obat dan pengaturan MAR", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationScheduleSetting", "Read")]
        public async Task<IActionResult> GetFrequencyCodesWithoutSchedule(CancellationToken cancellationToken = default)
            => Ok(ApiResponse<List<string>>.Ok(
                await _service.GetFrequencyCodesWithoutScheduleAsync(cancellationToken),
                "Kode frekuensi tanpa jadwal berhasil diambil."));

        [HttpGet("administration-setting")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationSettingResponse>), StatusCodes.Status200OK)]
        [AccessAction("Read", "Read Medication Schedule Setting", Description = "Melihat jam pemberian obat dan pengaturan MAR", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("MedicationScheduleSetting", "Read")]
        public async Task<IActionResult> GetAdministrationSetting(CancellationToken cancellationToken = default)
            => Ok(ApiResponse<MedicationAdministrationSettingResponse>.Ok(
                await _service.GetSettingAsync(cancellationToken),
                "Pengaturan MAR berhasil diambil."));

        [HttpPut("administration-setting")]
        [ProducesResponseType(typeof(ApiResponse<MedicationAdministrationSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [AccessAction("Update", "Update Medication Schedule Setting", Description = "Mengubah jam pemberian obat dan pengaturan MAR", AccessType = AccessTypes.Update, SortOrder = 2)]
        [AccessPermission("MedicationScheduleSetting", "Update")]
        public async Task<IActionResult> UpdateAdministrationSetting(
            [FromBody] UpdateMedicationAdministrationSettingRequest request,
            CancellationToken cancellationToken = default)
            => this.ToActionResult(await _service.UpdateSettingAsync(request, this.CurrentUserId(), cancellationToken));
    }
}
