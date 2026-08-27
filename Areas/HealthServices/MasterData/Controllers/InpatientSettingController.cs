using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.DTOs;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Models;
using QuilvianSystemBackend.Areas.HealthServices.MasterData.Services;
using QuilvianSystemBackend.Attributes;
using QuilvianSystemBackend.Constants;
using QuilvianSystemBackend.Responses;
using QuilvianSystemBackend.Services.Logging;
using System.Security.Claims;

namespace QuilvianSystemBackend.Areas.HealthServices.MasterData.Controllers
{
    /// <summary>
    /// Layar pengaturan Rawat Inap. Admin mengubah batas waktu pemesanan tempat tidur, umur
    /// episode Draft, target pengkajian, ambang daftar pantau, dan awalan nomor episode dari
    /// sini — tanpa satu baris kode pun disentuh.
    /// </summary>
    /// <remarks>
    /// Controller ini sengaja TIDAK punya endpoint menambah baris. Tabel pengaturan dipakai
    /// sebagai satu baris tunggal berkode <c>DEFAULT</c>; baris kedua akan membuat modul
    /// membaca angka yang berbeda dari yang disetel admin pada layar ini.
    /// </remarks>
    [ApiController]
    [Authorize]
    [Route("api/v1/health-services/master-data/inpatient-settings")]
    [AccessController(
        moduleCode: "HEALTH_SERVICE_MASTER_DATA",
        moduleName: "Health Service Master Data",
        displayName: "Inpatient Setting",
        AreaName = "HealthServices",
        ControllerName = "InpatientSetting",
        Description = "Mengelola pengaturan operasional modul Rawat Inap",
        SortOrder = 40
    )]
    [Tags("Health Services / Master Data / Inpatient Setting")]
    public class InpatientSettingController : ControllerBase
    {
        private const string LogCategory = "HealthServices.MasterData.Inpatient";

        private readonly InpatientSettingService _inpatientSettingService;
        private readonly LoggerService _loggerService;

        public InpatientSettingController(
            InpatientSettingService inpatientSettingService,
            LoggerService loggerService)
        {
            _inpatientSettingService = inpatientSettingService;
            _loggerService = loggerService;
        }

        /// <summary>Membaca pengaturan Rawat Inap yang berlaku.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse<InpatientSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Read", "Read Inpatient Setting", Description = "Melihat pengaturan Rawat Inap", AccessType = AccessTypes.Read, SortOrder = 1)]
        [AccessPermission("InpatientSetting", "Read")]
        public async Task<IActionResult> GetEffective(CancellationToken cancellationToken = default)
        {
            var entity = await _inpatientSettingService.GetEffectiveAsync(cancellationToken);

            if (entity == null)
            {
                return NotFound(ApiResponse<object>.Fail(
                    StatusCodes.Status404NotFound,
                    "Pengaturan Rawat Inap belum terisi. Jalankan seeder master Rawat Inap " +
                    "atau isi pengaturannya lebih dulu."));
            }

            return Ok(ApiResponse<InpatientSettingResponse>.Ok(
                ToResponse(entity),
                "Pengaturan Rawat Inap berhasil diambil."));
        }

        /// <summary>Mengubah nilai pengaturan Rawat Inap.</summary>
        /// <remarks>
        /// Nilai baru berlaku pada pembacaan berikutnya, tanpa aplikasi dinyalakan ulang.
        ///
        /// <b>Contoh.</b> Admin mengubah <c>BedReservationMinutes</c> dari 120 menjadi 180
        /// pada pukul 09:00. Pemesanan tempat tidur yang dibuat pukul 08:50 tetap mengunci
        /// 120 menit, karena batas waktunya sudah ditetapkan saat pemesanan itu dibuat.
        /// Pemesanan yang dibuat pukul 09:05 mengunci 180 menit.
        /// </remarks>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(ApiResponse<InpatientSettingResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
        [AccessAction("Update", "Update Inpatient Setting", Description = "Mengubah pengaturan Rawat Inap", AccessType = AccessTypes.Update, SortOrder = 3)]
        [AccessPermission("InpatientSetting", "Update")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateInpatientSettingRequest request,
            CancellationToken cancellationToken = default)
        {
            var actorUserId = GetCurrentUserId();

            var result = await _inpatientSettingService.UpdateAsync(
                id,
                request,
                actorUserId,
                cancellationToken);

            switch (result.Status)
            {
                case InpatientSettingUpdateStatus.NotFound:
                    return NotFound(ApiResponse<object>.Fail(
                        StatusCodes.Status404NotFound,
                        result.Message));

                case InpatientSettingUpdateStatus.Invalid:
                    return BadRequest(ApiResponse<object>.Fail(
                        StatusCodes.Status400BadRequest,
                        result.Message));
            }

            await _loggerService.InfoAsync(
                LogCategory,
                "InpatientSetting.Update",
                "Mengubah pengaturan Rawat Inap.",
                new { EntityId = id, Controller = "InpatientSetting", Action = "Update" }
            );

            return Ok(ApiResponse<InpatientSettingResponse>.Ok(
                ToResponse(result.Entity!),
                result.Message));
        }

        private static InpatientSettingResponse ToResponse(MstInpatientSetting entity)
            => new()
            {
                Id = entity.Id,
                Code = entity.Code,
                Name = entity.Name,
                BedReservationMinutes = entity.BedReservationMinutes,
                DraftEpisodeExpiryHours = entity.DraftEpisodeExpiryHours,
                InitialAssessmentTargetHours = entity.InitialAssessmentTargetHours,
                ProgressNoteVerificationTargetHours = entity.ProgressNoteVerificationTargetHours,
                PendingClosureThresholdHours = entity.PendingClosureThresholdHours,
                EpisodeNumberPrefix = entity.EpisodeNumberPrefix,
                IsDefault = entity.IsDefault,
                IsActive = entity.IsActive,
                Notes = entity.Notes,
                CreateDateTime = entity.CreateDateTime,
                UpdateDateTime = entity.UpdateDateTime
            };

        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id) ? id : Guid.Empty;
        }
    }
}
